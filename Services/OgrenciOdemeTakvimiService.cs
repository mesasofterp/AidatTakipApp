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

       // E�er TaksitTutari kaydedilmi�se ve �denen tutar bu tutara e�it veya b�y�kse "�dendi" true
      if (odeme.TaksitTutari.HasValue && odeme.TaksitTutari.Value > 0)
   {
        if (odeme.OdenenTutar >= odeme.TaksitTutari.Value && odeme.OdenenTutar > 0)
       {
         odeme.Odendi = true;
       }
      }

       _context.OgrenciOdemeTakvimi.Add(odeme);
await _context.SaveChangesAsync();
    
   // Sonraki �demelerin kalan borcunu g�ncelle
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

   existingOdeme.OdemeTarihi = odeme.OdemeTarihi;
    existingOdeme.OdenenTutar = odeme.OdenenTutar;
   existingOdeme.BorcTutari = odeme.BorcTutari;
         existingOdeme.Aciklama = odeme.Aciklama;
    existingOdeme.Version++;

     // E�er TaksitTutari kaydedilmi�se ve �denen tutar bu tutara e�it veya b�y�kse "�dendi" true
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
 
     // T�m �demelerin kalan borcunu yeniden hesapla
    await RecalculateKalanBorcForOgrenciAsync(existingOdeme.OgrenciId);
            
      return existingOdeme;
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
   
          // Kalan �demelerin borcunu yeniden hesapla
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
     // Aktif ��rencileri getir
        var ogrencilerWithData = await _context.Ogrenciler
            .Where(o => !o.IsDeleted && o.Aktif)
     .Select(o => o.Id)
            .ToListAsync();

        decimal toplamBorc = 0;

        foreach (var ogrenciId in ogrencilerWithData)
        {
            // ��rencinin t�m �denmemi� taksitlerinin taksit tutarlar�n� topla
    var odenmemisTaksitlerToplam� = await _context.OgrenciOdemeTakvimi
            .Where(o => o.OgrenciId == ogrenciId && 
        !o.IsDeleted && 
       !o.Odendi)  // �denmemi� taksitler
 .SumAsync(o => o.TaksitTutari ?? 0);

            toplamBorc += odenmemisTaksitlerToplam�;
}

        return toplamBorc;
    }

    /// <summary>
    /// ��rencinin t�m �deme kay�tlar� i�in kalan borcu kronolojik s�rayla yeniden hesaplar
    /// </summary>
    public async Task RecalculateKalanBorcForOgrenciAsync(long ogrenciId)
    {
    // ��renciyi ve �deme plan�n� getir
        var ogrenci = await _context.Ogrenciler
      .Include(o => o.OdemePlanlari)
       .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

    if (ogrenci == null)
            return;

     // Ba�lang�� borcu (�deme plan�ndan) - �deme plan� silinmemi�se
        decimal baslangicBorc = 0;
        if (ogrenci.OdemePlanlari != null && !ogrenci.OdemePlanlari.IsDeleted)
        {
 baslangicBorc = ogrenci.OdemePlanlari.Tutar;
     }

      // T�m �demeleri tarihe g�re s�rala (eski -> yeni) - Silinmemi� kay�tlar
        var odemeler = await _context.OgrenciOdemeTakvimi
       .Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
            .OrderBy(o => o.OdemeTarihi ?? DateTime.MaxValue)
            .ThenBy(o => o.Id)
            .ToListAsync();

        // E�er hi� �deme yoksa, i�lem yapma
 if (!odemeler.Any())
  return;

        decimal kalanBorc = baslangicBorc;

        // Her �demeyi s�rayla i�le
        foreach (var odeme in odemeler)
  {
          // Bu �deme yap�ld�ktan sonra kalan bor�
kalanBorc = Math.Max(0, kalanBorc - odeme.OdenenTutar);
       
       // Kalan borcu g�ncelle
    odeme.BorcTutari = kalanBorc;
        }

        // De�i�iklikleri kaydet
        await _context.SaveChangesAsync();
  }
    }
}
