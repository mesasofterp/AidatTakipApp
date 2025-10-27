using StudentApp.Models;

namespace StudentApp.Services
{
    public interface ICinsiyetlerService
    {
        Task<IEnumerable<Cinsiyetler>> GetAllCinsiyetlerAsync();
 Task<Cinsiyetler?> GetCinsiyetByIdAsync(long id);
 }
}
