using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OdemePlanlariService : IOdemePlanlariService
    {
        private readonly AppDbContext _context;

        public OdemePlanlariService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync()
        {
            return await GetAllOdemePlanlariAsync(false);
        }

        public async Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync(bool includePasif)
        {
            var query = _context.OdemePlanlari
                .Where(s => !s.IsDeleted);

            if (!includePasif)
            {
                query = query.Where(s => s.Aktif);
            }

            return await query
                .OrderBy(s => s.KursProgrami)
                .ThenBy(s => s.Tutar)
                .ToListAsync();
        }

        public async Task<OdemePlanlari?> GetOdemePlaniByIdAsync(long id)
        {
            return await _context.OdemePlanlari
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<OdemePlanlari> AddOdemePlaniAsync(OdemePlanlari odemePlani)
        {
            odemePlani.IsDeleted = false;
            odemePlani.Aktif = true;
            odemePlani.Version = 0;
            
            _context.OdemePlanlari.Add(odemePlani);
            await _context.SaveChangesAsync();
            return odemePlani;
        }

        public async Task<OdemePlanlari?> UpdateOdemePlaniAsync(OdemePlanlari odemePlani)
        {
            var existingOdemePlani = await _context.OdemePlanlari
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == odemePlani.Id);
            
            if (existingOdemePlani == null)
                return null;

            existingOdemePlani.KursProgrami = odemePlani.KursProgrami;
            existingOdemePlani.Taksit = odemePlani.Taksit;
            existingOdemePlani.Tutar = odemePlani.Tutar;
            existingOdemePlani.Vade = odemePlani.Vade;
            existingOdemePlani.Version++;

            await _context.SaveChangesAsync();
            return existingOdemePlani;
        }

        public async Task<bool> DeleteOdemePlaniAsync(long id)
        {
            var odemePlani = await _context.OdemePlanlari
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (odemePlani == null)
                return false;

            // Soft delete - IsDeleted'ý true yap
            odemePlani.IsDeleted = true;
            odemePlani.Aktif = false;
            await _context.SaveChangesAsync();
 
            return true;
        }

        public async Task<bool> ToggleAktifAsync(long id, bool aktif)
        {
            var odemePlani = await _context.OdemePlanlari
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == id);
   
            if (odemePlani == null)
                return false;

            odemePlani.Aktif = aktif;
            odemePlani.Version++;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
