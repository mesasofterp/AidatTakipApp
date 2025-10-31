using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StudentApp.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly string _apiId;
    private readonly string _apiKey;
    private readonly string _sender;
    private readonly string _messageType;
    private readonly string _messageContentType;
    private readonly IHttpClientFactory _httpClientFactory;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        var section = configuration.GetSection("SmsApi");
        _apiId = section["ApiId"]!;
        _apiKey = section["ApiKey"]!;
        _sender = section["Sender"]!;
        _messageType = section["MessageType"] ?? "normal";
        _messageContentType = section["MessageContentType"] ?? "bilgi";
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// SMS gönderme metodu - Buraya kendi SMS API çağrınızı yazacaksınız
    /// </summary>
    /// <param name="phoneNumber">Alıcı telefon numarası</param>
    /// <param name="message">Gönderilecek mesaj içeriği</param>
    /// <returns>SMS gönderme sonucu (başarılı/başarısız)</returns>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("SMS gönderiliyor: {PhoneNumber} - {Message}", phoneNumber, message);

            // TODO: Buraya kendi SMS API çağrınızı yazacaksınız
            // Örnek:
            // var response = await _httpClient.PostAsJsonAsync("https://your-sms-api.com/send", new { phoneNumber, message });
            // return response.IsSuccessStatusCode;

            // Şimdilik sadece log olarak simüle ediyoruz
            await Task.Delay(100); // API çağrısını simüle etmek için

            _logger.LogInformation("SMS başarıyla gönderildi: {PhoneNumber}", phoneNumber);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS gönderilirken hata oluştu: {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendBulkSmsAsync(List<(string phone, string message)> smsList)
    {
        try
        {
            var phones = smsList.Select(x => new { phone = x.phone, message = x.message }).ToList();
            var parameters = new {
                api_id = _apiId,
                api_key = _apiKey,
                sender = _sender,
                message_type = _messageType,
                message_content_type = _messageContentType,
                phones = phones
            };
            var json = JsonSerializer.Serialize(parameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("https://api.vatansms.net/api/v1/NtoN", content);
            var resultBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Vatansms toplu gönderim yanıtı: {Response}", resultBody);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu SMS gönderimi sırasında hata oluştu!");
            return false;
        }
    }
}

