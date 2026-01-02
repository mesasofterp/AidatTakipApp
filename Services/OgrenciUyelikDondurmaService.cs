using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OgrenciUyelikDondurmaService : IOgrenciUyelikDondurmaService
    {
     private readonly AppDbContext _context;
  private readonly ILogger<OgrenciUyelikDondurmaService> _logger;

        public OgrenciUyelikDondurmaService(AppDbContext context, ILogger<OgrenciUyelikDondurmaService> logger)
 {
  _context = context;
     _logger = logger;
        }

        public async Task<IEnumerable<OgrenciUyelikDondurma>> GetAllAsync()
     {
     return await _context.OgrenciUyelikDondurma
       .Include(d => d.Ogrenci)
   .Where(d => !d.IsDeleted)
 .OrderByDescending(d => d.BaslangicTarihi)
         .ToListAsync();
        }

        public async Task<IEnumerable<OgrenciUyelikDondurma>> GetByOgrenciIdAsync(long ogrenciId)
        {
       return await _context.OgrenciUyelikDondurma
        .Include(d => d.Ogrenci)
       .Where(d => d.OgrenciId == ogrenciId && !d.IsDeleted)
    .OrderByDescending(d => d.BaslangicTarihi)
.ToListAsync();
}

        public async Task<OgrenciUyelikDondurma?> GetByIdAsync(long id)
        {
   return await _context.OgrenciUyelikDondurma
        .Include(d => d.Ogrenci)
              .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

public async Task<OgrenciUyelikDondurma?> GetAktifDondurmaAsync(long ogrenciId)
        {
 return await _context.OgrenciUyelikDondurma
    .Where(d => d.OgrenciId == ogrenciId && 
         d.Status == DondurmaStatusEnum.Aktif && 
  !d.IsDeleted)
     .FirstOrDefaultAsync();
        }

  public async Task<OgrenciUyelikDondurma> CreateAsync(OgrenciUyelikDondurma dondurma)
        {
    // Öðrencinin aktif bir dondurmasý var mý kontrol et
          var mevcutAktifDondurma = await GetAktifDondurmaAsync(dondurma.OgrenciId);
       if (mevcutAktifDondurma != null)
      {
     throw new InvalidOperationException($"Öðrencinin zaten aktif bir dondurmasý var (Baþlangýç: {mevcutAktifDondurma.BaslangicTarihi:dd.MM.yyyy})");
    }

 // Tarih kontrolü
   if (dondurma.BitisTarihi <= dondurma.BaslangicTarihi)
      {
      throw new InvalidOperationException("Bitiþ tarihi baþlangýç tarihinden sonra olmalýdýr.");
   }

            dondurma.Status = DondurmaStatusEnum.Aktif;
     dondurma.Aktif = true;
          dondurma.IsDeleted = false;
       dondurma.Version = 0;
   dondurma.OdemeTarihleriAyarlandi = false;
    dondurma.KaydirilanGunSayisi = (dondurma.BitisTarihi - dondurma.BaslangicTarihi).Days + 1;

            _context.OgrenciUyelikDondurma.Add(dondurma);
      await _context.SaveChangesAsync();

   _logger.LogInformation("Üyelik dondurma kaydý oluþturuldu. Öðrenci ID: {OgrenciId}, Baþlangýç: {Baslangic}, Bitiþ: {Bitis}",
          dondurma.OgrenciId, dondurma.BaslangicTarihi, dondurma.BitisTarihi);

  // Ödeme tarihlerini otomatik ayarla
 await OdemeTarihleriniAyarlaAsync(dondurma.Id);

      return dondurma;
       }

public async Task<OgrenciUyelikDondurma?> UpdateAsync(OgrenciUyelikDondurma dondurma)
 {
        var existing = await _context.OgrenciUyelikDondurma
    .FirstOrDefaultAsync(d => d.Id == dondurma.Id && !d.IsDeleted);

   if (existing == null)
            return null;

       // Tarih kontrolü
          if (dondurma.BitisTarihi <= dondurma.BaslangicTarihi)
            {
                throw new InvalidOperationException("Bitiþ tarihi baþlangýç tarihinden sonra olmalýdýr.");
   }

 existing.BaslangicTarihi = dondurma.BaslangicTarihi;
       existing.BitisTarihi = dondurma.BitisTarihi;
   existing.Sebep = dondurma.Sebep;
       existing.Aciklama = dondurma.Aciklama;
      existing.KaydirilanGunSayisi = (dondurma.BitisTarihi - dondurma.BaslangicTarihi).Days + 1;
            existing.Version++;

   await _context.SaveChangesAsync();

            _logger.LogInformation("Üyelik dondurma kaydý güncellendi. ID: {Id}", dondurma.Id);

  return existing;
}

        public async Task<bool> DeleteAsync(long id)
{
       var dondurma = await _context.OgrenciUyelikDondurma
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

     if (dondurma == null)
  return false;

   dondurma.IsDeleted = true;
      dondurma.Aktif = false;
    await _context.SaveChangesAsync();

    _logger.LogInformation("Üyelik dondurma kaydý silindi. ID: {Id}", id);

        return true;
   }

        public async Task<bool> IptalEtAsync(long id, string kullaniciAdi, string iptalNedeni)
        {
         var dondurma = await _context.OgrenciUyelikDondurma
      .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (dondurma == null)
        return false;

         // Dondurma iptal edildiðinde, ödeme tarihleri ayarlandýysa geri al
            if (dondurma.OdemeTarihleriAyarlandi && dondurma.Status == DondurmaStatusEnum.Aktif)
            {
        await OdemeTarihleriniGeriAlAsync(id);
   }

      dondurma.Status = DondurmaStatusEnum.IptalEdildi;
            dondurma.IptalTarihi = DateTime.Now;
    dondurma.IptalEdenKullanici = kullaniciAdi;
            dondurma.IptalNedeni = iptalNedeni;
  dondurma.Version++;

     await _context.SaveChangesAsync();

    _logger.LogInformation("Üyelik dondurma iptal edildi. ID: {Id}, Ýptal Eden: {Kullanici}", id, kullaniciAdi);

            return true;
        }

      /// <summary>
        /// Dondurma iptal edildiðinde ödeme tarihlerini geri alýr
     /// </summary>
 private async Task<bool> OdemeTarihleriniGeriAlAsync(long dondurmaId)
 {
   var dondurma = await _context.OgrenciUyelikDondurma
       .FirstOrDefaultAsync(d => d.Id == dondurmaId && !d.IsDeleted);

     if (dondurma == null || !dondurma.OdemeTarihleriAyarlandi)
         return false;

         // Ödenmemiþ taksitleri getir
var odenmemisTaksitler = await _context.OgrenciOdemeTakvimi
 .Where(t => t.OgrenciId == dondurma.OgrenciId && 
     !t.Odendi && 
 !t.IsDeleted &&
   t.SonOdemeTarihi.HasValue)
    .OrderBy(t => t.TaksitNo)
 .ToListAsync();

       if (!odenmemisTaksitler.Any())
          {
       _logger.LogInformation("Öðrencinin ödenmemiþ taksiti yok. Öðrenci ID: {OgrenciId}", dondurma.OgrenciId);
 return true;
  }

   int kaydirilanGunSayisi = dondurma.KaydirilanGunSayisi;

      // Her ödenmemiþ taksitin son ödeme tarihini geri al (negatif kaydýrma)
    foreach (var taksit in odenmemisTaksitler)
  {
     taksit.SonOdemeTarihi = taksit.SonOdemeTarihi.Value.AddDays(-kaydirilanGunSayisi);
    taksit.Version++;
    }

            await _context.SaveChangesAsync();

         _logger.LogInformation("Dondurma iptal edildi. Öðrencinin {TaksitSayisi} adet taksit tarihi {GunSayisi} gün geri alýndý. Öðrenci ID: {OgrenciId}",
       odenmemisTaksitler.Count, kaydirilanGunSayisi, dondurma.OgrenciId);

         return true;
   }

        /// <summary>
        /// Öðrencinin ödenmemiþ taksitlerinin son ödeme tarihlerini dondurma süresince kaydýrýr
  /// </summary>
        public async Task<bool> OdemeTarihleriniAyarlaAsync(long dondurmaId)
  {
      var dondurma = await _context.OgrenciUyelikDondurma
       .FirstOrDefaultAsync(d => d.Id == dondurmaId && !d.IsDeleted);

 if (dondurma == null)
      return false;

            // Ödenmemiþ taksitleri getir
            var odenmemisTaksitler = await _context.OgrenciOdemeTakvimi
              .Where(t => t.OgrenciId == dondurma.OgrenciId &&
   !t.Odendi &&
           !t.IsDeleted &&
         t.SonOdemeTarihi.HasValue)
            .OrderBy(t => t.TaksitNo)
            .ToListAsync();

if (!odenmemisTaksitler.Any())
            {
   _logger.LogInformation("Öðrencinin ödenmemiþ taksiti yok. Öðrenci ID: {OgrenciId}", dondurma.OgrenciId);
          dondurma.OdemeTarihleriAyarlandi = true;
 await _context.SaveChangesAsync();
        return true;
 }

        int kaydirilanGunSayisi = dondurma.KaydirilanGunSayisi;

         // Her ödenmemiþ taksitin son ödeme tarihini kaydýr
          foreach (var taksit in odenmemisTaksitler)
         {
     taksit.SonOdemeTarihi = taksit.SonOdemeTarihi.Value.AddDays(kaydirilanGunSayisi);
    taksit.Version++;
        }

            dondurma.OdemeTarihleriAyarlandi = true;
 await _context.SaveChangesAsync();

          _logger.LogInformation("Öðrencinin {TaksitSayisi} adet taksit tarihi {GunSayisi} gün kaydýrýldý. Öðrenci ID: {OgrenciId}",
                odenmemisTaksitler.Count, kaydirilanGunSayisi, dondurma.OgrenciId);

     return true;
        }
    }
}
