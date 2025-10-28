using StudentApp.Models;

namespace StudentApp.Services;

public interface IZamanlayiciService
{
    Task<ZamanlayiciAyarlar?> GetActiveSchedulerAsync();
    Task<ZamanlayiciAyarlar> CreateOrUpdateSchedulerAsync(ZamanlayiciAyarlar settings);
    Task<bool> DeleteActiveSchedulerAsync();
    Task<string> GenerateCronExpressionAsync(int hour, int minute, bool isDaily = true);
    Task<string> FormatMessageAsync(string template, object studentData);
}

