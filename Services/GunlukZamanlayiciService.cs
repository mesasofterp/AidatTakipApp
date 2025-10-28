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
        _logger.LogInformation("Ödeme süresi dolmuş öğrenciler kontrol ediliyor...");

        // Aktif scheduler'ı al ve mesaj şablonunu oku
        var activeScheduler = await _schedulerService.GetActiveSchedulerAsync();
        string messageTemplate = activeScheduler?.MesajSablonu ?? 
            "Sayın [ÖĞRENCİ_ADI] [ÖĞRENCİ_SOYADI], ödemeniz [GEÇEN_GÜN] gündür beklemektedir. Borç tutarınız: [BORÇ_TUTARI]. Lütfen en kısa sürede ödemenizi yapınız.";

        // Bugünün tarihi
        var today = DateTime.Today;

        // OgrenciOdemeTakvimi tablosundan ödenmemiş (OdemeTarihi null) ve 
        // BorcTutari > 0 olan kayıtları bul
        var odemesiBekleyenKayitlar = await _context.OgrenciOdemeTakvimi
            .Include(oot => oot.Ogrenci)
            .Where(oot => oot.OdemeTarihi == null && oot.BorcTutari > 0)
            .ToListAsync();

        // Son ödeme tarihi bugün veya geçmiş olan kayıtları filtrele
        var odemesiGecmisKayitlar = odemesiBekleyenKayitlar
            .Where(oot => oot.SonOdemeTarihi.HasValue 
                        ? oot.SonOdemeTarihi.Value.Date <= today.Date
                        : oot.OlusturmaTarihi.AddDays(30).Date <= today.Date)
            .ToList();

        _logger.LogInformation("Ödeme süresi dolmuş {Count} kayıt bulundu", odemesiGecmisKayitlar.Count);

        int smsSayisi = 0;
        int basariliSmsSayisi = 0;

        // Her kayda göre SMS gönder
        foreach (var kayit in odemesiGecmisKayitlar)
        {
            var ogrenci = kayit.Ogrenci;
            
            // Telefon numarası varsa SMS gönder
            if (!string.IsNullOrEmpty(ogrenci.Telefon))
            {
                smsSayisi++;
                
                // Mesaj şablonunu doldur
                var sonOdemeTarihi = kayit.SonOdemeTarihi ?? kayit.OlusturmaTarihi.AddDays(30);
                var gecenGunSayisi = (today - sonOdemeTarihi).Days;

                // Placeholder'ları değiştir
                var mesaj = messageTemplate
                    .Replace("[ÖĞRENCİ_ADI]", ogrenci.OgrenciAdi)
                    .Replace("[ÖĞRENCİ_SOYADI]", ogrenci.OgrenciSoyadi)
                    .Replace("[GEÇEN_GÜN]", gecenGunSayisi.ToString())
                    .Replace("[BORÇ_TUTARI]", kayit.BorcTutari.ToString("C2"))
                    .Replace("[SON_ÖDEME_TARİHİ]", kayit.SonOdemeTarihi?.ToString("dd.MM.yyyy") ?? "");


                var sonuc = await _smsService.SendSmsAsync(ogrenci.Telefon, mesaj);
                
                if (sonuc)
                {
                    basariliSmsSayisi++;
                    _logger.LogInformation("SMS gönderildi: {OgrenciAdi} {OgrenciSoyadi} - {Telefon} - Borç: {Borc}", 
                        ogrenci.OgrenciAdi, ogrenci.OgrenciSoyadi, ogrenci.Telefon, kayit.BorcTutari);
                }
                else
                {
                    _logger.LogWarning("SMS gönderilemedi: {OgrenciAdi} {OgrenciSoyadi} - {Telefon}", 
                        ogrenci.OgrenciAdi, ogrenci.OgrenciSoyadi, ogrenci.Telefon);
                }

                // API rate limit'i için kısa bir bekleme
                await Task.Delay(100);
            }
            else
            {
                _logger.LogWarning("Telefon numarası olmayan öğrenci: {OgrenciAdi} {OgrenciSoyadi}", 
                    ogrenci.OgrenciAdi, ogrenci.OgrenciSoyadi);
            }
        }

        _logger.LogInformation("Toplam {SmsSayisi} SMS gönderilmeye çalışıldı, {BasariliSmsSayisi} başarılı", 
            smsSayisi, basariliSmsSayisi);
    }
}

