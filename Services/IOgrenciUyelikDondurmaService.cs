using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOgrenciUyelikDondurmaService
    {
        Task<IEnumerable<OgrenciUyelikDondurma>> GetAllAsync();
        Task<IEnumerable<OgrenciUyelikDondurma>> GetByOgrenciIdAsync(long ogrenciId);
        Task<OgrenciUyelikDondurma?> GetByIdAsync(long id);
        Task<OgrenciUyelikDondurma> CreateAsync(OgrenciUyelikDondurma dondurma);
       Task<OgrenciUyelikDondurma?> UpdateAsync(OgrenciUyelikDondurma dondurma);
        Task<bool> DeleteAsync(long id);
 Task<bool> IptalEtAsync(long id, string kullaniciAdi, string iptalNedeni);
        Task<OgrenciUyelikDondurma?> GetAktifDondurmaAsync(long ogrenciId);
      Task<bool> OdemeTarihleriniAyarlaAsync(long dondurmaId);
    }
}
