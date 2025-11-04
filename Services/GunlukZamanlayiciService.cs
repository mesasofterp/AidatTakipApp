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

        // Ödeme takviminde: ödenmemiş ve sms gitmemiş kayıtlar (aktif öğrenci)
        var bekleyenOdemeler = await _context.OgrenciOdemeTakvimi
            .Include(t => t.Ogrenci)
            .Where(t => !t.IsDeleted && !t.Odendi && !t.SmsGittiMi && t.Ogrenci != null && !t.Ogrenci.IsDeleted && t.Ogrenci.Aktif && t.SonOdemeTarihi < today)
            .ToListAsync();

        var smsList = new List<(string telefon, string mesaj, long ogrenciId, long odemeId)>();
        var activeSettings = await _schedulerService.GetActiveSchedulerAsync();
        var template = activeSettings?.MesajSablonu ?? "Sayın [ÖĞRENCİ_ADI] [ÖĞRENCİ_SOYADI], ödemeniz [REFERANS_TARIH] tarihinden beri yapılmamıştır. Lütfen ödemenizi yapınız.";
        var logDetay = new List<string>();

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

            string mesaj = template
                .Replace("[ÖĞRENCİ_ADI]", ogrenci.OgrenciAdi ?? "")
                .Replace("[ÖĞRENCİ_SOYADI]", ogrenci.OgrenciSoyadi ?? "")
                .Replace("[GEÇEN_GÜN]", days.ToString())
                .Replace("[BORÇ_TUTARI]", borc.ToString("N2"))
                .Replace("[REFERANS_TARIH]", referansTarih.ToString("dd.MM.yyyy"));
            smsList.Add((telefon: ogrenci.Telefon, mesaj: mesaj, ogrenciId: ogrenci.Id, odemeId: takvim.Id));
        }

        _logger.LogInformation("SMS gönderilecek öğrenci sayısı: {Count}", smsList.Count);
        if (smsList.Count == 0)
        {
            _logger.LogInformation("Kriterlere uyan öğrenci yok, SMS gönderilmedi.");
            return;
        }

        // (phone, message, ogrenciId, odemeId) listesinden (phone, message) listesine dönüştür
        var smsBatch = smsList.Select(x => (x.telefon, x.mesaj)).ToList();
        bool topluSonuc = await _smsService.SendBulkSmsAsync(smsBatch);

        if (!topluSonuc)
        {
            // Başarılıysa ilgili ödeme kayıtlarında SmsGittiMi=true olarak işaretle
            var odemeIds = smsList.Select(x => x.odemeId).ToList();
            var toUpdate = await _context.OgrenciOdemeTakvimi.Where(t => odemeIds.Contains(t.Id)).ToListAsync();
            foreach (var t in toUpdate)
            {
                t.SmsGittiMi = true;
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Toplu SMS gönderimi başarılı. {Count} ödeme kaydı SmsGittiMi=true olarak işaretlendi.", toUpdate.Count);
        }
        else
        {
            _logger.LogError("Toplu SMS gönderimi sırasında hata oluştu.");
        }
    }
}

