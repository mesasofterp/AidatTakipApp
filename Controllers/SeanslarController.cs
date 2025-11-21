using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    [Authorize]
    public class SeanslarController : Controller
    {
  private readonly ISeanslarService _seanslarService;
  private readonly IGunlerService _gunlerService;
  private readonly AppDbContext _context;

    public SeanslarController(
      ISeanslarService seanslarService,
     IGunlerService gunlerService,
     AppDbContext context)
{
 _seanslarService = seanslarService;
     _gunlerService = gunlerService;
          _context = context;
 }

        // GET: Seanslar
   public async Task<IActionResult> Index()
        {
        var seanslar = await _seanslarService.GetAllSeanslarAsync();
     return View(seanslar);
        }

   // GET: Seanslar/Create
        public async Task<IActionResult> Create()
        {
     await LoadDropdownsAsync();
       return View();
  }

      // POST: Seanslar/Create
   [HttpPost]
        [ValidateAntiForgeryToken]
   public async Task<IActionResult> Create(Seanslar seans)
 {
   if (ModelState.IsValid)
         {
try
  {
    await _seanslarService.AddSeansAsync(seans);
    TempData["SuccessMessage"] = "Seans baþarýyla eklendi!";
         return RedirectToAction(nameof(Index));
  }
      catch (Exception ex)
      {
      ModelState.AddModelError("", $"Seans eklenirken bir hata oluþtu: {ex.Message}");
    }
      }

       await LoadDropdownsAsync();
      return View(seans);
     }

    // GET: Seanslar/Details/5
        public async Task<IActionResult> Details(long id)
        {
  var seans = await _seanslarService.GetSeansByIdAsync(id);

   if (seans == null)
  {
        return NotFound();
   }

      // Seansa kayýtlý öðrenci sayýsýný al
     var ogrenciSayisi = await _context.Ogrenciler
         .Where(o => o.SeansId == id && !o.IsDeleted && o.Aktif)
      .CountAsync();

       ViewBag.KayitliOgrenciSayisi = ogrenciSayisi;

   return View(seans);
  }

// GET: Seanslar/Edit/5
        public async Task<IActionResult> Edit(long id)
    {
      var seans = await _seanslarService.GetSeansByIdAsync(id);

 if (seans == null)
   {
    return NotFound();
      }

  await LoadDropdownsAsync();
            return View(seans);
  }

        // POST: Seanslar/Edit/5
     [HttpPost]
 [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Seanslar seans)
   {
     if (id != seans.Id)
       {
        return NotFound();
  }

    if (ModelState.IsValid)
    {
   try
{
    var updatedSeans = await _seanslarService.UpdateSeansAsync(seans);
     if (updatedSeans == null)
      {
            return NotFound();
   }

           TempData["SuccessMessage"] = "Seans baþarýyla güncellendi!";
   return RedirectToAction(nameof(Index));
     }
       catch (Exception ex)
      {
   ModelState.AddModelError("", $"Seans güncellenirken bir hata oluþtu: {ex.Message}");
   }
   }

   await LoadDropdownsAsync();
      return View(seans);
        }

     // GET: Seanslar/Delete/5
   public async Task<IActionResult> Delete(long id)
        {
   var seans = await _seanslarService.GetSeansByIdAsync(id);

      if (seans == null)
        {
        return NotFound();
       }

      // Seansa kayýtlý öðrenci sayýsýný kontrol et
        var ogrenciSayisi = await _context.Ogrenciler
    .Where(o => o.SeansId == id && !o.IsDeleted)
       .CountAsync();

 ViewBag.KayitliOgrenciSayisi = ogrenciSayisi;

    return View(seans);
        }

        // POST: Seanslar/Delete/5
   [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
    try
       {
      var result = await _seanslarService.DeleteSeansAsync(id);

     if (result)
  {
     TempData["SuccessMessage"] = "Seans baþarýyla silindi.";
   }
   else
   {
TempData["ErrorMessage"] = "Seans bulunamadý.";
         }
     }
     catch (InvalidOperationException ex)
        {
      TempData["ErrorMessage"] = ex.Message;
   }
  catch (Exception ex)
            {
TempData["ErrorMessage"] = "Seans silinirken bir hata oluþtu.";
    }

return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
       var gunler = await _gunlerService.GetAllGunlerAsync();
  ViewBag.Gunler = new SelectList(gunler, "Id", "Gun");
        }
    }
}
