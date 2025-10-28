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
}

