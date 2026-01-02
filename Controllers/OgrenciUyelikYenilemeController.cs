using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;
using StudentApp.Services;
using StudentApp.Attributes;

namespace StudentApp.Controllers
{
  [Authorize]
    public class OgrenciUyelikYenilemeController : Controller
    {
        private readonly IOgrenciUyelikYenilemeService _yenilemeService;
        private readonly IOdemePlanlariService _odemePlanlariService;
        private readonly IOgrencilerService _ogrenciService;
        private readonly AppDbContext _context;
        private readonly ILogger<OgrenciUyelikYenilemeController> _logger;

        public OgrenciUyelikYenilemeController(
          IOgrenciUyelikYenilemeService yenilemeService,
IOdemePlanlariService odemePlanlariService,
    IOgrencilerService ogrenciService,
            AppDbContext context,
            ILogger<OgrenciUyelikYenilemeController> logger)
        {
            _yenilemeService = yenilemeService;
         _odemePlanlariService = odemePlanlariService;
     _ogrenciService = ogrenciService;
      _context = context;
            _logger = logger;
        }

        // GET: OgrenciUyelikYenileme
        public async Task<IActionResult> Index(long? ogrenciId)
        {
            IEnumerable<OgrenciUyelikYenileme> yenilemeler;

   if (ogrenciId.HasValue)
       {
                yenilemeler = await _yenilemeService.GetByOgrenciIdAsync(ogrenciId.Value);
     var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
    ViewBag.OgrenciAdi = ogrenci != null ? $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}" : "";
    ViewBag.OgrenciId = ogrenciId.Value;
            }
            else
            {
          yenilemeler = await _yenilemeService.GetAllAsync();
       }

            return View(yenilemeler);
      }

        // GET: OgrenciUyelikYenileme/YenilenebilirOgrenciler
        public async Task<IActionResult> YenilenebilirOgrenciler()
        {
     try
            {
        var yenilenebilirOgrenciler = await _yenilemeService.GetYenilenebilirOgrencilerAsync();
       
                // Ödeme planlarýný dropdown için yükle
           var odemePlanlari = await _odemePlanlariService.GetAllOdemePlanlariAsync();
      ViewBag.OdemePlanlari = new SelectList(odemePlanlari.Where(p => p.Aktif), "Id", "KursProgrami");

      return View(yenilenebilirOgrenciler);
 }
      catch (Exception ex)
         {
                _logger.LogError(ex, "Yenilenebilir öðrenciler listelenirken hata oluþtu");
            TempData["ErrorMessage"] = "Öðrenciler listelenirken bir hata oluþtu.";
          return RedirectToAction(nameof(Index));
     }
        }

        // GET: OgrenciUyelikYenileme/Create
        public async Task<IActionResult> Create(long? ogrenciId)
    {
 if (!ogrenciId.HasValue)
       {
 TempData["ErrorMessage"] = "Öðrenci seçimi gereklidir.";
  return RedirectToAction(nameof(YenilenebilirOgrenciler));
  }

  try
            {
        // Öðrencinin mevcut durumunu kontrol et
      var durum = await _yenilemeService.GetUyelikDurumuAsync(ogrenciId.Value);
                
     if (!durum.YenilemeGerekli)
    {
        TempData["WarningMessage"] = $"Bu öðrenci henüz yenileme için uygun deðil. {durum.YenilemeNedeni}";
             return RedirectToAction("Details", "Ogrenciler", new { id = ogrenciId.Value });
     }

        ViewBag.UyelikDurum = durum;

        // Ödeme planlarýný yükle
 var odemePlanlari = await _odemePlanlariService.GetAllOdemePlanlariAsync();
     ViewBag.OdemePlanlari = new SelectList(odemePlanlari.Where(p => p.Aktif), "Id", "KursProgrami");

    var model = new OgrenciUyelikYenileme
{
      OgrenciId = ogrenciId.Value,
          YenilemeTarihi = DateTime.Now,
      YenilemeBaslangicTarihi = DateTime.Today
   };

      return View(model);
     }
 catch (Exception ex)
         {
     _logger.LogError(ex, "Yenileme formu yüklenirken hata oluþtu. Öðrenci ID: {OgrenciId}", ogrenciId);
    TempData["ErrorMessage"] = "Yenileme formu yüklenirken bir hata oluþtu.";
        return RedirectToAction("Details", "Ogrenciler", new { id = ogrenciId.Value });
            }
        }

      // POST: OgrenciUyelikYenileme/Create
  [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long ogrenciId, long yeniOdemePlaniId, decimal? indirimTutari, string? indirimAciklama)
        {
   try
     {
                var kullaniciAdi = User.Identity?.Name ?? "Sistem";

    var yenileme = await _yenilemeService.YenileAsync(
       ogrenciId,
    yeniOdemePlaniId,
    indirimTutari,
indirimAciklama,
          kullaniciAdi
      );

 TempData["SuccessMessage"] = "Üyelik baþarýyla yenilendi! Yeni dönem taksitleri oluþturuldu.";
                return RedirectToAction("Details", "Ogrenciler", new { id = ogrenciId });
     }
            catch (Exception ex)
            {
        _logger.LogError(ex, "Üyelik yenilenirken hata oluþtu. Öðrenci ID: {OgrenciId}", ogrenciId);
 TempData["ErrorMessage"] = $"Üyelik yenilenirken bir hata oluþtu: {ex.Message}";
        return RedirectToAction(nameof(Create), new { ogrenciId });
          }
      }

        // POST: OgrenciUyelikYenileme/TopluYenile
        [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> TopluYenile([FromForm] long[] selectedIds, long yeniOdemePlaniId)
        {
          if (selectedIds == null || selectedIds.Length == 0)
            {
    TempData["ErrorMessage"] = "Lütfen en az bir öðrenci seçin.";
        return RedirectToAction(nameof(YenilenebilirOgrenciler));
         }

      if (yeniOdemePlaniId <= 0)
       {
    TempData["ErrorMessage"] = "Lütfen geçerli bir ödeme planý seçin.";
        return RedirectToAction(nameof(YenilenebilirOgrenciler));
      }

     try
        {
      var kullaniciAdi = User.Identity?.Name ?? "Sistem";
     var sonuc = await _yenilemeService.TopluYenileAsync(
     selectedIds.ToList(),
           yeniOdemePlaniId,
       kullaniciAdi
    );

    if (sonuc.BasariliYenilemeSayisi > 0)
   {
               TempData["SuccessMessage"] = $"? {sonuc.BasariliYenilemeSayisi} öðrencinin üyeliði baþarýyla yenilendi!";
     }

         if (sonuc.HataliYenilemeSayisi > 0)
            {
               var hataMesaji = $"?? {sonuc.HataliYenilemeSayisi} öðrenci yenilenemedi:<br/>";
            hataMesaji += string.Join("<br/>", sonuc.Hatalar.Take(5));
            if (sonuc.Hatalar.Count > 5)
 {
     hataMesaji += $"<br/>... ve {sonuc.Hatalar.Count - 5} hata daha.";
    }
         TempData["WarningMessage"] = hataMesaji;
 }

       return RedirectToAction(nameof(Index));
   }
     catch (Exception ex)
            {
        _logger.LogError(ex, "Toplu yenileme sýrasýnda hata oluþtu");
   TempData["ErrorMessage"] = "Toplu yenileme sýrasýnda bir hata oluþtu.";
      return RedirectToAction(nameof(YenilenebilirOgrenciler));
      }
}

        // GET: OgrenciUyelikYenileme/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
   {
       return NotFound();
        }

          var yenileme = await _yenilemeService.GetByIdAsync(id.Value);

            if (yenileme == null)
      {
             return NotFound();
     }

        return View(yenileme);
        }

     // GET: OgrenciUyelikYenileme/UyelikDurumu/5
        public async Task<IActionResult> UyelikDurumu(long ogrenciId)
        {
 try
{
           var durum = await _yenilemeService.GetUyelikDurumuAsync(ogrenciId);
     return View(durum);
     }
  catch (Exception ex)
            {
      _logger.LogError(ex, "Üyelik durumu kontrol edilirken hata oluþtu. Öðrenci ID: {OgrenciId}", ogrenciId);
              TempData["ErrorMessage"] = "Üyelik durumu kontrol edilirken bir hata oluþtu.";
          return RedirectToAction("Details", "Ogrenciler", new { id = ogrenciId });
        }
        }

        // API: Ödeme planý bilgisi al (AJAX için)
        [HttpGet]
 public async Task<IActionResult> GetOdemePlaniInfo(long odemePlaniId)
        {
      try
          {
           var plan = await _context.OdemePlanlari
     .Where(p => p.Id == odemePlaniId && !p.IsDeleted)
        .Select(p => new
                    {
      p.Id,
    p.KursProgrami,
  p.ToplamTutar,
  p.TaksitSayisi,
         p.TaksitTutari,
       p.Vade
      })
   .FirstOrDefaultAsync();

      if (plan == null)
         {
       return NotFound();
  }

        return Json(plan);
    }
        catch (Exception ex)
            {
 _logger.LogError(ex, "Ödeme planý bilgisi alýnýrken hata oluþtu");
  return BadRequest();
   }
     }
    }
}
