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
     .Include(s => s.SeansGunler)
     .Where(s => !s.IsDeleted)
     .OrderBy(s => s.SeansAdi)
   .ToListAsync();
        }

    public async Task<Seanslar?> GetSeansByIdAsync(long id)
      {
        return await _context.Seanslar
                .Include(s => s.SeansGunler)
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
            // GunId artýk NotMapped, güncellemiyoruz
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
      // SeansGunler üzerinden günlük seanslarý getir
     return await _context.SeansGunler
       .Include(sg => sg.Seans)
     .Where(sg => sg.GunId == gunId && !sg.IsDeleted && !sg.Seans.IsDeleted)
            .Select(sg => sg.Seans)
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

public async Task UpdateSeansMevcuduAsync(long seansId, int delta)
        {
            var seans = await _context.Seanslar.FindAsync(seansId);
            if (seans != null)
        {
      seans.SeansMevcudu = (seans.SeansMevcudu ?? 0) + delta;
                if (seans.SeansMevcudu < 0) seans.SeansMevcudu = 0;
      await _context.SaveChangesAsync();
   }
        }

        public async Task RecalculateSeansMevcuduAsync(long seansId)
        {
            var seans = await _context.Seanslar.FindAsync(seansId);
   if (seans != null)
            {
         var mevcutOgrenciSayisi = await _context.Ogrenciler
    .Where(o => o.SeansId == seansId && !o.IsDeleted && o.Aktif)
   .CountAsync();
             seans.SeansMevcudu = mevcutOgrenciSayisi;
       await _context.SaveChangesAsync();
        }
        }
    }
}
