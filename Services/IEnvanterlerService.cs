using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IEnvanterlerService
    {
        Task<IEnumerable<Envanterler>> GetAllAsync();
        Task<IEnumerable<Envanterler>> GetActiveAsync();
   Task<Envanterler?> GetByIdAsync(long id);
    Task<Envanterler> CreateAsync(Envanterler envanter);
        Task<Envanterler> UpdateAsync(Envanterler envanter);
        Task<bool> DeleteAsync(long id);
      Task<bool> SoftDeleteAsync(long id);
  Task<decimal> GetToplamDegerAsync();
    }
}
