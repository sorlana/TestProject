using System.Security.Cryptography;
using Services.Abstractions.Interfaces;
using Services.Contracts.Results;
using Services.Repositories.Abstractions.Interfaces;
using Domain.Entities.Entities;

namespace Services.Implementations.Implementations;

/// <summary>
/// Реализация сервиса подтверждения телефона
/// Обеспечивает отправку и проверку SMS кодов подтверждения
/// </summary>
public class PhoneVerificationService : IPhoneVerificationService
{
    private readonly IPhoneVerificationCodeRepository _codeRepository;
    private readonly ISmsService _smsService;
    
    // Константы для настройки сервиса
    private const int CodeLength = 6;
    private const int CodeExpirationMinutes = 10;
    private const int MaxSendAttemptsPerHour = 3;

    public PhoneVerificationService(
        IPhoneVerificationCodeRepository codeRepository,
        ISmsService smsService)
    {
        _codeRepository = codeRepository;
        _smsService = smsService;
    }

    /// <summary>
    /// Отправка кода подтверждения на телефон
    /// </summary>
    public async Task<SendCodeResult> SendVerificationCodeAsync(
        string phoneNumber, 
        CancellationToken cancellationToken = default)
    {
        // Проверка rate limiting: максимум 3 отправки в час
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var sentCount = await _codeRepository.GetSentCountSinceAsync(phoneNumber, oneHourAgo, cancellationToken);
        
        if (sentCount >= MaxSendAttemptsPerHour)
        {
            return new SendCodeResult
            {
                Success = false,
                Message = "Превышен лимит отправки кодов. Попробуйте позже.",
                Errors = new[] { "Вы можете запросить код не более 3 раз в час." }
            };
        }

        // Генерация 6-значного числового кода
        var code = GenerateVerificationCode();
        
        // Создание записи кода в базе данных
        var verificationCode = new PhoneVerificationCode
        {
            PhoneNumber = phoneNumber,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
            AttemptCount = 0
        };
        
        await _codeRepository.CreateAsync(verificationCode, cancellationToken);
        
        // Отправка SMS с кодом
        var message = $"Ваш код подтверждения: {code}. Код действителен {CodeExpirationMinutes} минут.";
        var smsSent = await _smsService.SendSmsAsync(phoneNumber, message, cancellationToken);
        
        if (!smsSent)
        {
            return new SendCodeResult
            {
                Success = false,
                Message = "Не удалось отправить SMS. Попробуйте позже.",
                Errors = new[] { "Ошибка отправки SMS сообщения." }
            };
        }
        
        return new SendCodeResult
        {
            Success = true,
            Message = "Код подтверждения отправлен на ваш телефон.",
            ExpiresAt = verificationCode.ExpiresAt
        };
    }

    /// <summary>
    /// Проверка кода подтверждения
    /// </summary>
    public async Task<VerificationResult> VerifyCodeAsync(
        string phoneNumber, 
        string code, 
        CancellationToken cancellationToken = default)
    {
        // Получение активного кода для номера телефона
        var verificationCode = await _codeRepository.GetActiveCodeAsync(phoneNumber, cancellationToken);
        
        if (verificationCode == null)
        {
            return new VerificationResult
            {
                Success = false,
                IsVerified = false,
                Message = "Код подтверждения не найден.",
                Errors = new[] { "Запросите новый код подтверждения." }
            };
        }
        
        // Проверка срока действия кода (10 минут)
        if (verificationCode.ExpiresAt < DateTime.UtcNow)
        {
            return new VerificationResult
            {
                Success = false,
                IsVerified = false,
                Message = "Код подтверждения истек.",
                Errors = new[] { "Запросите новый код подтверждения." }
            };
        }
        
        // Проверка, не был ли код уже использован
        if (verificationCode.IsUsed)
        {
            return new VerificationResult
            {
                Success = false,
                IsVerified = false,
                Message = "Код подтверждения уже использован.",
                Errors = new[] { "Запросите новый код подтверждения." }
            };
        }
        
        // Увеличение счетчика попыток
        verificationCode.AttemptCount++;
        await _codeRepository.UpdateAsync(verificationCode, cancellationToken);
        
        // Проверка корректности кода
        if (verificationCode.Code != code)
        {
            return new VerificationResult
            {
                Success = false,
                IsVerified = false,
                Message = "Неверный код подтверждения.",
                Errors = new[] { "Проверьте правильность введенного кода." }
            };
        }
        
        // Пометка кода как использованного
        await _codeRepository.MarkAsUsedAsync(verificationCode.Id, cancellationToken);
        
        return new VerificationResult
        {
            Success = true,
            IsVerified = true,
            Message = "Телефон успешно подтвержден."
        };
    }

    /// <summary>
    /// Повторная отправка кода подтверждения
    /// Генерирует новый код и отправляет его
    /// </summary>
    public async Task<SendCodeResult> ResendCodeAsync(
        string phoneNumber, 
        CancellationToken cancellationToken = default)
    {
        // Повторная отправка использует ту же логику, что и первичная отправка
        // Rate limiting проверяется внутри SendVerificationCodeAsync
        return await SendVerificationCodeAsync(phoneNumber, cancellationToken);
    }

    /// <summary>
    /// Генерация 6-значного числового кода с использованием криптографически безопасного генератора
    /// </summary>
    private static string GenerateVerificationCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        
        // Генерируем число от 0 до 999999
        var randomNumber = BitConverter.ToUInt32(randomBytes, 0) % 1000000;
        
        // Форматируем как 6-значное число с ведущими нулями
        return randomNumber.ToString("D6");
    }
}
