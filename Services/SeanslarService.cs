using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class SeanslarService : ISeanslarService
 {
        private readonly AppDbContext _context;

        public SeanslarService(AppDbContext context)
 {
         _context = context;
        }

        public async Task<IEnumerable<Seanslar>> GetAllSeanslarAsync()
        {
      return await _context.Seanslar
    .Include(s => s.Gun)
    .Where(s => !s.IsDeleted)
     .OrderBy(s => s.Gun.Gun)
.ThenBy(s => s.SeansBaslangicSaati)
       .ToListAsync();
        }

     public async Task<Seanslar?> GetSeansByIdAsync(long id)
        {
  return await _context.Seanslar
     .Include(s => s.Gun)
       .Where(s => !s.IsDeleted)
    .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Seanslar> AddSeansAsync(Seanslar seans)
        {
       seans.IsDeleted = false;
       seans.Aktif = true;
  seans.Version = 0;

            _context.Seanslar.Add(seans);
  await _context.SaveChangesAsync();

        return seans;
  }

        public async Task<Seanslar?> UpdateSeansAsync(Seanslar seans)
        {
var existingSeans = await _context.Seanslar
     .Where(s => !s.IsDeleted)
      .FirstOrDefaultAsync(s => s.Id == seans.Id);

            if (existingSeans == null)
        return null;

          existingSeans.SeansAdi = seans.SeansAdi;
    existingSeans.GunId = seans.GunId;
   existingSeans.SeansBaslangicSaati = seans.SeansBaslangicSaati;
          existingSeans.SeansBitisSaati = seans.SeansBitisSaati;
    existingSeans.SeansKapasitesi = seans.SeansKapasitesi;
 existingSeans.SeansMevcudu = seans.SeansMevcudu;
            existingSeans.Aciklama = seans.Aciklama;
     existingSeans.Version++;

    await _context.SaveChangesAsync();
        return existingSeans;
      }

        public async Task<bool> DeleteSeansAsync(long id)
        {
            var seans = await _context.Seanslar
         .Where(s => !s.IsDeleted)
              .FirstOrDefaultAsync(s => s.Id == id);

            if (seans == null)
       return false;

    // Bu seansa kayýtlý öðrencileri kontrol et
       var ogrencilerCount = await _context.Ogrenciler
    .Where(o => o.SeansId == id && !o.IsDeleted)
                .CountAsync();

          if (ogrencilerCount > 0)
{
                throw new InvalidOperationException($"Bu seansa kayýtlý {ogrencilerCount} öðrenci bulunmaktadýr. Önce öðrencilerin seans bilgilerini güncelleyiniz.");
            }

         // Soft delete
       seans.IsDeleted = true;
   seans.Aktif = false;

   await _context.SaveChangesAsync();
            return true;
   }

        public async Task<IEnumerable<Seanslar>> GetSeansByGunIdAsync(long gunId)
        {
            return await _context.Seanslar
 .Include(s => s.Gun)
      .Where(s => s.GunId == gunId && !s.IsDeleted)
                .OrderBy(s => s.SeansBaslangicSaati)
             .ToListAsync();
        }

        public async Task<bool> CheckSeansKapasitesiAsync(long seansId)
   {
            var seans = await GetSeansByIdAsync(seansId);
         if (seans == null || !seans.SeansKapasitesi.HasValue)
   return true;

            var mevcutOgrenciSayisi = await _context.Ogrenciler
                .Where(o => o.SeansId == seansId && !o.IsDeleted && o.Aktif)
                .CountAsync();

     return mevcutOgrenciSayisi < seans.SeansKapasitesi.Value;
        }
    }
}
