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
            // Job data'dan scheduler ID'sini al
            long? schedulerId = null;
            if (context.JobDetail.JobDataMap.ContainsKey("SchedulerId"))
            {
                var schedulerIdStr = context.JobDetail.JobDataMap.GetString("SchedulerId");
                if (long.TryParse(schedulerIdStr, out var id))
                {
                    schedulerId = id;
                    _logger.LogInformation("Scheduler ID: {SchedulerId} için job çalışıyor", schedulerId);
                }
            }

            // Servis üzerinden günlük görevleri çalıştır
            await _dailyTaskService.ExecuteDailyTaskAsync(schedulerId);

            _logger.LogInformation("DailyJob başarıyla tamamlandı - {0}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DailyJob çalışırken hata oluştu - {0}", DateTime.Now);
        }
    }
}
