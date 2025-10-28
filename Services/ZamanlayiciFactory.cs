using Quartz;
using StudentApp.Models;
using StudentApp.Jobs;

namespace StudentApp.Services;

public class ZamanlayiciFactory : IZamanlayiciFactory
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<ZamanlayiciFactory> _logger;
    private const string JobKeyName = "DailyJob";
    private const string TriggerKeyName = "DailyJob-trigger";

    public ZamanlayiciFactory(IScheduler scheduler, ILogger<ZamanlayiciFactory> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task RecreateJobAsync(ZamanlayiciAyarlar settings)
    {
        try
        {
            // Eski job'ı sil
            await DeleteJobAsync();

            // Job key oluştur
            var jobKey = new JobKey(JobKeyName);
            var triggerKey = new TriggerKey(TriggerKeyName);

            // Job oluştur
            var job = JobBuilder.Create<DailyJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();

            // Trigger oluştur (settings'den cron expression al)
            var cronSchedule = CronScheduleBuilder.CronSchedule(settings.CronIfadesi);
            
            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .StartNow()
                .WithSchedule(cronSchedule)
                .Build();

            // Job ve Trigger'ı scheduler'a ekle
            await _scheduler.ScheduleJob(job, trigger);

            _logger.LogInformation("Job ve Trigger yeniden oluşturuldu - Saat: {Hour}:{Minute}, Cron: {CronExpression}", 
                settings.Saat, settings.Dakika, settings.CronIfadesi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job yeniden oluşturulurken hata oluştu");
            throw;
        }
    }

    public async Task DeleteJobAsync()
    {
        try
        {
            var jobKey = new JobKey(JobKeyName);
            var triggerKey = new TriggerKey(TriggerKeyName);

            // Trigger'ı durdur ve sil
            await _scheduler.UnscheduleJob(triggerKey);
            
            // Job'ı sil
            await _scheduler.DeleteJob(jobKey);

            _logger.LogInformation("Eski job ve trigger silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job silinirken hata oluştu");
        }
    }
}

