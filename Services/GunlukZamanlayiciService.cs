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
    public async Task ExecuteDailyTaskAsync()
    {
        _logger.LogInformation("Günlük görev başlatılıyor - {0}", DateTime.Now);

        try
        {
            // Toplam öğrenci sayısını al
            int ogrenciCount = await GetTotalOgrenciCountAsync();
            _logger.LogInformation("Toplam öğrenci sayısı: {OgrenciCount}", ogrenciCount);

            // Günlük bildirimleri gönder
            await SendDailyNotificationsAsync();

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
    /// </summary>
    public async Task SendDailyNotificationsAsync()
    {
        _logger.LogInformation("Otomatik SMS ödeme takibi başlatıldı ({Date})", DateTime.Now);
        var today = DateTime.Today;

        // Tüm aktif öğrencileri ve ödemeleri çek
        var ogrenciler = await _context.Ogrenciler.Where(o => !o.IsDeleted && o.Aktif)
            .Include(o => o.OdemePlanlari)
            .ToListAsync();

        var odemeler = await _context.OgrenciOdemeTakvimi
            .Where(t => !t.IsDeleted)
            .ToListAsync();

        var smsList = new List<(string phone, string message, long ogrenciId)>();
        var activeSettings = await _schedulerService.GetActiveSchedulerAsync();
        var template = activeSettings?.MesajSablonu ?? "Sayın [ÖĞRENCİ_ADI] [ÖĞRENCİ_SOYADI], ödemeniz [REFERANS_TARIH] tarihinden beri yapılmamıştır. Lütfen ödemenizi yapınız.";
        var logDetay = new List<string>();

        foreach (var ogrenci in ogrenciler)
        {
            // İlgili öğrenciye ait tüm ödemeler
            var ogrenciOdemeleri = odemeler.Where(x => x.OgrenciId == ogrenci.Id && x.OdemeTarihi.HasValue).ToList();
            var sonOdemeTarihi = ogrenciOdemeleri
                .OrderByDescending(x => x.OdemeTarihi)
                .FirstOrDefault()?.OdemeTarihi;
            var referansTarih = sonOdemeTarihi ?? ogrenci.KayitTarihi;

            // 30 gün geçti mi?
            if ((today - referansTarih.Date).TotalDays < 30)
                continue;

            // Aynı ayda SMS gönderilmiş mi?
            if (ogrenci.SonSmsTarihi.HasValue &&
                ogrenci.SonSmsTarihi.Value.Year == today.Year &&
                ogrenci.SonSmsTarihi.Value.Month == today.Month)
                continue;

            if (string.IsNullOrWhiteSpace(ogrenci.Telefon))
            {
                _logger.LogWarning("Telefon numarası olmayan öğrenci: {Ad} {Soyad}", ogrenci.OgrenciAdi, ogrenci.OgrenciSoyadi);
                continue;
            }

            // Borç tutarı: son ödeme kaydındaki kalan borç, yoksa plan tutarı
            var ogrenciOdemeleriOrdered = ogrenciOdemeleri
                .OrderByDescending(x => x.OdemeTarihi)
                .ThenByDescending(x => x.Id)
                .ToList();
            var lastPayment = ogrenciOdemeleriOrdered.FirstOrDefault();
            var borc = lastPayment?.BorcTutari ?? (ogrenci.OdemePlanlari?.Tutar ?? 0m);
            var days = (today - referansTarih.Date).Days;

            string mesaj = template
                .Replace("[ÖĞRENCİ_ADI]", ogrenci.OgrenciAdi ?? "")
                .Replace("[ÖĞRENCİ_SOYADI]", ogrenci.OgrenciSoyadi ?? "")
                .Replace("[GEÇEN_GÜN]", days.ToString())
                .Replace("[BORÇ_TUTARI]", borc.ToString("N2"))
                .Replace("[REFERANS_TARIH]", referansTarih.ToString("dd.MM.yyyy"));
            smsList.Add((ogrenci.Telefon, mesaj, ogrenci.Id));
        }

        _logger.LogInformation("SMS gönderilecek öğrenci sayısı: {Count}", smsList.Count);
        if (smsList.Count == 0)
        {
            _logger.LogInformation("Kriterlere uyan öğrenci yok, SMS gönderilmedi.");
            return;
        }

        // (phone, message, ogrenciId) listesinden (phone, message) listesine dönüştür
        var smsBatch = smsList.Select(x => (x.phone, x.message)).ToList();
        bool topluSonuc = await _smsService.SendBulkSmsAsync(smsBatch);

        if (topluSonuc)
        {
            // Başarılıysa SonSmsTarihi güncelle
            var successIds = smsList.Select(x => x.ogrenciId).ToList();
            var updateList = ogrenciler.Where(o => successIds.Contains(o.Id)).ToList();
            foreach (var ogr in updateList)
            {
                ogr.SonSmsTarihi = today;
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Toplu SMS gönderimi başarılı. {Count} öğrencinin SonSmsTarihi güncellendi.", updateList.Count);
        }
        else
        {
            _logger.LogError("Toplu SMS gönderimi sırasında hata oluştu.");
        }
    }
}

