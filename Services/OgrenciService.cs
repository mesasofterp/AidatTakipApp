using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class OgrenciService : IOgrencilerService
    {
        private readonly AppDbContext _context;

        public OgrenciService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync()
        {
            return await GetAllOgrenciAsync(false);
        }

        public async Task<IEnumerable<Ogrenciler>> GetAllOgrenciAsync(bool includePasif)
        {
            var query = _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Where(s => !s.IsDeleted);

            if (!includePasif)
            {
                query = query.Where(s => s.Aktif);
            }

            return await query
                .OrderBy(s => s.OgrenciSoyadi)
                .ThenBy(s => s.OgrenciAdi)
                .ToListAsync();
        }

        public async Task<Ogrenciler?> GetOgrenciByIdAsync(long id)
        {
            return await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Ogrenciler> AddOgrenciAsync(Ogrenciler ogrenci)
        {
            // Email kontrolü - Silinmemiþ kayýtlar arasýnda kontrol et
            var existingEmail = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Email == ogrenci.Email)
                .FirstOrDefaultAsync();

            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Bu e-posta adresi ({ogrenci.Email}) zaten kullanýlýyor.");
            }

            // TC No kontrolü (eðer girilmiþse) - Silinmemiþ kayýtlar arasýnda kontrol et
            if (!string.IsNullOrWhiteSpace(ogrenci.TCNO))
            {
                var existingTCNO = await _context.Ogrenciler
                    .Where(o => !o.IsDeleted && o.TCNO == ogrenci.TCNO)
                    .FirstOrDefaultAsync();

                if (existingTCNO != null)
                {
                    throw new InvalidOperationException($"Bu TC Kimlik No ({ogrenci.TCNO}) zaten kullanýlýyor.");
                }
            }

            ogrenci.IsDeleted = false;
            ogrenci.Aktif = true;
            ogrenci.Version = 0;

            _context.Ogrenciler.Add(ogrenci);
            await _context.SaveChangesAsync();

            // Öðrenci oluþturulduktan sonra taksitleri oluþtur
            await CreateTaksitlerForOgrenciAsync(ogrenci.Id);

            return ogrenci;
        }

        public async Task<Ogrenciler?> UpdateOgrenciAsync(Ogrenciler ogrenci)
        {
            var existingOgrenci = await _context.Ogrenciler
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == ogrenci.Id);

            if (existingOgrenci == null)
                return null;

            // Email kontrolü - Kendi kaydý hariç, silinmemiþ diðer kayýtlarda ayný email var mý?
            var duplicateEmail = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Id != ogrenci.Id && o.Email == ogrenci.Email)
                .FirstOrDefaultAsync();

            if (duplicateEmail != null)
            {
                throw new InvalidOperationException($"Bu e-posta adresi ({ogrenci.Email}) baþka bir öðrenci tarafýndan kullanýlýyor.");
            }

            // TC No kontrolü - Kendi kaydý hariç, silinmemiþ diðer kayýtlarda ayný TC var mý?
            if (!string.IsNullOrWhiteSpace(ogrenci.TCNO))
            {
                var duplicateTCNO = await _context.Ogrenciler
                    .Where(o => !o.IsDeleted && o.Id != ogrenci.Id && o.TCNO == ogrenci.TCNO)
                    .FirstOrDefaultAsync();

                if (duplicateTCNO != null)
                {
                    throw new InvalidOperationException($"Bu TC Kimlik No ({ogrenci.TCNO}) baþka bir öðrenci tarafýndan kullanýlýyor.");
                }
            }

            existingOgrenci.OgrenciAdi = ogrenci.OgrenciAdi;
            existingOgrenci.OgrenciSoyadi = ogrenci.OgrenciSoyadi;
            existingOgrenci.Email = ogrenci.Email;
            existingOgrenci.Telefon = ogrenci.Telefon;
            existingOgrenci.TCNO = ogrenci.TCNO;
            existingOgrenci.Boy = ogrenci.Boy;
            existingOgrenci.Kilo = ogrenci.Kilo;
            existingOgrenci.Adres = ogrenci.Adres;
            existingOgrenci.KayitTarihi = ogrenci.KayitTarihi;
            existingOgrenci.DogumTarihi = ogrenci.DogumTarihi;
            existingOgrenci.CinsiyetId = ogrenci.CinsiyetId;
            existingOgrenci.OdemePlanlariId = ogrenci.OdemePlanlariId;
            existingOgrenci.Aciklama = ogrenci.Aciklama;
            existingOgrenci.Version++;

            await _context.SaveChangesAsync();
            return existingOgrenci;
        }

        public async Task<bool> DeleteOgrenciAsync(long id)
        {
            var ogrenci = await _context.Ogrenciler
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ogrenci == null)
                return false;

            // Soft delete - IsDeleted'ý true yap
            ogrenci.IsDeleted = true;
            ogrenci.Aktif = false;

            // Öðrenciye ait tüm ödeme takvimlerini de soft delete yap
            var odemeTakvimleri = await _context.OgrenciOdemeTakvimi
                .Where(o => o.OgrenciId == id && !o.IsDeleted)
                .ToListAsync();

            foreach (var odeme in odemeTakvimleri)
            {
                odeme.IsDeleted = true;
                odeme.Aktif = false;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleAktifAsync(long id, bool aktif)
        {
            var ogrenci = await _context.Ogrenciler
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ogrenci == null)
                return false;

            ogrenci.Aktif = aktif;
            ogrenci.Version++;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Öðrenci için ödeme planýna göre taksit kayýtlarý oluþturur
        /// </summary>
        public async Task CreateTaksitlerForOgrenciAsync(long ogrenciId)
        {
            var ogrenci = await _context.Ogrenciler
                .Include(o => o.OdemePlanlari)
                .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

            if (ogrenci == null || ogrenci.OdemePlanlari == null || ogrenci.OdemePlanlari.IsDeleted)
                return;

            var odemePlani = ogrenci.OdemePlanlari;

            // Taksit sayýsý ve toplam tutar
            int taksitSayisi = odemePlani.Taksit;
            decimal toplamTutar = odemePlani.Tutar;

            // Taksit baþýna düþen tutar
            decimal taksitTutari = Math.Round(toplamTutar / taksitSayisi, 2);

            // Vade hesaplama: Vade varsa taksite böl, yoksa varsayýlan 30 gün
            int vadeSuresi = odemePlani.Vade.HasValue ? odemePlani.Vade.Value : (taksitSayisi * 30);
            int taksitBasinaGun = vadeSuresi / taksitSayisi;

            // Baþlangýç tarihi olarak kayýt tarihi
            DateTime baslangicTarihi = ogrenci.KayitTarihi;

            // Kalan borç takibi
            decimal kalanBorc = toplamTutar;

            // Taksitleri oluþtur
            for (int i = 1; i <= taksitSayisi; i++)
            {
                // Son taksitte kalan tutarý tam olarak hesapla (yuvarlama farký için)
                decimal buTaksitTutari = (i == taksitSayisi) ? kalanBorc : taksitTutari;

                // Taksit son ödeme tarihi
                DateTime sonOdemeTarihi = baslangicTarihi.AddDays(i * taksitBasinaGun);

                var taksit = new OgrenciOdemeTakvimi
                {
                    OgrenciId = ogrenciId,
                    TaksitNo = i,
                    TaksitTutari = buTaksitTutari,  // Taksit tutarýný sakla
                    SonOdemeTarihi = sonOdemeTarihi,
                    OdenenTutar = 0, // Henüz ödenmedi
                    BorcTutari = kalanBorc,
                    Odendi = false,
                    SmsGittiMi = false,  // SMS henüz gönderilmedi
                    OdemeTarihi = null, // Ödeme yapýlmadý
                    OlusturmaTarihi = DateTime.Now,
                    Aktif = true,
                    IsDeleted = false,
                    Version = 0
                };

                _context.OgrenciOdemeTakvimi.Add(taksit);

                // Kalan borcu güncelle
                kalanBorc -= buTaksitTutari;
            }

            await _context.SaveChangesAsync();
        }
    }
}
