using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOdemePlanlariService
    {
        Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync();
        Task<OdemePlanlari?> GetOdemePlaniByIdAsync(long id);
        Task<OdemePlanlari> AddOdemePlaniAsync(OdemePlanlari odemePlani);
        Task<OdemePlanlari?> UpdateOdemePlaniAsync(OdemePlanlari odemePlani);
        Task<bool> DeleteOdemePlaniAsync(long id);
    }
}
