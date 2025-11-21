using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class GunlerService : IGunlerService
    {
  private readonly AppDbContext _context;

        public GunlerService(AppDbContext context)
 {
   _context = context;
  }

        public async Task<IEnumerable<Gunler>> GetAllGunlerAsync()
        {
  return await _context.Gunler
       .OrderBy(g => g.Id)
      .ToListAsync();
        }

        public async Task<Gunler?> GetGunByIdAsync(long id)
{
 return await _context.Gunler
        .FirstOrDefaultAsync(g => g.Id == id);
  }
    }
}
