using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOdemePlanlariService
    {
        Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync();
        Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync(bool includePasif);
        Task<OdemePlanlari?> GetOdemePlaniByIdAsync(long id);
        Task<OdemePlanlari> AddOdemePlaniAsync(OdemePlanlari odemePlani);
        Task<OdemePlanlari?> UpdateOdemePlaniAsync(OdemePlanlari odemePlani);
        Task<bool> DeleteOdemePlaniAsync(long id);
        Task<bool> ToggleAktifAsync(long id, bool aktif);
    }
}
