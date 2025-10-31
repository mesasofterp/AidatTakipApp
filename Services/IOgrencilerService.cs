using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOgrencilerService
    {
        Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync();
        Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync(bool includePasif);
        Task<Ogrenciler?> GetOgrenciByIdAsync(long id);
        Task<Ogrenciler> AddOgrenciAsync(Ogrenciler ogrenci);
        Task<Ogrenciler?> UpdateOgrenciAsync(Ogrenciler ogrenci);
        Task<bool> DeleteOgrenciAsync(long id);
        Task<bool> ToggleAktifAsync(long id, bool aktif);
        Task CreateTaksitlerForOgrenciAsync(long ogrenciId);
    }
}
