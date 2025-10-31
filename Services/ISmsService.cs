namespace StudentApp.Services;

public interface ISmsService
{
    /// <summary>
    /// SMS gönderme metodu - Buraya kendi SMS API çağrınızı yazacaksınız
    /// </summary>
    /// <param name="phoneNumber">Alıcı telefon numarası</param>
    /// <param name="message">Gönderilecek mesaj içeriği</param>
    /// <returns>SMS gönderme sonucu (başarılı/başarısız)</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Toplu SMS gönderme metodu
    /// </summary>
    /// <param name="smsList">SMS listesi (telefon ve mesaj)</param>
    /// <returns>Başarı durumu</returns>
    Task<bool> SendBulkSmsAsync(List<(string phone, string message)> smsList);
}

