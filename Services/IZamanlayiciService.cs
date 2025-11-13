using StudentApp.Models;

namespace StudentApp.Services;

public interface IZamanlayiciService
{
    Task<IEnumerable<ZamanlayiciAyarlar>> GetActiveSchedulersAsync();
    Task<ZamanlayiciAyarlar?> GetSchedulerByIdAsync(long id);
    Task<ZamanlayiciAyarlar> CreateSchedulerAsync(ZamanlayiciAyarlar settings);
    Task<ZamanlayiciAyarlar> UpdateSchedulerAsync(ZamanlayiciAyarlar settings);
    Task<bool> DeleteSchedulerAsync(long id);
    Task<string> GenerateCronExpressionAsync(int hour, int minute, bool isDaily = true);
    Task<string> FormatMessageAsync(string template, object studentData);
}

