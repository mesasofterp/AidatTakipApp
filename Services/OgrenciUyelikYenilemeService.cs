using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OgrenciUyelikYenilemeService : IOgrenciUyelikYenilemeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OgrenciUyelikYenilemeService> _logger;
        private readonly IOgrenciOdemeTakvimiService _odemeService;

        public OgrenciUyelikYenilemeService(
        AppDbContext context,
    ILogger<OgrenciUyelikYenilemeService> logger,
          IOgrenciOdemeTakvimiService odemeService)
     {
      _context = context;
            _logger = logger;
   _odemeService = odemeService;
    }

        public async Task<IEnumerable<OgrenciUyelikYenileme>> GetAllAsync()
  {
     return await _context.OgrenciUyelikYenileme
.Include(y => y.Ogrenci)
               .Include(y => y.EskiOdemePlani)
     .Include(y => y.YeniOdemePlani)
      .Where(y => !y.IsDeleted)
     .OrderByDescending(y => y.YenilemeTarihi)
       .ToListAsync();
        }

        public async Task<IEnumerable<OgrenciUyelikYenileme>> GetByOgrenciIdAsync(long ogrenciId)
   {
return await _context.OgrenciUyelikYenileme
          .Include(y => y.EskiOdemePlani)
 .Include(y => y.YeniOdemePlani)
      .Where(y => y.OgrenciId == ogrenciId && !y.IsDeleted)
  .OrderByDescending(y => y.YenilemeTarihi)
      .ToListAsync();
        }

        public async Task<OgrenciUyelikYenileme?> GetByIdAsync(long id)
 {
      return await _context.OgrenciUyelikYenileme
        .Include(y => y.Ogrenci)
        .Include(y => y.EskiOdemePlani)
                .Include(y => y.YeniOdemePlani)
  .FirstOrDefaultAsync(y => y.Id == id && !y.IsDeleted);
        }

        public async Task<OgrenciUyelikYenileme> YenileAsync(
   long ogrenciId,
long yeniOdemePlaniId,
        decimal? indirimTutari,
         string? indirimAciklama,
            string kullaniciAdi)
        {
     var ogrenci = await _context.Ogrenciler
              .Include(o => o.OdemePlanlari)
        .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

          if (ogrenci == null)
          throw new InvalidOperationException("Öðrenci bulunamadý.");

         var yeniOdemePlani = await _context.OdemePlanlari
   .FirstOrDefaultAsync(p => p.Id == yeniOdemePlaniId && !p.IsDeleted);

     if (yeniOdemePlani == null)
                throw new InvalidOperationException("Yeni ödeme planý bulunamadý.");

   // Mevcut durumu kaydet
   var eskiOdemePlaniId = ogrenci.OdemePlanlariId;
            var eskiPlan = ogrenci.OdemePlanlari;
   var kalanBorc = await _odemeService.GetKalanBorcAsync(ogrenciId);
   var toplamOdenen = await _odemeService.GetToplamOdenenTutarAsync(ogrenciId);

     // Yenileme kaydý oluþtur
      var yenileme = new OgrenciUyelikYenileme
     {
  OgrenciId = ogrenciId,
     EskiOdemePlaniId = eskiOdemePlaniId,
      YeniOdemePlaniId = yeniOdemePlaniId,
      YenilemeTarihi = DateTime.Now,
             YenilemeBaslangicTarihi = DateTime.Today,
     EskiDonemToplamTutar = eskiPlan?.ToplamTutar ?? 0,
    YeniDonemToplamTutar = yeniOdemePlani.ToplamTutar - (indirimTutari ?? 0),
                EskiDonemKalanBorc = kalanBorc,
  IndirimTutari = indirimTutari,
       IndirimAciklama = indirimAciklama,
     OtomatikYenileme = false,
     Durum = YenilemeDurumuEnum.Aktif,
      YenileyenKullanici = kullaniciAdi,
            Aktif = true,
       IsDeleted = false
    };

   _context.OgrenciUyelikYenileme.Add(yenileme);

            // Öðrencinin ödeme planýný güncelle
      ogrenci.OdemePlanlariId = yeniOdemePlaniId;
     ogrenci.IlkTaksitSonOdemeTarihi = DateTime.Today; // Yeni dönem baþlangýcý
            ogrenci.Version++;

            await _context.SaveChangesAsync();

            // Yeni dönem için taksitleri oluþtur
         await YeniDonemTaksitleriniOlusturAsync(ogrenciId, yeniOdemePlani, yenileme.YenilemeBaslangicTarihi, kalanBorc);

       _logger.LogInformation(
    "Öðrenci üyeliði yenilendi. Öðrenci ID: {OgrenciId}, Eski Plan: {EskiPlan}, Yeni Plan: {YeniPlan}, Kullanýcý: {Kullanici}",
           ogrenciId, eskiOdemePlaniId, yeniOdemePlaniId, kullaniciAdi);

         return yenileme;
  }

public async Task<UyelikDurumBilgi> GetUyelikDurumuAsync(long ogrenciId)
        {
        var ogrenci = await _context.Ogrenciler
          .Include(o => o.OdemePlanlari)
     .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

       if (ogrenci == null)
 throw new InvalidOperationException("Öðrenci bulunamadý.");

  var taksitler = await _context.OgrenciOdemeTakvimi
                .Where(t => t.OgrenciId == ogrenciId && !t.IsDeleted)
    .OrderBy(t => t.TaksitNo)
         .ToListAsync();

          var durum = new UyelikDurumBilgi
     {
     OgrenciId = ogrenciId,
 OgrenciAdSoyad = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}",
  MevcutOdemePlani = ogrenci.OdemePlanlari,
                ToplamTaksitSayisi = taksitler.Count,
       OdenenTaksitSayisi = taksitler.Count(t => t.Odendi),
    ToplamOdenen = taksitler.Sum(t => t.OdenenTutar),
   KalanBorc = taksitler.Any(t => !t.Odendi) ? taksitler.Where(t => !t.Odendi).Max(t => t.BorcTutari) : 0,
   SonOdemeTarihi = taksitler.Where(t => t.OdemeTarihi != null).Max(t => t.OdemeTarihi)
            };

            // Tahmini bitiþ tarihi hesapla
            var sonTaksit = taksitler.OrderByDescending(t => t.TaksitNo).FirstOrDefault();
            if (sonTaksit?.SonOdemeTarihi != null)
            {
    durum.TahminiBitisTarihi = sonTaksit.SonOdemeTarihi;
            }

            // Yenileme gerekli mi?
    var tumTaksitlerOdendi = durum.ToplamTaksitSayisi > 0 && durum.OdenenTaksitSayisi == durum.ToplamTaksitSayisi;
            var kalanBorcYok = durum.KalanBorc <= 0;

        if (tumTaksitlerOdendi && kalanBorcYok)
            {
        durum.YenilemeGerekli = true;
   durum.YenilemeNedeni = "Tüm taksitler ödendi, yeni dönem için yenileme yapýlabilir.";
            }
      else if (durum.OdenenTaksitSayisi >= durum.ToplamTaksitSayisi * 0.8m) // %80'i ödendiyse
     {
      durum.YenilemeGerekli = true;
             durum.YenilemeNedeni = "Taksitlerin %80'i tamamlandý, yenileme için uygun.";
       }
  else
 {
  durum.YenilemeGerekli = false;
   durum.YenilemeNedeni = $"Henüz yenileme için uygun deðil ({durum.OdenenTaksitSayisi}/{durum.ToplamTaksitSayisi} taksit ödendi).";
   }

            return durum;
        }

        public async Task<bool> YenilemeGerekliMiAsync(long ogrenciId)
        {
var durum = await GetUyelikDurumuAsync(ogrenciId);
            return durum.YenilemeGerekli;
        }

        public async Task<IEnumerable<OgrenciYenilemeBilgi>> GetYenilenebilirOgrencilerAsync()
   {
  var aktifOgrenciler = await _context.Ogrenciler
      .Include(o => o.OdemePlanlari)
              .Where(o => o.Aktif && !o.IsDeleted)
             .ToListAsync();

     var yenilenebilirler = new List<OgrenciYenilemeBilgi>();

          foreach (var ogrenci in aktifOgrenciler)
            {
                var taksitler = await _context.OgrenciOdemeTakvimi
    .Where(t => t.OgrenciId == ogrenci.Id && !t.IsDeleted)
         .ToListAsync();

                if (!taksitler.Any()) continue;

 var toplamTaksit = taksitler.Count;
         var odenenTaksit = taksitler.Count(t => t.Odendi);
    var tumTaksitlerOdendi = odenenTaksit == toplamTaksit && odenenTaksit > 0;
  var coguOdendi = odenenTaksit >= toplamTaksit * 0.8m;
   var kalanBorc = taksitler.Any(t => !t.Odendi) ? taksitler.Where(t => !t.Odendi).Max(t => t.BorcTutari) : 0;

      if (tumTaksitlerOdendi || coguOdendi)
  {
           var sonTaksit = taksitler.OrderByDescending(t => t.SonOdemeTarihi).FirstOrDefault();
    var kalanGun = sonTaksit?.SonOdemeTarihi != null 
   ? (sonTaksit.SonOdemeTarihi.Value - DateTime.Today).Days 
          : 0;

        yenilenebilirler.Add(new OgrenciYenilemeBilgi
{
              OgrenciId = ogrenci.Id,
            OgrenciAdSoyad = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}",
    OdemePlaniAdi = ogrenci.OdemePlanlari?.KursProgrami ?? "Belirtilmemiþ",
            OdemePlaniId = ogrenci.OdemePlanlariId,
            TamamlananTaksitSayisi = odenenTaksit,
   ToplamTaksitSayisi = toplamTaksit,
      ToplamOdenen = taksitler.Sum(t => t.OdenenTutar),
          KalanBorc = kalanBorc,
  SonOdemeTarihi = sonTaksit?.SonOdemeTarihi,
            KalanGun = kalanGun,
 TumTaksitlerOdendi = tumTaksitlerOdendi
      });
                }
            }

       return yenilenebilirler.OrderBy(y => y.KalanGun).ThenBy(y => y.OgrenciAdSoyad);
        }

        public async Task<TopluYenilemesonuc> TopluYenileAsync(List<long> ogrenciIdList, long yeniOdemePlaniId, string kullaniciAdi)
        {
            var sonuc = new TopluYenilemesonuc
       {
         ToplamOgrenciSayisi = ogrenciIdList.Count
            };

      foreach (var ogrenciId in ogrenciIdList)
          {
     try
    {
     await YenileAsync(ogrenciId, yeniOdemePlaniId, null, "Toplu yenileme", kullaniciAdi);
    sonuc.BasariliYenilemeSayisi++;
          sonuc.YenilenenOgrenciIdler.Add(ogrenciId);
             }
  catch (Exception ex)
       {
   sonuc.HataliYenilemeSayisi++;
       var ogrenci = await _context.Ogrenciler.FindAsync(ogrenciId);
     var ogrenciAd = ogrenci != null ? $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}" : $"ID:{ogrenciId}";
          sonuc.Hatalar.Add($"{ogrenciAd}: {ex.Message}");
   _logger.LogError(ex, "Toplu yenileme hatasý. Öðrenci ID: {OgrenciId}", ogrenciId);
      }
        }

       return sonuc;
        }

   /// <summary>
     /// Yeni dönem için taksitleri oluþturur
        /// </summary>
        private async Task YeniDonemTaksitleriniOlusturAsync(
            long ogrenciId,
   OdemePlanlari odemePlani,
            DateTime baslangicTarihi,
    decimal eskiDonemKalanBorc)
        {
      var yeniTaksitler = new List<OgrenciOdemeTakvimi>();
            var ilkTaksitTarihi = baslangicTarihi;

     // Vade hesaplamasý
    var vadeGunSayisi = odemePlani.Vade ?? (odemePlani.TaksitSayisi * 30); // Varsayýlan 30 gün/taksit
    var taksitAralik = vadeGunSayisi / odemePlani.TaksitSayisi;

       for (int i = 1; i <= odemePlani.TaksitSayisi; i++)
            {
    var taksitSonOdemeTarihi = ilkTaksitTarihi.AddDays(taksitAralik * i);
          var oncekiTaksitKalanBorc = await HesaplaOncekiTaksitKalanBorcAsync(ogrenciId, i, odemePlani, eskiDonemKalanBorc);

  var taksit = new OgrenciOdemeTakvimi
                {
      OgrenciId = ogrenciId,
        TaksitNo = i,
           TaksitTutari = odemePlani.TaksitTutari,
           SonOdemeTarihi = taksitSonOdemeTarihi,
           OdenenTutar = 0,
    Odendi = false,
          BorcTutari = oncekiTaksitKalanBorc,
           OlusturmaTarihi = DateTime.Now,
    Aktif = true,
            IsDeleted = false,
     Aciklama = $"Yeni dönem - {i}. taksit"
     };

     yeniTaksitler.Add(taksit);
            }

            await _context.OgrenciOdemeTakvimi.AddRangeAsync(yeniTaksitler);
      await _context.SaveChangesAsync();

   _logger.LogInformation(
 "Yeni dönem taksitleri oluþturuldu. Öðrenci ID: {OgrenciId}, Taksit Sayýsý: {TaksitSayisi}",
          ogrenciId, yeniTaksitler.Count);
        }

    private async Task<decimal> HesaplaOncekiTaksitKalanBorcAsync(
            long ogrenciId,
    int mevcutTaksitNo,
  OdemePlanlari odemePlani,
decimal eskiDonemKalanBorc)
        {
            // Ýlk taksitte eski dönem kalan borç da eklenir
      if (mevcutTaksitNo == 1)
    {
 return eskiDonemKalanBorc + odemePlani.ToplamTutar;
            }

    // Sonraki taksitler için: önceki taksitlerin toplamý - ödenenler
  var kalanBorc = eskiDonemKalanBorc + odemePlani.ToplamTutar - ((mevcutTaksitNo - 1) * odemePlani.TaksitTutari);
            return Math.Max(0, kalanBorc);
        }
    }
}
