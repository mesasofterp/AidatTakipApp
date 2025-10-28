namespace StudentApp.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
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
}

