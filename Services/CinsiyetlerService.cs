using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
 public class CinsiyetlerService : ICinsiyetlerService
    {
        private readonly AppDbContext _context;

  public CinsiyetlerService(AppDbContext context)
        {
            _context = context;
  }

        public async Task<IEnumerable<Cinsiyetler>> GetAllCinsiyetlerAsync()
      {
   return await _context.Cinsiyetler
    .OrderBy(c => c.Cinsiyet)
         .ToListAsync();
 }

        public async Task<Cinsiyetler?> GetCinsiyetByIdAsync(long id)
        {
   return await _context.Cinsiyetler
           .FirstOrDefaultAsync(c => c.Id == id);
      }
    }
}
