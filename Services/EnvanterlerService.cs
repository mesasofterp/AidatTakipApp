using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class EnvanterlerService : IEnvanterlerService
  {
        private readonly AppDbContext _context;
        private readonly ILogger<EnvanterlerService> _logger;

        public EnvanterlerService(AppDbContext context, ILogger<EnvanterlerService> logger)
        {
            _context = context;
   _logger = logger;
        }

        public async Task<IEnumerable<Envanterler>> GetAllAsync()
        {
       try
            {
      return await _context.Envanterler
    .Where(e => !e.IsDeleted)
   .OrderBy(e => e.EnvanterAdi)
         .ToListAsync();
            }
       catch (Exception ex)
            {
  _logger.LogError(ex, "Envanterler getirilirken hata oluþtu");
                throw;
  }
        }

        public async Task<IEnumerable<Envanterler>> GetActiveAsync()
      {
    try
   {
         return await _context.Envanterler
            .Where(e => !e.IsDeleted && e.Aktif)
    .OrderBy(e => e.EnvanterAdi)
        .ToListAsync();
   }
      catch (Exception ex)
   {
        _logger.LogError(ex, "Aktif envanterler getirilirken hata oluþtu");
 throw;
      }
        }

 public async Task<Envanterler?> GetByIdAsync(long id)
        {
       try
         {
         return await _context.Envanterler
          .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
     }
          catch (Exception ex)
            {
     _logger.LogError(ex, "Envanter getirilirken hata oluþtu. ID: {Id}", id);
            throw;
            }
        }

        public async Task<Envanterler> CreateAsync(Envanterler envanter)
        {
            try
            {
   envanter.Aktif = true;
        envanter.IsDeleted = false;
          envanter.Version = 0;

      _context.Envanterler.Add(envanter);
    await _context.SaveChangesAsync();

        _logger.LogInformation("Yeni envanter oluþturuldu. ID: {Id}, Adý: {Ad}", 
   envanter.Id, envanter.EnvanterAdi);

           return envanter;
     }
    catch (Exception ex)
      {
  _logger.LogError(ex, "Envanter oluþturulurken hata oluþtu");
          throw;
            }
        }

        public async Task<Envanterler> UpdateAsync(Envanterler envanter)
 {
            try
  {
           var existingEnvanter = await _context.Envanterler
     .FirstOrDefaultAsync(e => e.Id == envanter.Id && !e.IsDeleted);

    if (existingEnvanter == null)
           {
               throw new InvalidOperationException("Envanter bulunamadý");
     }

                // Optimistic concurrency check
          if (existingEnvanter.Version != envanter.Version)
     {
  throw new DbUpdateConcurrencyException("Bu envanter baþka bir kullanýcý tarafýndan güncellenmiþ");
        }

           existingEnvanter.EnvanterAdi = envanter.EnvanterAdi;
        existingEnvanter.Adet = envanter.Adet;
     existingEnvanter.BirimFiyat = envanter.BirimFiyat;
      existingEnvanter.Aktif = envanter.Aktif;
     existingEnvanter.Aciklama = envanter.Aciklama;
     existingEnvanter.Version++;

          await _context.SaveChangesAsync();

    _logger.LogInformation("Envanter güncellendi. ID: {Id}, Adý: {Ad}", 
     envanter.Id, envanter.EnvanterAdi);

                return existingEnvanter;
      }
        catch (Exception ex)
    {
            _logger.LogError(ex, "Envanter güncellenirken hata oluþtu. ID: {Id}", envanter.Id);
         throw;
     }
   }

        public async Task<bool> DeleteAsync(long id)
{
            try
            {
                var envanter = await _context.Envanterler.FindAsync(id);
             
if (envanter == null)
             {
   return false;
    }

   _context.Envanterler.Remove(envanter);
      await _context.SaveChangesAsync();

       _logger.LogInformation("Envanter kalýcý olarak silindi. ID: {Id}", id);
  return true;
            }
         catch (Exception ex)
            {
          _logger.LogError(ex, "Envanter silinirken hata oluþtu. ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> SoftDeleteAsync(long id)
        {
            try
            {
   var envanter = await _context.Envanterler
   .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

          if (envanter == null)
         {
      return false;
    }

    envanter.IsDeleted = true;
      envanter.Aktif = false;
    await _context.SaveChangesAsync();

      _logger.LogInformation("Envanter soft delete yapýldý. ID: {Id}", id);
       return true;
            }
            catch (Exception ex)
    {
     _logger.LogError(ex, "Envanter soft delete yapýlýrken hata oluþtu. ID: {Id}", id);
           throw;
            }
        }

        public async Task<decimal> GetToplamDegerAsync()
        {
            try
            {
          return await _context.Envanterler
   .Where(e => !e.IsDeleted && e.Aktif)
             .SumAsync(e => e.Adet * e.BirimFiyat);
            }
  catch (Exception ex)
         {
 _logger.LogError(ex, "Toplam envanter deðeri hesaplanýrken hata oluþtu");
       throw;
          }
        }
    }
}
