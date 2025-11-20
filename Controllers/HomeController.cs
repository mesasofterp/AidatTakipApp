using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApp.Models;
using StudentApp.Services;
using StudentApp.Data;

namespace StudentApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IOgrencilerService _ogrenciService;
        private readonly IOgrenciOdemeTakvimiService _odemeService;
        private readonly AppDbContext _context;

        public HomeController(
            IOgrencilerService ogrenciService,
            IOgrenciOdemeTakvimiService odemeService,
            AppDbContext context)
        {
            _ogrenciService = ogrenciService;
            _odemeService = odemeService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetOgrenciList(string searchTerm = "")
        {
            var ogrenciler = await _ogrenciService.GetAllOgrenciAsync(false); // Sadece aktifler
            
            // searchTerm boş olsa bile filtreleme yapılabilir (tüm öğrencileri döndür)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.Trim().ToLower();
                ogrenciler = ogrenciler.Where(o =>
                    o.OgrenciAdi.ToLower().Contains(searchLower) ||
                    o.OgrenciSoyadi.ToLower().Contains(searchLower) ||
                    (o.OgrenciAdi + " " + o.OgrenciSoyadi).ToLower().Contains(searchLower) ||
                    (o.Email != null && o.Email.ToLower().Contains(searchLower)) ||
                    (o.TCNO != null && o.TCNO.Contains(searchTerm.Trim()))
                );
            }

            var result = ogrenciler
                .OrderBy(o => o.OgrenciSoyadi)
                .ThenBy(o => o.OgrenciAdi)
                .Take(50) // Maksimum 50 sonuç
                .Select(o => new
                {
                    id = o.Id,
                    ad = o.OgrenciAdi,
                    soyad = o.OgrenciSoyadi,
                    adSoyad = $"{o.OgrenciAdi} {o.OgrenciSoyadi}",
                    email = o.Email,
                    tcno = o.TCNO
                })
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOgrenciDetay(long id)
        {
            var ogrenci = await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Include(s => s.OgrenciDetay)
                .Where(s => !s.IsDeleted && s.Id == id)
                .FirstOrDefaultAsync();

            if (ogrenci == null)
            {
                return NotFound(new { error = "Öğrenci bulunamadı." });
            }

            var yas = DateTime.Now.Year - ogrenci.DogumTarihi.Year;
            if (DateTime.Now.DayOfYear < ogrenci.DogumTarihi.DayOfYear)
            {
                yas--;
            }

            // Öğrencinin başarılarını getir
            var basarilar = await _context.OgrenciBasarilari
                .Where(b => b.OgrenciId == id && !b.IsDeleted)
                .OrderByDescending(b => b.Tarih)
                .ThenByDescending(b => b.Id)
                .Select(b => new
                {
                    id = b.Id,
                    baslik = b.Baslik,
                    turu = b.Turu,
                    tarih = b.Tarih.HasValue ? b.Tarih.Value.ToString("dd.MM.yyyy") : null,
                    aciklama = b.Aciklama
                })
                .ToListAsync();

            // Öğrencinin envanter satışlarını getir
            var envanterSatislari = await _context.OgrenciEnvanterSatis
                .Include(e => e.Envanter)
                .Where(e => e.OgrenciId == id && !e.IsDeleted)
                .OrderByDescending(e => e.SatisTarihi)
                .Select(e => new
                {
                    id = e.Id,
                    envanterAdi = e.Envanter != null ? e.Envanter.EnvanterAdi : "Silinmiş Envanter",
                    satisTarihi = e.SatisTarihi.HasValue ? e.SatisTarihi.Value.ToString("dd.MM.yyyy") : null,
                    satisAdet = e.SatisAdet,
                    odenenTutar = e.OdenenTutar,
                    aciklama = e.Aciklama,
                    aktif = e.Aktif
                })
                .ToListAsync();

            var toplamEnvanterHarcama = envanterSatislari.Sum(e => e.odenenTutar);

            var result = new
            {
                id = ogrenci.Id,
                ad = ogrenci.OgrenciAdi,
                soyad = ogrenci.OgrenciSoyadi,
                tcno = ogrenci.TCNO,
                dogumTarihi = ogrenci.DogumTarihi.ToString("dd.MM.yyyy"),
                yas = yas,
                cinsiyet = ogrenci.Cinsiyet?.Cinsiyet,
                kayitTarihi = ogrenci.KayitTarihi.ToString("dd.MM.yyyy"),
                email = ogrenci.Email,
                telefon = ogrenci.Telefon,
                adres = ogrenci.Adres,
                odemePlani = ogrenci.OdemePlanlari != null ? new
                {
                    kursProgrami = ogrenci.OdemePlanlari.KursProgrami,
                    toplamTutar = ogrenci.OdemePlanlari.ToplamTutar,
                    taksitSayisi = ogrenci.OdemePlanlari.TaksitSayisi,
                    taksitTutari = ogrenci.OdemePlanlari.TaksitTutari,
                    vade = ogrenci.OdemePlanlari.Vade
                } : null,
                ilkTaksitSonOdemeTarihi = ogrenci.IlkTaksitSonOdemeTarihi?.ToString("dd.MM.yyyy"),
                durum = ogrenci.Aktif ? "Aktif" : "Pasif",
                veliAdSoyad = ogrenci.OgrenciDetay?.VeliAdSoyad,
                veliTelefon = ogrenci.OgrenciDetay?.VeliTelefonNumarasi,
                basarilar = basarilar,
                envanterSatislari = envanterSatislari,
                toplamEnvanterHarcama = toplamEnvanterHarcama
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOgrenciOdemeTakvimi(long id)
        {
            var odemeler = await _odemeService.GetOdemelerByOgrenciIdAsync(id);
            
            var result = odemeler
                .OrderBy(o => o.TaksitNo ?? 0)
                .Select(o => new
                {
                    id = o.Id,
                    taksitNo = o.TaksitNo,
                    taksitTutari = o.TaksitTutari,
                    sonOdemeTarihi = o.SonOdemeTarihi?.ToString("dd.MM.yyyy"),
                    odemeTarihi = o.OdemeTarihi?.ToString("dd.MM.yyyy"),
                    odenenTutar = o.OdenenTutar,
                    borcTutari = o.BorcTutari,
                    odendi = o.Odendi,
                    aciklama = o.Aciklama,
                    gecikmeGunu = o.SonOdemeTarihi.HasValue && !o.Odendi && o.SonOdemeTarihi.Value.Date < DateTime.Today
                        ? (DateTime.Today - o.SonOdemeTarihi.Value.Date).Days
                        : (int?)null
                })
                .ToList();

            return Json(result);
        }
    }
}

