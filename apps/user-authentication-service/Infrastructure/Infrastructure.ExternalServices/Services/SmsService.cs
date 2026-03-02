using Infrastructure.ExternalServices.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.ExternalServices.Services;

/// <summary>
/// Сервис отправки SMS через внешнего провайдера (SMS.ru)
/// </summary>
public class SmsService : ISmsService
{
    private readonly SmsSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SmsService> _logger;

    public SmsService(
        IOptions<SmsSettings> settings,
        HttpClient httpClient,
        ILogger<SmsService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        // Настройка HttpClient
        _httpClient.BaseAddress = new Uri(_settings.ApiUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    /// <summary>
    /// Отправка SMS сообщения
    /// </summary>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отправка SMS на номер {PhoneNumber}", MaskPhoneNumber(phoneNumber));

            // Формирование запроса к SMS.ru API
            // Формат: /sms/send?api_id=YOUR_API_KEY&to=PHONE&msg=MESSAGE&json=1
            var requestUrl = $"/sms/send?api_id={_settings.ApiKey}&to={phoneNumber}&msg={Uri.EscapeDataString(message)}&json=1";

            if (!string.IsNullOrEmpty(_settings.SenderName))
            {
                requestUrl += $"&from={Uri.EscapeDataString(_settings.SenderName)}";
            }

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ошибка HTTP при отправке SMS: {StatusCode}", response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Ответ от SMS провайдера: {Response}", responseContent);

            // Парсинг ответа от SMS.ru
            var smsResponse = JsonSerializer.Deserialize<SmsRuResponse>(responseContent);

            if (smsResponse?.Status == "OK")
            {
                _logger.LogInformation("SMS успешно отправлено на номер {PhoneNumber}, ID сообщения: {MessageId}", 
                    MaskPhoneNumber(phoneNumber), smsResponse.Sms?.FirstOrDefault()?.SmsId);
                return true;
            }
            else
            {
                _logger.LogWarning("Ошибка при отправке SMS: {StatusCode} - {StatusText}", 
                    smsResponse?.StatusCode, smsResponse?.StatusText);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP запроса при отправке SMS на номер {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Таймаут при отправке SMS на номер {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при отправке SMS на номер {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
    }

    /// <summary>
    /// Маскирование номера телефона для логирования (показываем только последние 4 цифры)
    /// </summary>
    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";

        return $"****{phoneNumber[^4..]}";
    }

    /// <summary>
    /// Модель ответа от SMS.ru API
    /// </summary>
    private class SmsRuResponse
    {
        public string? Status { get; set; }
        public string? StatusCode { get; set; }
        public string? StatusText { get; set; }
        public List<SmsRuMessage>? Sms { get; set; }
    }

    /// <summary>
    /// Модель сообщения в ответе от SMS.ru
    /// </summary>
    private class SmsRuMessage
    {
        public string? SmsId { get; set; }
        public string? Status { get; set; }
        public string? StatusCode { get; set; }
        public string? StatusText { get; set; }
    }
}
