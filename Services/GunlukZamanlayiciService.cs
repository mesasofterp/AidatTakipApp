using Microsoft.EntityFrameworkCore;
using StudentApp.Data;

namespace StudentApp.Services;

public class GunlukZamanlayiciService : IGunlukZamanlayiciService
{
    private readonly AppDbContext _context;
    private readonly ILogger<GunlukZamanlayiciService> _logger;
    private readonly ISmsService _smsService;
    private readonly IZamanlayiciService _schedulerService;

    public GunlukZamanlayiciService(AppDbContext context, ILogger<GunlukZamanlayiciService> logger, ISmsService smsService, IZamanlayiciService schedulerService)
    {
        _context = context;
        _logger = logger;
        _smsService = smsService;
        _schedulerService = schedulerService;
    }

    /// <summary>
    /// Ana günlük görev metodunu çalıştırır
    /// </summary>
    public async Task ExecuteDailyTaskAsync(long? schedulerId = null)
    {
        _logger.LogInformation("Günlük görev başlatılıyor - {0}, Scheduler ID: {SchedulerId}", DateTime.Now, schedulerId);

        try
        {
            // Toplam öğrenci sayısını al
            int ogrenciCount = await GetTotalOgrenciCountAsync();
            _logger.LogInformation("Toplam öğrenci sayısı: {OgrenciCount}", ogrenciCount);

            // Günlük bildirimleri gönder
            await SendDailyNotificationsAsync(schedulerId);

            // Buraya daha fazla görev ekleyebilirsiniz
            // Örnek: Rapor oluşturma, veri temizleme, backup alma vb.

            _logger.LogInformation("Günlük görev başarıyla tamamlandı - {0}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Günlük görev çalışırken hata oluştu - {0}", DateTime.Now);
            throw;
        }
    }

    /// <summary>
    /// Toplam öğrenci sayısını getirir
    /// </summary>
    public async Task<int> GetTotalOgrenciCountAsync()
    {
        return await _context.Ogrenciler.CountAsync();
    }

    /// <summary>
    /// Günlük bildirimleri gönderir - ödeme süresi dolmuş öğrencilere SMS gönderir
    /// OgrenciOdemeTakvimi tablosundan ödenmemiş kayıtları kontrol eder
    /// Belirli bir scheduler için çalışır
    /// </summary>
    public async Task SendDailyNotificationsAsync(long? schedulerId = null)
    {
        _logger.LogInformation("Otomatik SMS ödeme takibi başlatıldı ({Date})", DateTime.Now);
        var today = DateTime.Today;

        // Aktif scheduler'ları al
        var activeSchedulers = await _schedulerService.GetActiveSchedulersAsync();
        
        // Eğer belirli bir scheduler ID verilmişse, sadece onu kullan
        if (schedulerId.HasValue)
        {
            activeSchedulers = activeSchedulers.Where(s => s.Id == schedulerId.Value).ToList();
        }

        if (!activeSchedulers.Any())
        {
            _logger.LogInformation("Aktif scheduler bulunamadı.");
            return;
        }

        var smsList = new List<(string telefon, string mesaj, long ogrenciId, long odemeId, long schedulerId)>();
        var logDetay = new List<string>();

        // Her scheduler için ayrı ayrı kontrol yap
        foreach (var scheduler in activeSchedulers)
        {
            var offset = scheduler.GorevCalismaGunuOfseti;
            var kontrolTarihi = today.AddDays(-offset); // Offset'e göre kontrol tarihini hesapla

            _logger.LogInformation("Scheduler kontrol ediliyor: {Name}, Gün Ofseti: {Offset}, Kontrol Tarihi: {KontrolTarihi}", 
                scheduler.Isim, offset, kontrolTarihi.ToString("dd.MM.yyyy"));

            // Ödeme takviminde: ödenmemiş ve sms gitmemiş kayıtlar (aktif öğrenci)
            // Son ödeme tarihi, kontrol tarihine eşit olan kayıtları bul
            var bekleyenOdemeler = await _context.OgrenciOdemeTakvimi
                .Include(t => t.Ogrenci)
                .Where(t => !t.IsDeleted 
                    && !t.Odendi 
                    && t.Ogrenci != null 
                    && !t.Ogrenci.IsDeleted 
                    && t.Ogrenci.Aktif 
                    && t.SonOdemeTarihi.HasValue
                    && t.SonOdemeTarihi.Value.Date == kontrolTarihi.Date)
                .ToListAsync();

            _logger.LogInformation("Scheduler {Name} için {Count} adet ödeme kaydı bulundu.", scheduler.Isim, bekleyenOdemeler.Count);

            foreach (var takvim in bekleyenOdemeler)
            {
                var ogrenci = takvim.Ogrenci;
                if (ogrenci == null) continue;
                if (string.IsNullOrWhiteSpace(ogrenci.Telefon))
                {
                    _logger.LogWarning("Telefon numarası olmayan öğrenci: {Ad} {Soyad}", ogrenci.OgrenciAdi, ogrenci.OgrenciSoyadi);
                    continue;
                }

                var referansTarih = takvim.SonOdemeTarihi?.Date ?? today;
                var days = (today - referansTarih).Days;
                var borc = takvim.BorcTutari;
                var template = scheduler.MesajSablonu;

                string mesaj = template
                    .Replace("[ÖĞRENCİ_ADI]", ogrenci.OgrenciAdi ?? "")
                    .Replace("[ÖĞRENCİ_SOYADI]", ogrenci.OgrenciSoyadi ?? "")
                    .Replace("[GEÇEN_GÜN]", days.ToString())
                    .Replace("[KALAN_GÜN]", Math.Max(0, -days).ToString())
                    .Replace("[BORÇ_TUTARI]", borc.ToString("N2"))
                    .Replace("[REFERANS_TARIH]", referansTarih.ToString("dd.MM.yyyy"));

                smsList.Add((telefon: ogrenci.Telefon, mesaj: mesaj, ogrenciId: ogrenci.Id, odemeId: takvim.Id, schedulerId: scheduler.Id));
            }
        }

        _logger.LogInformation("Toplam SMS gönderilecek öğrenci sayısı: {Count}", smsList.Count);
        if (smsList.Count == 0)
        {
            _logger.LogInformation("Kriterlere uyan öğrenci yok, SMS gönderilmedi.");
            return;
        }

        // Scheduler'lara göre grupla ve her biri için ayrı ayrı SMS gönder
        var schedulerGroups = smsList.GroupBy(x => x.schedulerId);
        
        foreach (var group in schedulerGroups)
        {
            var scheduler = activeSchedulers.FirstOrDefault(s => s.Id == group.Key);
            if (scheduler == null)
            {
                _logger.LogInformation("Scheduler {Id} bulunamadı.", group.Key);
                continue;
            }

            // (phone, message, ogrenciId, odemeId, schedulerId) listesinden (phone, message) listesine dönüştür
            var smsBatch = group.Select(x => (x.telefon, x.mesaj)).ToList();
            bool topluSonuc = await _smsService.SendBulkSmsAsync(smsBatch);

            if (!topluSonuc)
            {
                // Başarılıysa ilgili ödeme kayıtlarında SmsGittiMi=true olarak işaretle
                var odemeIds = group.Select(x => x.odemeId).ToList();
                var toUpdate = await _context.OgrenciOdemeTakvimi.Where(t => odemeIds.Contains(t.Id)).ToListAsync();
                foreach (var t in toUpdate)
                {
                    t.SmsGittiMi = true;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Scheduler {Name} için toplu SMS gönderimi başarılı. {Count} ödeme kaydı SmsGittiMi=true olarak işaretlendi.", 
                    scheduler.Isim, toUpdate.Count);
            }
            else
            {
                _logger.LogError("Scheduler {Name} için toplu SMS gönderimi sırasında hata oluştu.", scheduler.Isim);
            }
        }
    }
}

