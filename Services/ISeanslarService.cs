using StudentApp.Models;

namespace StudentApp.Services
{
    public interface ISeanslarService
    {
        Task<IEnumerable<Seanslar>> GetAllSeanslarAsync();
        Task<Seanslar?> GetSeansByIdAsync(long id);
        Task<Seanslar> AddSeansAsync(Seanslar seans);
        Task<Seanslar?> UpdateSeansAsync(Seanslar seans);
        Task<bool> DeleteSeansAsync(long id);
        Task<IEnumerable<Seanslar>> GetSeansByGunIdAsync(long gunId);
        Task<bool> CheckSeansKapasitesiAsync(long seansId);
        Task UpdateSeansMevcuduAsync(long seansId, int delta);
        Task RecalculateSeansMevcuduAsync(long seansId);
    }
}
