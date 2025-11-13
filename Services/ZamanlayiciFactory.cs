using Quartz;
using StudentApp.Models;
using StudentApp.Jobs;

namespace StudentApp.Services;

public class ZamanlayiciFactory : IZamanlayiciFactory
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<ZamanlayiciFactory> _logger;

    public ZamanlayiciFactory(IScheduler scheduler, ILogger<ZamanlayiciFactory> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    private string GetJobKeyName(long schedulerId) => $"SchedulerJob-{schedulerId}";
    private string GetTriggerKeyName(long schedulerId) => $"SchedulerTrigger-{schedulerId}";

    public async Task CreateJobAsync(ZamanlayiciAyarlar settings)
    {
        try
        {
            var jobKeyName = GetJobKeyName(settings.Id);
            var triggerKeyName = GetTriggerKeyName(settings.Id);
            
            // Job key oluştur
            var jobKey = new JobKey(jobKeyName);
            var triggerKey = new TriggerKey(triggerKeyName);

            // Job zaten var mı kontrol et
            if (await _scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("Job zaten mevcut: {JobKey}", jobKeyName);
                return;
            }

            // Job oluştur - Scheduler ID'sini job data'ya ekle
            var job = JobBuilder.Create<DailyJob>()
                .WithIdentity(jobKey)
                .UsingJobData("SchedulerId", settings.Id.ToString())
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

            _logger.LogInformation("Job ve Trigger oluşturuldu - ID: {Id}, İsim: {Name}, Gün Ofseti: {Offset}, Saat: {Hour}:{Minute}, Cron: {CronExpression}", 
                settings.Id, settings.Isim, settings.GorevCalismaGunuOfseti, settings.Saat, settings.Dakika, settings.CronIfadesi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job oluşturulurken hata oluştu - Scheduler ID: {Id}", settings.Id);
            throw;
        }
    }

    public async Task UpdateJobAsync(ZamanlayiciAyarlar settings)
    {
        try
        {
            // Önce eski job'ı sil
            await DeleteJobAsync(settings.Id);
            
            // Yeni job oluştur
            await CreateJobAsync(settings);

            _logger.LogInformation("Job güncellendi - ID: {Id}, İsim: {Name}", settings.Id, settings.Isim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job güncellenirken hata oluştu - Scheduler ID: {Id}", settings.Id);
            throw;
        }
    }

    public async Task DeleteJobAsync(long schedulerId)
    {
        try
        {
            var jobKeyName = GetJobKeyName(schedulerId);
            var triggerKeyName = GetTriggerKeyName(schedulerId);
            
            var jobKey = new JobKey(jobKeyName);
            var triggerKey = new TriggerKey(triggerKeyName);

            // Job var mı kontrol et
            if (!await _scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("Silinecek job bulunamadı: {JobKey}", jobKeyName);
                return;
            }

            // Trigger'ı durdur ve sil
            await _scheduler.UnscheduleJob(triggerKey);
            
            // Job'ı sil
            await _scheduler.DeleteJob(jobKey);

            _logger.LogInformation("Job ve trigger silindi - Scheduler ID: {Id}", schedulerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job silinirken hata oluştu - Scheduler ID: {Id}", schedulerId);
        }
    }
}

