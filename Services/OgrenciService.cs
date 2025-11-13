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
                .Include(s => s.OgrenciDetay)
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Ogrenciler> AddOgrenciAsync(Ogrenciler ogrenci)
        {
            // Email kontrol� - Silinmemi� kay�tlar aras�nda kontrol et
            var existingEmail = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Email == ogrenci.Email)
                .FirstOrDefaultAsync();

            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Bu e-posta adresi ({ogrenci.Email}) zaten kullan�l�yor.");
            }

            // TC No kontrol� (e�er girilmi�se) - Silinmemi� kay�tlar aras�nda kontrol et
            if (!string.IsNullOrWhiteSpace(ogrenci.TCNO))
            {
                var existingTCNO = await _context.Ogrenciler
                    .Where(o => !o.IsDeleted && o.TCNO == ogrenci.TCNO)
                    .FirstOrDefaultAsync();

                if (existingTCNO != null)
                {
                    throw new InvalidOperationException($"Bu TC Kimlik No ({ogrenci.TCNO}) zaten kullan�l�yor.");
                }
            }

            ogrenci.IsDeleted = false;
            ogrenci.Aktif = true;
            ogrenci.Version = 0;

            _context.Ogrenciler.Add(ogrenci);
            await _context.SaveChangesAsync();

            // ��renci olu�turulduktan sonra taksitleri olu�tur
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

            // Email kontrol� - Kendi kayd� hari�, silinmemi� di�er kay�tlarda ayn� email var m�?
            var duplicateEmail = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Id != ogrenci.Id && o.Email == ogrenci.Email)
                .FirstOrDefaultAsync();

            if (duplicateEmail != null)
            {
                throw new InvalidOperationException($"Bu e-posta adresi ({ogrenci.Email}) ba�ka bir ��renci taraf�ndan kullan�l�yor.");
            }

            // TC No kontrol� - Kendi kayd� hari�, silinmemi� di�er kay�tlarda ayn� TC var m�?
            if (!string.IsNullOrWhiteSpace(ogrenci.TCNO))
            {
                var duplicateTCNO = await _context.Ogrenciler
                    .Where(o => !o.IsDeleted && o.Id != ogrenci.Id && o.TCNO == ogrenci.TCNO)
                    .FirstOrDefaultAsync();

                if (duplicateTCNO != null)
                {
                    throw new InvalidOperationException($"Bu TC Kimlik No ({ogrenci.TCNO}) ba�ka bir ��renci taraf�ndan kullan�l�yor.");
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
            // �lkTaksitSonOdemeTarihi g�ncellenmiyor - Controller'da korunuyor
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

            // Soft delete - IsDeleted'� true yap
            ogrenci.IsDeleted = true;
            ogrenci.Aktif = false;

            // ��renciye ait t�m �deme takvimlerini de soft delete yap
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
        /// ��renci i�in �deme plan�na g�re taksit kay�tlar� olu�turur
        /// </summary>
        public async Task CreateTaksitlerForOgrenciAsync(long ogrenciId)
        {
            var ogrenci = await _context.Ogrenciler
                .Include(o => o.OdemePlanlari)
                .FirstOrDefaultAsync(o => o.Id == ogrenciId && !o.IsDeleted);

            if (ogrenci == null || ogrenci.OdemePlanlari == null || ogrenci.OdemePlanlari.IsDeleted)
                return;

            var odemePlani = ogrenci.OdemePlanlari;

            // Taksit say�s� ve toplam tutar
            int taksitSayisi = odemePlani.TaksitSayisi;
            decimal toplamTutar = odemePlani.ToplamTutar;
            decimal taksitTutari = odemePlani.TaksitTutari;

            // Vade hesaplama: Vade varsa taksite b�l, yoksa varsay�lan 30 g�n
            int vadeSuresi = odemePlani.Vade.HasValue ? odemePlani.Vade.Value : (taksitSayisi * 30);
            int taksitBasinaGun = vadeSuresi / taksitSayisi;

            // �lk taksit son �deme tarihi: Kullan�c� girmediyse kay�t tarihi, girdiyse o tarih
            DateTime ilkTaksitSonOdemeTarihi = ogrenci.IlkTaksitSonOdemeTarihi ?? ogrenci.KayitTarihi;

            // �LK TAKS�TTE KALAN BOR� = TOPLAM TUTAR
            // Sonraki taksitlerde azalacak
            decimal kalanBorc = toplamTutar;

            // Taksitleri olu�tur
            for (int i = 1; i <= taksitSayisi; i++)
            {
                // Son taksitte kalan tutar� tam olarak hesapla (yuvarlama fark� i�in)
                decimal buTaksitTutari = (i == taksitSayisi) ? kalanBorc : taksitTutari;

                // Taksit son �deme tarihi
                // �lk taksit i�in kullan�c�n�n girdi�i/kay�t tarihi, sonrakiler i�in hesaplanm�� tarih
                DateTime sonOdemeTarihi;
                if (i == 1)
                {
                    sonOdemeTarihi = ilkTaksitSonOdemeTarihi;
                }
                else
                {
                    // Sonraki taksitler: ilk taksit tarihinden itibaren hesapla
                    sonOdemeTarihi = ilkTaksitSonOdemeTarihi.AddDays((i - 1) * taksitBasinaGun);
                }

                var taksit = new OgrenciOdemeTakvimi
                {
                    OgrenciId = ogrenciId,
                    TaksitNo = i,
                    TaksitTutari = taksitTutari,  // Bu taksit i�in �denecek tutar
                    SonOdemeTarihi = sonOdemeTarihi,
                    OdenenTutar = 0, // Hen�z �denmedi
                    BorcTutari = kalanBorc, // Bu taksit �NCES� kalan bor� (ilk taksitte ToplamTutar)
                    Odendi = false,
                    SmsGittiMi = false,
                    OdemeTarihi = null,
                    OlusturmaTarihi = DateTime.Now,
                    Aktif = true,
                    IsDeleted = false,
                    Version = 0
                };

                _context.OgrenciOdemeTakvimi.Add(taksit);

                // Bir sonraki taksit i�in kalan borcu g�ncelle
                // Bu taksit �dendikten sonraki kalan bor�
                kalanBorc -= taksitTutari;
            }

            await _context.SaveChangesAsync();
        }
    }
}
