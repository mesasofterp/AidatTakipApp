using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services;

public class ZamanlayiciService : IZamanlayiciService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ZamanlayiciService> _logger;
    private readonly IZamanlayiciFactory _schedulerFactory;

    public ZamanlayiciService(AppDbContext context, ILogger<ZamanlayiciService> logger, IZamanlayiciFactory schedulerFactory)
    {
        _context = context;
        _logger = logger;
        _schedulerFactory = schedulerFactory;
    }

    public async Task<ZamanlayiciAyarlar?> GetActiveSchedulerAsync()
    {
        return await _context.ZamanlayiciAyarlar
            .Where(s =>  s.Aktif)
            .FirstOrDefaultAsync();
    }

    public async Task<ZamanlayiciAyarlar> CreateOrUpdateSchedulerAsync(ZamanlayiciAyarlar settings)
    {
        // Aktif scheduler varsa devre dışı bırak
        var activeScheduler = await GetActiveSchedulerAsync();
        if (activeScheduler != null)
        {
            await DeleteActiveSchedulerAsync();
        }

        // Yeni scheduler oluştur
        settings.Id = 0; // Yeni kayıt için
        settings.OlusturmaTarihi = DateTime.Now;
        settings.GuncellenmeTarihi = DateTime.Now;
        
        _context.ZamanlayiciAyarlar.Add(settings);
        await _context.SaveChangesAsync();

        // Job'ı yeniden oluştur
        await _schedulerFactory.RecreateJobAsync(settings);

        _logger.LogInformation("Scheduler oluşturuldu/güncellendi: {Name}", settings.Isim);
        return settings;
    }

    public async Task<bool> DeleteActiveSchedulerAsync()
    {
        var activeScheduler = await GetActiveSchedulerAsync();
        if (activeScheduler != null)
        {
            activeScheduler.Aktif = false;
            activeScheduler.GuncellenmeTarihi = DateTime.Now;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Aktif scheduler devre dışı bırakıldı: {Name}", activeScheduler.Isim);
            return true;
        }
        return false;
    }

    public async Task<string> GenerateCronExpressionAsync(int hour, int minute, bool isDaily = true)
    {
        // Her gün belirtilen saatte çalış
        return $"0 {minute} {hour} * * ?";
    }

    public async Task<string> FormatMessageAsync(string template, object studentData)
    {
        var message = template;
        
        // Dynamically replace placeholders from studentData object
        var type = studentData.GetType();
        var properties = type.GetProperties();
        
        foreach (var prop in properties)
        {
            var placeholder = $"[{prop.Name.ToUpper()}]";
            var value = prop.GetValue(studentData)?.ToString() ?? "";
            message = message.Replace(placeholder, value);
        }

        return message;
    }
}

