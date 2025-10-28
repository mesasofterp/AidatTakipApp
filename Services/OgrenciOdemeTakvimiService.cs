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

            _context.OgrenciOdemeTakvimi.Add(odeme);
   await _context.SaveChangesAsync();
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

     await _context.SaveChangesAsync();
         return existingOdeme;
        }

public async Task<bool> DeleteOdemeAsync(long id)
  {
   var odeme = await _context.OgrenciOdemeTakvimi
       .Where(o => !o.IsDeleted)
.FirstOrDefaultAsync(o => o.Id == id);
          
 if (odeme == null)
     return false;

     // Soft delete
odeme.IsDeleted = true;
   odeme.Aktif = false;
   await _context.SaveChangesAsync();
      
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
        .FirstOrDefaultAsync();

   return sonOdeme?.BorcTutari ?? 0;
        }

        public async Task<decimal> GetToplamKalanBorcAsync()
        {
  // Her öðrenci için son ödeme kaydýndaki kalan borcu al
       var ogrenciler = await _context.Ogrenciler
  .Where(o => !o.IsDeleted && o.Aktif)
    .Select(o => o.Id)
         .ToListAsync();

      decimal toplamBorc = 0;

   foreach (var ogrenciId in ogrenciler)
            {
            var sonOdeme = await _context.OgrenciOdemeTakvimi
.Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
 .OrderByDescending(o => o.OdemeTarihi)
     .ThenByDescending(o => o.Id)
                  .FirstOrDefaultAsync();

      if (sonOdeme != null)
     {
     toplamBorc += sonOdeme.BorcTutari;
       }
        else
        {
      // Hiç ödeme yoksa, ödeme planýndan toplam tutarý al
 var ogrenci = await _context.Ogrenciler
     .Include(o => o.OdemePlanlari)
   .FirstOrDefaultAsync(o => o.Id == ogrenciId);

         if (ogrenci?.OdemePlanlari != null)
     {
          toplamBorc += ogrenci.OdemePlanlari.Tutar;
        }
       }
     }

            return toplamBorc;
     }
    }
}
