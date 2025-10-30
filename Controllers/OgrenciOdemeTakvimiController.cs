using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    public class OgrenciOdemeTakvimiController : Controller
    {
        private readonly IOgrenciOdemeTakvimiService _odemeService;
   private readonly IOgrencilerService _ogrenciService;

        public OgrenciOdemeTakvimiController(
            IOgrenciOdemeTakvimiService odemeService,
    IOgrencilerService ogrencilerService)
{
            _odemeService = odemeService;
       _ogrenciService = ogrencilerService;
        }

        // GET: OgrenciOdemeTakvimi
        public async Task<IActionResult> Index(long? ogrenciId)
        {
            IEnumerable<OgrenciOdemeTakvimi> odemeler;
            
         if (ogrenciId.HasValue)
    {
        odemeler = await _odemeService.GetOdemelerByOgrenciIdAsync(ogrenciId.Value);
      var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
           ViewBag.OgrenciAdi = ogrenci != null ? $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}" : "";
        ViewBag.OgrenciId = ogrenciId.Value;
            }
     else
            {
        odemeler = await _odemeService.GetAllOdemelerAsync();
            // T�m ��renciler i�in toplam kalan bor�
       ViewBag.ToplamKalanBorc = await _odemeService.GetToplamKalanBorcAsync();
            }

return View(odemeler);
   }

        // GET: OgrenciOdemeTakvimi/Create
      public async Task<IActionResult> Create(long? ogrenciId)
 {
            // ��renci ID'si zorunlu - yoksa ��renci listesine y�nlendir
            if (!ogrenciId.HasValue)
    {
         TempData["ErrorMessage"] = "�deme giri�i i�in �nce bir ��renci se�melisiniz.";
           return RedirectToAction("Index", "Ogrenciler");
            }

            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
            if (ogrenci == null)
        {
     TempData["ErrorMessage"] = "��renci bulunamad�.";
          return RedirectToAction("Index", "Ogrenciler");
        }
    
            ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
       ViewBag.OgrenciId = ogrenciId.Value;
       
      // Toplam bor� hesapla
          var toplamOdenen = await _odemeService.GetToplamOdenenTutarAsync(ogrenciId.Value);
         var kalanBorc = await _odemeService.GetKalanBorcAsync(ogrenciId.Value);
        
            // �lk �deme ise, �deme plan�ndan toplam tutar� al
            if (toplamOdenen == 0 && ogrenci.OdemePlanlari != null)
            {
    // Toplam tutar = Taksit Say�s� � Tutar
                kalanBorc = ogrenci.OdemePlanlari.Tutar;
            }

ViewBag.KalanBorc = kalanBorc;
          ViewBag.ToplamOdenen = toplamOdenen;
     ViewBag.OdemePlani = ogrenci.OdemePlanlari;

  return View(new OgrenciOdemeTakvimi 
            { 
          OgrenciId = ogrenciId.Value,
      OdemeTarihi = DateTime.Now
            });
 }

    // POST: OgrenciOdemeTakvimi/Create
      [HttpPost]
  [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OgrenciOdemeTakvimi odeme)
        {
      // ��renci ID kontrol�
       if (odeme.OgrenciId <= 0)
          {
   TempData["ErrorMessage"] = "Ge�ersiz ��renci bilgisi.";
     return RedirectToAction("Index", "Ogrenciler");
 }

            if (ModelState.IsValid)
            {
        try
    {
           await _odemeService.AddOdemeAsync(odeme);
  TempData["SuccessMessage"] = "�deme ba�ar�yla kaydedildi!";
       return RedirectToAction(nameof(Index), new { ogrenciId = odeme.OgrenciId });
}
        catch (Exception ex)
            {
      ModelState.AddModelError("", $"�deme kaydedilirken bir hata olu�tu: {ex.Message}");
  }
    }

            // Hata durumunda ��renci bilgilerini tekrar y�kle
    var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(odeme.OgrenciId);
 if (ogrenci != null)
   {
                ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
                ViewBag.OgrenciId = odeme.OgrenciId;
              
   var toplamOdenen = await _odemeService.GetToplamOdenenTutarAsync(odeme.OgrenciId);
     var kalanBorc = await _odemeService.GetKalanBorcAsync(odeme.OgrenciId);
      
       if (toplamOdenen == 0 && ogrenci.OdemePlanlari != null)
      {
               kalanBorc = ogrenci.OdemePlanlari.Tutar * ogrenci.OdemePlanlari.Taksit;
   }

      ViewBag.KalanBorc = kalanBorc;
                ViewBag.ToplamOdenen = toplamOdenen;
                ViewBag.OdemePlani = ogrenci.OdemePlanlari;
            }

            return View(odeme);
        }

        // GET: OgrenciOdemeTakvimi/Details/5
  public async Task<IActionResult> Details(long id)
  {
      var odeme = await _odemeService.GetOdemeByIdAsync(id);
     if (odeme == null)
  {
      return NotFound();
       }

   return View(odeme);
     }

        // GET: OgrenciOdemeTakvimi/Edit/5
     public async Task<IActionResult> Edit(long id)
   {
      var odeme = await _odemeService.GetOdemeByIdAsync(id);
   if (odeme == null)
   {
       return NotFound();
   }

       if (odeme.Ogrenci != null)
{
     ViewBag.OgrenciAdi = $"{odeme.Ogrenci.OgrenciAdi} {odeme.Ogrenci.OgrenciSoyadi}";
     }

  return View(odeme);
  }

        // POST: OgrenciOdemeTakvimi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
   public async Task<IActionResult> Edit(long id, OgrenciOdemeTakvimi odeme)
        {
   if (id != odeme.Id)
  {
          return NotFound();
   }

        if (ModelState.IsValid)
    {
    try
        {
      var updatedOdeme = await _odemeService.UpdateOdemeAsync(odeme);
if (updatedOdeme == null)
     {
     return NotFound();
      }

       TempData["SuccessMessage"] = "�deme ba�ar�yla g�ncellendi! Kalan bor� tutarlar� yeniden hesapland�.";
           return RedirectToAction(nameof(Index), new { ogrenciId = odeme.OgrenciId });
          }
    catch (Exception ex)
      {
 ModelState.AddModelError("", $"�deme g�ncellenirken bir hata olu�tu: {ex.Message}");
        }
  }

   var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(odeme.OgrenciId);
  if (ogrenci != null)
       {
          ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
   }

            return View(odeme);
        }

        // GET: OgrenciOdemeTakvimi/Delete/5
        public async Task<IActionResult> Delete(long id)
      {
       var odeme = await _odemeService.GetOdemeByIdAsync(id);
 if (odeme == null)
  {
        return NotFound();
      }

 return View(odeme);
     }

        // POST: OgrenciOdemeTakvimi/Delete/5
   [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
      try
{
    var odeme = await _odemeService.GetOdemeByIdAsync(id);
       var result = await _odemeService.DeleteOdemeAsync(id);
    if (result)
      {
  TempData["SuccessMessage"] = "�deme ba�ar�yla silindi! Kalan bor� tutarlar� yeniden hesapland�.";
    if (odeme != null)
       {
      return RedirectToAction(nameof(Index), new { ogrenciId = odeme.OgrenciId });
   }
   }
  else
   {
  TempData["ErrorMessage"] = "�deme bulunamad�.";
  }
       }
   catch (Exception ex)
  {
 TempData["ErrorMessage"] = "�deme silinirken bir hata olu�tu.";
       }

       return RedirectToAction(nameof(Index));
 }

        // Utility: T�m ��renciler i�in kalan borcu yeniden hesapla
        [HttpPost]
        [ValidateAntiForgeryToken]
 public async Task<IActionResult> RecalculateAllBorcs()
        {
  try
            {
     var ogrenciler = await _ogrenciService.GetAllOgrenciAsync(true); // true = pasif ��renciler dahil
       int hesaplananOgrenciSayisi = 0;

      foreach (var ogrenci in ogrenciler)
       {
         await _odemeService.RecalculateKalanBorcForOgrenciAsync(ogrenci.Id);
          hesaplananOgrenciSayisi++;
          }

         TempData["SuccessMessage"] = $"T�m ��rencilerin ({hesaplananOgrenciSayisi}) kalan bor� tutarlar� ba�ar�yla yeniden hesapland�!";
            }
     catch (Exception ex)
  {
TempData["ErrorMessage"] = $"Kalan bor� hesaplan�rken bir hata olu�tu: {ex.Message}";
            }

  return RedirectToAction(nameof(Index));
   }
    }
}
