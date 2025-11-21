using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IGunlerService
    {
    Task<IEnumerable<Gunler>> GetAllGunlerAsync();
        Task<Gunler?> GetGunByIdAsync(long id);
    }
}
