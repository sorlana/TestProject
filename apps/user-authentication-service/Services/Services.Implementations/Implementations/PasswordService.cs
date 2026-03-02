using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Services.Abstractions.Interfaces;
using Services.Contracts.Enums;

namespace Services.Implementations.Implementations;

/// <summary>
/// Реализация сервиса работы с паролями
/// Обеспечивает генерацию, валидацию и оценку надежности паролей
/// </summary>
public class PasswordService : IPasswordService
{
    // Наборы символов для генерации пароля
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    
    // Длина генерируемого пароля
    private const int PasswordLength = 16;
    
    // Минимальные требования к паролю
    private const int MinPasswordLength = 8;

    /// <summary>
    /// Генерация надежного пароля
    /// Генерирует пароль длиной 16 символов с использованием криптографически случайных чисел
    /// </summary>
    public string GenerateSecurePassword()
    {
        // Объединяем все наборы символов
        var allChars = UppercaseChars + LowercaseChars + DigitChars + SpecialChars;
        
        var password = new StringBuilder(PasswordLength);
        
        using var rng = RandomNumberGenerator.Create();
        
        // Гарантируем наличие хотя бы одного символа каждого типа
        password.Append(GetRandomChar(UppercaseChars, rng));
        password.Append(GetRandomChar(LowercaseChars, rng));
        password.Append(GetRandomChar(DigitChars, rng));
        password.Append(GetRandomChar(SpecialChars, rng));
        
        // Заполняем оставшиеся позиции случайными символами из всех наборов
        for (int i = 4; i < PasswordLength; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }
        
        // Перемешиваем символы для случайного порядка
        return ShuffleString(password.ToString(), rng);
    }

    /// <summary>
    /// Валидация пароля по правилам
    /// Проверяет минимальную длину, наличие заглавной буквы и цифры
    /// </summary>
    public Task<bool> ValidatePasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(false);
        }

        // Проверка минимальной длины (минимум 8 символов)
        if (password.Length < MinPasswordLength)
        {
            return Task.FromResult(false);
        }

        // Проверка наличия хотя бы одной заглавной буквы
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return Task.FromResult(false);
        }

        // Проверка наличия хотя бы одной цифры
        if (!Regex.IsMatch(password, @"\d"))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    /// Оценка надежности пароля
    /// Анализирует длину, разнообразие символов и сложность пароля
    /// </summary>
    public PasswordStrength EvaluatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return PasswordStrength.VeryWeak;
        }

        int score = 0;

        // Оценка длины пароля
        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        // Проверка наличия заглавных букв
        if (Regex.IsMatch(password, @"[A-Z]"))
        {
            score++;
        }

        // Проверка наличия строчных букв
        if (Regex.IsMatch(password, @"[a-z]"))
        {
            score++;
        }

        // Проверка наличия цифр
        if (Regex.IsMatch(password, @"\d"))
        {
            score++;
        }

        // Проверка наличия специальных символов
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]"))
        {
            score++;
        }

        // Проверка разнообразия символов (не повторяющиеся последовательности)
        if (!Regex.IsMatch(password, @"(.)\1{2,}"))
        {
            score++;
        }

        // Определение уровня надежности на основе набранных баллов
        return score switch
        {
            <= 2 => PasswordStrength.VeryWeak,
            3 or 4 => PasswordStrength.Weak,
            5 or 6 => PasswordStrength.Medium,
            7 => PasswordStrength.Strong,
            _ => PasswordStrength.VeryStrong
        };
    }

    /// <summary>
    /// Получение случайного символа из набора с использованием криптографически безопасного генератора
    /// </summary>
    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomIndex = BitConverter.ToUInt32(randomBytes, 0) % chars.Length;
        return chars[(int)randomIndex];
    }

    /// <summary>
    /// Перемешивание строки с использованием алгоритма Fisher-Yates
    /// </summary>
    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        int n = array.Length;
        
        for (int i = n - 1; i > 0; i--)
        {
            var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            int j = (int)(BitConverter.ToUInt32(randomBytes, 0) % (i + 1));
            
            // Обмен элементов
            (array[i], array[j]) = (array[j], array[i]);
        }
        
        return new string(array);
    }
}
