using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;
using StudentApp.Attributes;

namespace StudentApp.Controllers
{
    [Authorize]
    public class OgrenciUyelikDondurmaController : Controller
    {
        private readonly IOgrenciUyelikDondurmaService _dondurmaService;
        private readonly IOgrencilerService _ogrenciService;
        private readonly ILogger<OgrenciUyelikDondurmaController> _logger;

        public OgrenciUyelikDondurmaController(
        IOgrenciUyelikDondurmaService dondurmaService,
       IOgrencilerService ogrenciService,
 ILogger<OgrenciUyelikDondurmaController> logger)
        {
  _dondurmaService = dondurmaService;
        _ogrenciService = ogrenciService;
  _logger = logger;
}

        // GET: OgrenciUyelikDondurma
        public async Task<IActionResult> Index(long? ogrenciId)
  {
    IEnumerable<OgrenciUyelikDondurma> dondurmaListesi;

      if (ogrenciId.HasValue)
            {
          dondurmaListesi = await _dondurmaService.GetByOgrenciIdAsync(ogrenciId.Value);
     var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
         ViewBag.OgrenciAdi = ogrenci != null ? $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}" : "";
          ViewBag.OgrenciId = ogrenciId.Value;
  }
            else
         {
          dondurmaListesi = await _dondurmaService.GetAllAsync();
            }

   return View(dondurmaListesi);
        }

        // GET: OgrenciUyelikDondurma/Create
    public async Task<IActionResult> Create(long? ogrenciId)
        {
    if (!ogrenciId.HasValue)
            {
   TempData["ErrorMessage"] = "Dondurma iþlemi için önce bir öðrenci seçmelisiniz.";
          return RedirectToAction("Index", "Ogrenciler");
     }

            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
   if (ogrenci == null)
   {
      TempData["ErrorMessage"] = "Öðrenci bulunamadý.";
                return RedirectToAction("Index", "Ogrenciler");
   }

            // Aktif dondurma var mý kontrol et
    var aktifDondurma = await _dondurmaService.GetAktifDondurmaAsync(ogrenciId.Value);
   if (aktifDondurma != null)
    {
                TempData["ErrorMessage"] = $"Öðrencinin zaten aktif bir dondurmasý var (Baþlangýç: {aktifDondurma.BaslangicTarihi:dd.MM.yyyy}).";
      return RedirectToAction(nameof(Index), new { ogrenciId });
    }

            ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
  ViewBag.OgrenciId = ogrenciId.Value;

       return View(new OgrenciUyelikDondurma
          {
       OgrenciId = ogrenciId.Value,
      BaslangicTarihi = DateTime.Today,
              BitisTarihi = DateTime.Today.AddDays(7)
     });
      }

        // POST: OgrenciUyelikDondurma/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OgrenciUyelikDondurma dondurma)
        {
            if (dondurma.OgrenciId <= 0)
   {
    TempData["ErrorMessage"] = "Geçersiz öðrenci bilgisi.";
     return RedirectToAction("Index", "Ogrenciler");
            }

   if (ModelState.IsValid)
       {
        try
     {
     await _dondurmaService.CreateAsync(dondurma);
         TempData["SuccessMessage"] = "Üyelik dondurma kaydý baþarýyla oluþturuldu! Ödeme tarihleri otomatik olarak ayarlandý.";
    return RedirectToAction(nameof(Index), new { ogrenciId = dondurma.OgrenciId });
                }
         catch (InvalidOperationException ex)
                {
    ModelState.AddModelError("", ex.Message);
                }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Dondurma kaydý oluþturulurken hata");
           ModelState.AddModelError("", "Dondurma kaydý oluþturulurken bir hata oluþtu.");
       }
    }

     // Hata durumunda öðrenci bilgilerini tekrar yükle
   var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(dondurma.OgrenciId);
      if (ogrenci != null)
     {
     ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
    ViewBag.OgrenciId = dondurma.OgrenciId;
            }

 return View(dondurma);
        }

        // GET: OgrenciUyelikDondurma/Details/5
public async Task<IActionResult> Details(long id)
        {
    var dondurma = await _dondurmaService.GetByIdAsync(id);
            if (dondurma == null)
    {
    return NotFound();
        }

            return View(dondurma);
        }

        // GET: OgrenciUyelikDondurma/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
var dondurma = await _dondurmaService.GetByIdAsync(id);
   if (dondurma == null)
    {
       return NotFound();
            }

          if (dondurma.Ogrenci != null)
        {
     ViewBag.OgrenciAdi = $"{dondurma.Ogrenci.OgrenciAdi} {dondurma.Ogrenci.OgrenciSoyadi}";
            }

    return View(dondurma);
 }

        // POST: OgrenciUyelikDondurma/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, OgrenciUyelikDondurma dondurma)
        {
            if (id != dondurma.Id)
       {
     return NotFound();
            }

        if (ModelState.IsValid)
            {
          try
          {
       var updated = await _dondurmaService.UpdateAsync(dondurma);
           if (updated == null)
         {
   return NotFound();
             }

      TempData["SuccessMessage"] = "Dondurma kaydý baþarýyla güncellendi!";
              return RedirectToAction(nameof(Index), new { ogrenciId = dondurma.OgrenciId });
    }
                catch (InvalidOperationException ex)
       {
         ModelState.AddModelError("", ex.Message);
        }
        catch (Exception ex)
 {
          _logger.LogError(ex, "Dondurma kaydý güncellenirken hata");
       ModelState.AddModelError("", "Dondurma kaydý güncellenirken bir hata oluþtu.");
      }
      }

   var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(dondurma.OgrenciId);
            if (ogrenci != null)
          {
        ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
      }

        return View(dondurma);
        }

      // GET: OgrenciUyelikDondurma/Delete/5
     public async Task<IActionResult> Delete(long id)
        {
  var dondurma = await _dondurmaService.GetByIdAsync(id);
            if (dondurma == null)
            {
      return NotFound();
      }

            return View(dondurma);
        }

        // POST: OgrenciUyelikDondurma/Delete/5
        [HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
          {
           var dondurma = await _dondurmaService.GetByIdAsync(id);
          var result = await _dondurmaService.DeleteAsync(id);
                
     if (result)
        {
      TempData["SuccessMessage"] = "Dondurma kaydý baþarýyla silindi!";
        if (dondurma != null)
        {
    return RedirectToAction(nameof(Index), new { ogrenciId = dondurma.OgrenciId });
           }
      }
           else
   {
      TempData["ErrorMessage"] = "Dondurma kaydý bulunamadý.";
      }
      }
      catch (Exception ex)
         {
      _logger.LogError(ex, "Dondurma kaydý silinirken hata");
         TempData["ErrorMessage"] = "Dondurma kaydý silinirken bir hata oluþtu.";
}

            return RedirectToAction(nameof(Index));
        }

        // POST: OgrenciUyelikDondurma/IptalEt/5
        [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> IptalEt(long id, string iptalNedeni)
        {
try
        {
                var kullaniciAdi = User.Identity?.Name ?? "Bilinmeyen";
        var result = await _dondurmaService.IptalEtAsync(id, kullaniciAdi, iptalNedeni);
          
          if (result)
  {
      TempData["SuccessMessage"] = "Dondurma kaydý baþarýyla iptal edildi!";
         }
        else
    {
       TempData["ErrorMessage"] = "Dondurma kaydý bulunamadý.";
         }
  }
          catch (Exception ex)
     {
                _logger.LogError(ex, "Dondurma iptal edilirken hata");
  TempData["ErrorMessage"] = "Dondurma iptal edilirken bir hata oluþtu.";
      }

 return RedirectToAction(nameof(Index));
        }

        // POST: OgrenciUyelikDondurma/TarihleriYenidenAyarla/5
        [HttpPost]
 [ValidateAntiForgeryToken]
        public async Task<IActionResult> TarihleriYenidenAyarla(long id)
    {
            try
   {
 var result = await _dondurmaService.OdemeTarihleriniAyarlaAsync(id);
         
     if (result)
     {
   TempData["SuccessMessage"] = "Ödeme tarihleri baþarýyla yeniden ayarlandý!";
           }
     else
     {
          TempData["ErrorMessage"] = "Dondurma kaydý bulunamadý.";
          }
       }
            catch (Exception ex)
            {
      _logger.LogError(ex, "Ödeme tarihleri ayarlanýrken hata");
              TempData["ErrorMessage"] = "Ödeme tarihleri ayarlanýrken bir hata oluþtu.";
            }

        var dondurma = await _dondurmaService.GetByIdAsync(id);
            if (dondurma != null)
          {
                return RedirectToAction(nameof(Index), new { ogrenciId = dondurma.OgrenciId });
    }

    return RedirectToAction(nameof(Index));
        }
  }
}
