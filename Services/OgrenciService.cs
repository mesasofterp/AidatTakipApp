using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OgrenciService : IOgrencilerService
    {
        private readonly AppDbContext _context;

        public OgrenciService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync()
        {
            return await GetAllOgrenciAsync(false);
        }

        public async Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync(bool includePasif)
        {
            var query = _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Where(s => !s.IsDeleted);

            if (!includePasif)
            {
                query = query.Where(s => s.Aktif);
            }

            return await query
                .OrderBy(s => s.OgrenciSoyadi)
                .ThenBy(s => s.OgrenciAdi)
                .ToListAsync();
        }

        public async Task<Ogrenciler?> GetOgrenciByIdAsync(long id)
        {
            return await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Ogrenciler> AddOgrenciAsync(Ogrenciler ogrenci)
        {
            ogrenci.IsDeleted = false;
            ogrenci.Aktif = true;
            ogrenci.Version = 0;

            _context.Ogrenciler.Add(ogrenci);
            await _context.SaveChangesAsync();
            return ogrenci;
        }

        public async Task<Ogrenciler?> UpdateOgrenciAsync(Ogrenciler ogrenci)
        {
            var existingOgrenci = await _context.Ogrenciler
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == ogrenci.Id);

            if (existingOgrenci == null)
                return null;

            existingOgrenci.OgrenciAdi = ogrenci.OgrenciAdi;
            existingOgrenci.OgrenciSoyadi = ogrenci.OgrenciSoyadi;
            existingOgrenci.Email = ogrenci.Email;
            existingOgrenci.Telefon = ogrenci.Telefon;
            existingOgrenci.TCNO = ogrenci.TCNO;
            existingOgrenci.Boy = ogrenci.Boy;
            existingOgrenci.Kilo = ogrenci.Kilo;
            existingOgrenci.Adres = ogrenci.Adres;
            existingOgrenci.KayitTarihi = ogrenci.KayitTarihi;
            existingOgrenci.DogumTarihi = ogrenci.DogumTarihi;
            existingOgrenci.CinsiyetId = ogrenci.CinsiyetId;
            existingOgrenci.OdemePlanlariId = ogrenci.OdemePlanlariId;
            existingOgrenci.Aciklama = ogrenci.Aciklama;
            existingOgrenci.Version++;

            await _context.SaveChangesAsync();
            return existingOgrenci;
        }

        public async Task<bool> DeleteOgrenciAsync(long id)
        {
            var ogrenci = await _context.Ogrenciler
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ogrenci == null)
                return false;

            // Soft delete - IsDeleted'ý true yap
            ogrenci.IsDeleted = true;
            ogrenci.Aktif = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleAktifAsync(long id, bool aktif)
        {
            var ogrenci = await _context.Ogrenciler
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ogrenci == null)
                return false;

            ogrenci.Aktif = aktif;
            ogrenci.Version++;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
