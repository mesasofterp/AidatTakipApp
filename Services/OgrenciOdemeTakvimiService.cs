using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OgrenciOdemeTakvimiService : IOgrenciOdemeTakvimiService
    {
        private readonly AppDbContext _context;

        public OgrenciOdemeTakvimiService(AppDbContext context)
    {
   _context = context;
        }

        public async Task<IEnumerable<OgrenciOdemeTakvimi>> GetAllOdemelerAsync()
        {
            return await _context.OgrenciOdemeTakvimi
   .Include(o => o.Ogrenci)
  .Where(o => !o.IsDeleted)
 .OrderByDescending(o => o.OdemeTarihi)
  .ToListAsync();
}

        public async Task<IEnumerable<OgrenciOdemeTakvimi>> GetOdemelerByOgrenciIdAsync(long ogrenciId)
        {
            return await _context.OgrenciOdemeTakvimi
                .Include(o => o.Ogrenci)
             .Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
 .OrderByDescending(o => o.OdemeTarihi)
                .ToListAsync();
        }

      public async Task<OgrenciOdemeTakvimi?> GetOdemeByIdAsync(long id)
   {
 return await _context.OgrenciOdemeTakvimi
    .Include(o => o.Ogrenci)
              .Where(o => !o.IsDeleted)
         .FirstOrDefaultAsync(o => o.Id == id);
  }

        public async Task<OgrenciOdemeTakvimi> AddOdemeAsync(OgrenciOdemeTakvimi odeme)
        {
       odeme.IsDeleted = false;
   odeme.Aktif = true;
  odeme.Version = 0;
 
if (!odeme.OdemeTarihi.HasValue)
       {
  odeme.OdemeTarihi = DateTime.Now;
}

       // Eðer TaksitTutari kaydedilmiþse ve ödenen tutar bu tutara eþit veya büyükse "Ödendi" true
      if (odeme.TaksitTutari.HasValue && odeme.TaksitTutari.Value > 0)
   {
        if (odeme.OdenenTutar >= odeme.TaksitTutari.Value && odeme.OdenenTutar > 0)
       {
         odeme.Odendi = true;
       }
      }

       _context.OgrenciOdemeTakvimi.Add(odeme);
await _context.SaveChangesAsync();
    
   // Sonraki ödemelerin kalan borcunu güncelle
    await RecalculateKalanBorcForOgrenciAsync(odeme.OgrenciId);
    
         return odeme;
        }

    public async Task<OgrenciOdemeTakvimi?> UpdateOdemeAsync(OgrenciOdemeTakvimi odeme)
    {
        var existingOdeme = await _context.OgrenciOdemeTakvimi
            .Where(o => !o.IsDeleted)
 .FirstOrDefaultAsync(o => o.Id == odeme.Id);

      if (existingOdeme == null)
            return null;

        // Son ödeme tarihinde deðiþiklik var mý kontrol et
 var sonOdemeTarihiDegisti = existingOdeme.SonOdemeTarihi != odeme.SonOdemeTarihi;
  var ilkTaksitMi = existingOdeme.TaksitNo == 1;

        existingOdeme.OdemeTarihi = odeme.OdemeTarihi;
        existingOdeme.SonOdemeTarihi = odeme.SonOdemeTarihi;
      existingOdeme.OdenenTutar = odeme.OdenenTutar;
        existingOdeme.BorcTutari = odeme.BorcTutari;
        existingOdeme.Aciklama = odeme.Aciklama;
        existingOdeme.Version++;

        // Eðer TaksitTutari kaydedilmiþse ve ödenen tutar bu tutara eþit veya büyükse "ödendi" true
     if (existingOdeme.TaksitTutari.HasValue && existingOdeme.TaksitTutari.Value > 0)
        {
            if (existingOdeme.OdenenTutar >= existingOdeme.TaksitTutari.Value && existingOdeme.OdenenTutar > 0)
   {
     existingOdeme.Odendi = true;
 }
  else
            {
        existingOdeme.Odendi = false;
            }
 }

        await _context.SaveChangesAsync();

      // Eðer ilk taksitin son ödeme tarihi deðiþtiyse, diðer taksitleri de güncelle
        if (sonOdemeTarihiDegisti && ilkTaksitMi && existingOdeme.SonOdemeTarihi.HasValue)
        {
       await UpdateDigerTaksitTarihleriniAsync(existingOdeme.OgrenciId, existingOdeme.SonOdemeTarihi.Value);
    }

   // Tüm ödemelerin kalan borcunu yeniden hesapla
        await RecalculateKalanBorcForOgrenciAsync(existingOdeme.OgrenciId);

        return existingOdeme;
    }

    /// <summary>
    /// Ýlk taksitin son ödeme tarihine göre diðer taksitlerin tarihlerini günceller
    /// </summary>
    private async Task UpdateDigerTaksitTarihleriniAsync(long ogrenciId, DateTime ilkTaksitTarihi)
    {
        // Öðrencinin ödeme planýný getir
        var ogrenci = await _context.Ogrenciler
    .Include(o => o.OdemePlanlari)
    .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

        if (ogrenci == null || ogrenci.OdemePlanlari == null)
            return;

    var odemePlani = ogrenci.OdemePlanlari;

    // Vade hesaplama
        int vadeSuresi = odemePlani.Vade.HasValue ? odemePlani.Vade.Value : (odemePlani.Taksit * 30);
        int taksitBasinaGun = vadeSuresi / odemePlani.Taksit;

        // Ýlk taksit hariç diðer tüm taksitleri getir
        var digerTaksitler = await _context.OgrenciOdemeTakvimi
   .Where(o => o.OgrenciId == ogrenciId && 
     !o.IsDeleted && 
            o.TaksitNo.HasValue && 
             o.TaksitNo.Value > 1)
    .OrderBy(o => o.TaksitNo)
            .ToListAsync();

        // Her taksit için yeni tarihi hesapla
        foreach (var taksit in digerTaksitler)
    {
          if (taksit.TaksitNo.HasValue)
  {
    // Yeni son ödeme tarihi = Ýlk taksit tarihi + (Taksit No * Taksit baþýna gün)
      taksit.SonOdemeTarihi = ilkTaksitTarihi.AddDays(taksit.TaksitNo.Value * taksitBasinaGun);
      taksit.Version++;
   }
        }

   await _context.SaveChangesAsync();
    }

        public async Task<bool> DeleteOdemeAsync(long id)
      {
   var odeme = await _context.OgrenciOdemeTakvimi
    .Where(o => !o.IsDeleted)
         .FirstOrDefaultAsync(o => o.Id == id);
   
 if (odeme == null)
         return false;

       var ogrenciId = odeme.OgrenciId;
   
         // Soft delete
         odeme.IsDeleted = true;
      odeme.Aktif = false;
       await _context.SaveChangesAsync();
   
          // Kalan ödemelerin borcunu yeniden hesapla
            await RecalculateKalanBorcForOgrenciAsync(ogrenciId);
            
            return true;
   }

        public async Task<decimal> GetToplamOdenenTutarAsync(long ogrenciId)
        {
            return await _context.OgrenciOdemeTakvimi
            .Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
   .SumAsync(o => o.OdenenTutar);
    }

  public async Task<decimal> GetKalanBorcAsync(long ogrenciId)
    {
  var sonOdeme = await _context.OgrenciOdemeTakvimi
        .Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
   .OrderByDescending(o => o.OdemeTarihi)
        .ThenByDescending(o => o.Id)
           .FirstOrDefaultAsync();

      return sonOdeme?.BorcTutari ?? 0;
    }

    public async Task<decimal> GetToplamKalanBorcAsync()
    {
     // Aktif öðrencileri getir
        var ogrencilerWithData = await _context.Ogrenciler
            .Where(o => !o.IsDeleted && o.Aktif)
     .Select(o => o.Id)
            .ToListAsync();

        decimal toplamBorc = 0;

        foreach (var ogrenciId in ogrencilerWithData)
        {
            // Öðrencinin tüm ödenmemiþ taksitlerinin taksit tutarlarýný topla
    var odenmemisTaksitlerToplamý = await _context.OgrenciOdemeTakvimi
            .Where(o => o.OgrenciId == ogrenciId && 
        !o.IsDeleted && 
       !o.Odendi)  // Ödenmemiþ taksitler
 .SumAsync(o => o.TaksitTutari ?? 0);

            toplamBorc += odenmemisTaksitlerToplamý;
}

        return toplamBorc;
    }

    /// <summary>
    /// Öðrencinin tüm ödeme kayýtlarý için kalan borcu kronolojik sýrayla yeniden hesaplar
    /// </summary>
    public async Task RecalculateKalanBorcForOgrenciAsync(long ogrenciId)
    {
    // Öðrenciyi ve ödeme planýný getir
        var ogrenci = await _context.Ogrenciler
      .Include(o => o.OdemePlanlari)
       .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

    if (ogrenci == null)
            return;

     // Baþlangýç borcu (ödeme planýndan) - Ödeme planý silinmemiþse
        decimal baslangicBorc = 0;
        if (ogrenci.OdemePlanlari != null && !ogrenci.OdemePlanlari.IsDeleted)
        {
 baslangicBorc = ogrenci.OdemePlanlari.Tutar;
     }

      // Tüm ödemeleri tarihe göre sýrala (eski -> yeni) - Silinmemiþ kayýtlar
        var odemeler = await _context.OgrenciOdemeTakvimi
       .Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
            .OrderBy(o => o.OdemeTarihi ?? DateTime.MaxValue)
            .ThenBy(o => o.Id)
            .ToListAsync();

        // Eðer hiç ödeme yoksa, iþlem yapma
 if (!odemeler.Any())
  return;

        decimal kalanBorc = baslangicBorc;

        // Her ödemeyi sýrayla iþle
        foreach (var odeme in odemeler)
  {
          // Bu ödeme yapýldýktan sonra kalan borç
kalanBorc = Math.Max(0, kalanBorc - odeme.OdenenTutar);
       
       // Kalan borcu güncelle
    odeme.BorcTutari = kalanBorc;
        }

        // Deðiþiklikleri kaydet
        await _context.SaveChangesAsync();
  }

        /// <summary>
     /// Taksiti hýzlýca ödendi olarak iþaretler
    /// </summary>
        public async Task<bool> MarkAsOdendiAsync(long id)
        {
            var odeme = await _context.OgrenciOdemeTakvimi
   .Where(o => !o.IsDeleted)
       .FirstOrDefaultAsync(o => o.Id == id);

            if (odeme == null)
  return false;

      // Eðer zaten ödenmiþse, iþlem yapma
   if (odeme.Odendi)
            return true;

            // Taksit tutarý varsa, ödenen tutarý taksit tutarýna eþitle
     if (odeme.TaksitTutari.HasValue && odeme.TaksitTutari.Value > 0)
  {
       odeme.OdenenTutar = odeme.TaksitTutari.Value;
       }

            // Ödendi olarak iþaretle
            odeme.Odendi = true;

            // Ödeme tarihi yoksa bugünün tarihini ata
 if (!odeme.OdemeTarihi.HasValue)
     {
       odeme.OdemeTarihi = DateTime.Now;
   }

            odeme.Version++;
            await _context.SaveChangesAsync();

          // Kalan borçlarý yeniden hesapla
            await RecalculateKalanBorcForOgrenciAsync(odeme.OgrenciId);

      return true;
}
    }
}
