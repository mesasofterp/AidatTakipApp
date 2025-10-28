using Quartz;
using StudentApp.Services;

namespace StudentApp.Jobs;

public class DailyJob : IJob
{
    private readonly ILogger<DailyJob> _logger;
    private readonly IGunlukZamanlayiciService _dailyTaskService;

    public DailyJob(ILogger<DailyJob> logger, IGunlukZamanlayiciService dailyTaskService)
    {
        _logger = logger;
        _dailyTaskService = dailyTaskService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("DailyJob çalışmaya başladı - {0}", DateTime.Now);

        try
        {
            // Servis üzerinden günlük görevleri çalıştır
            await _dailyTaskService.ExecuteDailyTaskAsync();

            _logger.LogInformation("DailyJob başarıyla tamamlandı - {0}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DailyJob çalışırken hata oluştu - {0}", DateTime.Now);
        }
    }
}
