namespace StudentApp.Services;

public interface IGunlukZamanlayiciService
{
    Task ExecuteDailyTaskAsync(long? schedulerId = null);
    Task<int> GetTotalOgrenciCountAsync();
    Task SendDailyNotificationsAsync(long? schedulerId = null);
}

