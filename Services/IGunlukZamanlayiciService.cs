namespace StudentApp.Services;

public interface IGunlukZamanlayiciService
{
    Task ExecuteDailyTaskAsync();
    Task<int> GetTotalOgrenciCountAsync();
    Task SendDailyNotificationsAsync();
}

