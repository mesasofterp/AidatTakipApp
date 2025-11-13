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

    public async Task<IEnumerable<ZamanlayiciAyarlar>> GetActiveSchedulersAsync()
    {
        return await _context.ZamanlayiciAyarlar
            .Where(s => s.Aktif && !s.IsDeleted)
            .OrderBy(s => s.GorevCalismaGunuOfseti)
            .ToListAsync();
    }

    public async Task<ZamanlayiciAyarlar?> GetSchedulerByIdAsync(long id)
    {
        return await _context.ZamanlayiciAyarlar
            .Where(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<ZamanlayiciAyarlar> CreateSchedulerAsync(ZamanlayiciAyarlar settings)
    {
        // Yeni scheduler oluştur
        settings.Id = 0; // Yeni kayıt için
        settings.Aktif = true;
        settings.IsDeleted = false;
        settings.OlusturmaTarihi = DateTime.Now;
        settings.GuncellenmeTarihi = DateTime.Now;
        
        _context.ZamanlayiciAyarlar.Add(settings);
        await _context.SaveChangesAsync();

        // Job'ı oluştur
        await _schedulerFactory.CreateJobAsync(settings);

        _logger.LogInformation("Scheduler oluşturuldu: {Name}, Gün Ofseti: {Offset}", settings.Isim, settings.GorevCalismaGunuOfseti);
        return settings;
    }

    public async Task<ZamanlayiciAyarlar> UpdateSchedulerAsync(ZamanlayiciAyarlar settings)
    {
        var existingScheduler = await GetSchedulerByIdAsync(settings.Id);
        if (existingScheduler == null)
        {
            throw new InvalidOperationException($"Scheduler bulunamadı: {settings.Id}");
        }

        // Mevcut scheduler'ı güncelle
        existingScheduler.Isim = settings.Isim;
        existingScheduler.Saat = settings.Saat;
        existingScheduler.Dakika = settings.Dakika;
        existingScheduler.CronIfadesi = settings.CronIfadesi;
        existingScheduler.Aciklama = settings.Aciklama;
        existingScheduler.MesajSablonu = settings.MesajSablonu;
        existingScheduler.GorevCalismaGunuOfseti = settings.GorevCalismaGunuOfseti;
        existingScheduler.Aktif = settings.Aktif;
        existingScheduler.GuncellenmeTarihi = DateTime.Now;
        existingScheduler.Version++;

        await _context.SaveChangesAsync();

        // Job'ı güncelle
        await _schedulerFactory.UpdateJobAsync(existingScheduler);

        _logger.LogInformation("Scheduler güncellendi: {Name}, Gün Ofseti: {Offset}", existingScheduler.Isim, existingScheduler.GorevCalismaGunuOfseti);
        return existingScheduler;
    }

    public async Task<bool> DeleteSchedulerAsync(long id)
    {
        var scheduler = await GetSchedulerByIdAsync(id);
        if (scheduler != null)
        {
            scheduler.Aktif = false;
            scheduler.IsDeleted = true;
            scheduler.GuncellenmeTarihi = DateTime.Now;
            await _context.SaveChangesAsync();
            
            // Job'ı sil
            await _schedulerFactory.DeleteJobAsync(id);
            
            _logger.LogInformation("Scheduler silindi: {Name}", scheduler.Isim);
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

