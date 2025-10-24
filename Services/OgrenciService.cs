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
            return await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .OrderBy(s => s.OgrenciSoyadi)
                .ThenBy(s => s.OgrenciAdi)
                .ToListAsync();
        }

        public async Task<Ogrenciler?> GetOgrenciByIdAsync(long id)
        {
            return await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Ogrenciler> AddOgrenciAsync(Ogrenciler ogrenci)
        {
            _context.Ogrenciler.Add(ogrenci);
            await _context.SaveChangesAsync();
            return ogrenci;
        }

        public async Task<Ogrenciler?> UpdateOgrenciAsync(Ogrenciler ogrenci)
        {
            var existingOgrenci = await _context.Ogrenciler.FindAsync(ogrenci.Id);
            if (existingOgrenci == null)
                return null;

            existingOgrenci.OgrenciAdi = ogrenci.OgrenciAdi;
            existingOgrenci.OgrenciSoyadi = ogrenci.OgrenciSoyadi;
            existingOgrenci.Email = ogrenci.Email;
            existingOgrenci.KayitTarihi = ogrenci.KayitTarihi;
            existingOgrenci.DogumTarihi = ogrenci.DogumTarihi;
            existingOgrenci.CinsiyetId = ogrenci.CinsiyetId;
            existingOgrenci.OdemePlanlariId = ogrenci.OdemePlanlariId;

            await _context.SaveChangesAsync();
            return existingOgrenci;
        }

        public async Task<bool> DeleteOgrenciAsync(long id)
        {
            var ogrenci = await _context.Ogrenciler.FindAsync(id);
            if (ogrenci == null)
                return false;

            _context.Ogrenciler.Remove(ogrenci);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
