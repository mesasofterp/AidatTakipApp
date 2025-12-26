using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OdemePlanlariService : IOdemePlanlariService
    {
        private readonly AppDbContext _context;

        public OdemePlanlariService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync()
        {
            return await GetAllOdemePlanlariAsync(false);
        }

        public async Task<IEnumerable<OdemePlanlari>> GetAllOdemePlanlariAsync(bool includePasif)
        {
            var query = _context.OdemePlanlari
                .Where(s => !s.IsDeleted);

            if (!includePasif)
            {
                query = query.Where(s => s.Aktif);
            }

            return await query
                .OrderBy(s => s.KursProgrami)
                .ThenBy(s => s.TaksitTutari)
                .ToListAsync();
        }

        public async Task<OdemePlanlari?> GetOdemePlaniByIdAsync(long id)
        {
            return await _context.OdemePlanlari
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<OdemePlanlari> AddOdemePlaniAsync(OdemePlanlari odemePlani)
        {
            odemePlani.IsDeleted = false;
            odemePlani.Aktif = true;
            odemePlani.Version = 0;
            
            _context.OdemePlanlari.Add(odemePlani);
            await _context.SaveChangesAsync();
            return odemePlani;
        }

        public async Task<OdemePlanlari?> UpdateOdemePlaniAsync(OdemePlanlari odemePlani)
        {
            var existingOdemePlani = await _context.OdemePlanlari
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == odemePlani.Id);
            
            if (existingOdemePlani == null)
                return null;

            // Eski deðerleri kaydet (deðiþiklik kontrolü için)
            var eskiTaksitTutari = existingOdemePlani.TaksitTutari;
    var eskiTaksitSayisi = existingOdemePlani.TaksitSayisi;

            // Yeni deðerleri ata
    existingOdemePlani.KursProgrami = odemePlani.KursProgrami;
     existingOdemePlani.TaksitSayisi = odemePlani.TaksitSayisi;
            existingOdemePlani.TaksitTutari = odemePlani.TaksitTutari;
       existingOdemePlani.Vade = odemePlani.Vade;
    existingOdemePlani.Aciklama = odemePlani.Aciklama;
            existingOdemePlani.Version++;

  await _context.SaveChangesAsync();

         // Eðer taksit tutarý veya sayýsý deðiþtiyse, bu plana baðlý öðrencilerin ödenmemiþ taksitlerini güncelle
            if (eskiTaksitTutari != odemePlani.TaksitTutari || eskiTaksitSayisi != odemePlani.TaksitSayisi)
      {
    await UpdateOgrenciTaksitleriniAsync(odemePlani.Id, odemePlani.TaksitTutari, odemePlani.TaksitSayisi);
    }

            return existingOdemePlani;
     }

   /// <summary>
      /// Ödeme planýna baðlý öðrencilerin ödenmemiþ taksitlerini günceller
        /// </summary>
  /// <returns>Güncellenen toplam taksit sayýsý ve etkilenen öðrenci sayýsý</returns>
        private async Task<(int ogrenciSayisi, int taksitSayisi)> UpdateOgrenciTaksitleriniAsync(long odemePlaniId, decimal yeniTaksitTutari, int yeniTaksitSayisi)
    {
      // Bu ödeme planýna baðlý öðrencileri getir
 var ogrenciler = await _context.Ogrenciler
      .Where(o => o.OdemePlanlariId == odemePlaniId && !o.IsDeleted && o.Aktif)
          .ToListAsync();

      int toplamOgrenciSayisi = 0;
  int toplamTaksitSayisi = 0;

            foreach (var ogrenci in ogrenciler)
 {
   // Öðrencinin ödenmemiþ taksitlerini getir
       var odenmemisTaksitler = await _context.OgrenciOdemeTakvimi
 .Where(t => t.OgrenciId == ogrenci.Id && 
!t.IsDeleted && 
  !t.Odendi)
 .OrderBy(t => t.TaksitNo)
        .ToListAsync();

             if (!odenmemisTaksitler.Any())
     continue;

  toplamOgrenciSayisi++;
      toplamTaksitSayisi += odenmemisTaksitler.Count;

    // Her ödenmemiþ taksit için yeni tutarý güncelle
     foreach (var taksit in odenmemisTaksitler)
{
// Taksit tutarýný güncelle
        taksit.TaksitTutari = yeniTaksitTutari;
       taksit.Version++;
        }

    await _context.SaveChangesAsync();

      // Borç hesaplamalarýný yeniden yap
      // Bu öðrenci için tüm taksitlerin borç tutarýný yeniden hesapla
      await RecalculateKalanBorcForOgrenciAsync(ogrenci.Id);
     }

        return (toplamOgrenciSayisi, toplamTaksitSayisi);
   }

      /// <summary>
        /// Öðrencinin tüm ödeme kayýtlarý için kalan borcu kronolojik sýrayla yeniden hesaplar
   /// </summary>
        private async Task RecalculateKalanBorcForOgrenciAsync(long ogrenciId)
        {
            // Öðrenciyi ve ödeme planýný getir
          var ogrenci = await _context.Ogrenciler
  .Include(o => o.OdemePlanlari)
     .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

      if (ogrenci == null)
        return;

 // Baþlangýç borcu = Toplam Tutar (ödeme planýndan)
         decimal baslangicBorc = 0;
       if (ogrenci.OdemePlanlari != null && !ogrenci.OdemePlanlari.IsDeleted)
    {
                baslangicBorc = ogrenci.OdemePlanlari.ToplamTutar;
         }

 // Tüm taksitleri TaksitNo'ya göre sýrala
    var taksitler = await _context.OgrenciOdemeTakvimi
 .Where(o => o.OgrenciId == ogrenciId && !o.IsDeleted)
      .OrderBy(o => o.TaksitNo ?? 0)
   .ThenBy(o => o.Id)
            .ToListAsync();

     // Eðer hiç taksit yoksa, iþlem yapma
     if (!taksitler.Any())
                return;

decimal kalanBorc = baslangicBorc;

          // Her taksiti sýrayla iþle
 foreach (var taksit in taksitler)
  {
      // Bu taksitte ÖNCE kalan borç
    taksit.BorcTutari = kalanBorc;

        // Sadece ödenen taksitler için borcu düþ
       if (taksit.Odendi && taksit.OdenenTutar > 0)
     {
   kalanBorc = Math.Max(0, kalanBorc - taksit.OdenenTutar);
       }
    }

 // Deðiþiklikleri kaydet
      await _context.SaveChangesAsync();
}

    public async Task<bool> DeleteOdemePlaniAsync(long id)
        {
    var odemePlani = await _context.OdemePlanlari
       .Where(o => !o.IsDeleted)
    .FirstOrDefaultAsync(o => o.Id == id);
            
      if (odemePlani == null)
        return false;

            // Soft delete - IsDeleted'ý true yap
     odemePlani.IsDeleted = true;
            odemePlani.Aktif = false;
            await _context.SaveChangesAsync();
 
      return true;
  }

        public async Task<bool> ToggleAktifAsync(long id, bool aktif)
   {
            var odemePlani = await _context.OdemePlanlari
          .Where(o => !o.IsDeleted)
         .FirstOrDefaultAsync(o => o.Id == id);
   
   if (odemePlani == null)
      return false;

      odemePlani.Aktif = aktif;
    odemePlani.Version++;
            await _context.SaveChangesAsync();

      return true;
        }
    }
}
