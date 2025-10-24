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
            return await _context.OdemePlanlari
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.KursProgrami)
                .ThenBy(s => s.Tutar)
                .ToListAsync();
        }

        public async Task<OdemePlanlari?> GetOdemePlaniByIdAsync(long id)
        {
            return await _context.OdemePlanlari
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<OdemePlanlari> AddOdemePlaniAsync(OdemePlanlari odemePlani)
        {
            _context.OdemePlanlari.Add(odemePlani);
            await _context.SaveChangesAsync();
            return odemePlani;
        }

        public async Task<OdemePlanlari?> UpdateOdemePlaniAsync(OdemePlanlari odemePlani)
        {
            var existingOdemePlani = await _context.OdemePlanlari.FindAsync(odemePlani.Id);
            if (existingOdemePlani == null)
                return null;

            existingOdemePlani.KursProgrami = odemePlani.KursProgrami;
            existingOdemePlani.Taksit = odemePlani.Taksit;
            existingOdemePlani.Tutar = odemePlani.Tutar;
            existingOdemePlani.Vade = odemePlani.Vade;

            await _context.SaveChangesAsync();
            return existingOdemePlani;
        }

        public async Task<bool> DeleteOdemePlaniAsync(long id)
        {
            var odemePlani = await _context.OdemePlanlari.FindAsync(id);
            if (odemePlani == null)
                return false;

            odemePlani.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
