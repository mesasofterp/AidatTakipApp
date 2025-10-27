using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOgrenciOdemeTakvimiService
    {
        Task<IEnumerable<OgrenciOdemeTakvimi>> GetAllOdemelerAsync();
        Task<IEnumerable<OgrenciOdemeTakvimi>> GetOdemelerByOgrenciIdAsync(long ogrenciId);
        Task<OgrenciOdemeTakvimi?> GetOdemeByIdAsync(long id);
        Task<OgrenciOdemeTakvimi> AddOdemeAsync(OgrenciOdemeTakvimi odeme);
        Task<OgrenciOdemeTakvimi?> UpdateOdemeAsync(OgrenciOdemeTakvimi odeme);
        Task<bool> DeleteOdemeAsync(long id);
        Task<decimal> GetToplamOdenenTutarAsync(long ogrenciId);
        Task<decimal> GetKalanBorcAsync(long ogrenciId);
        Task<decimal> GetToplamKalanBorcAsync();
    }
}
