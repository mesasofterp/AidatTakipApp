using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOgrencilerService
    {
        Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync();
        Task<Ogrenciler?> GetOgrenciByIdAsync(long id);
        Task<Ogrenciler> AddOgrenciAsync(Ogrenciler ogrenci);
        Task<Ogrenciler?> UpdateOgrenciAsync(Ogrenciler ogrenci);
        Task<bool> DeleteOgrenciAsync(long id);
    }
}
