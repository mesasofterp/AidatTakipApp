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
       var seanslar = await _context.Seanslar
      .Include(s => s.SeansGunler)
      .ThenInclude(sg => sg.Gun)
   .Where(s => !s.IsDeleted)
       .OrderBy(s => s.SeansAdi)
  .ToListAsync();

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
   public async Task<IActionResult> Create(Seanslar seans, List<long> SelectedGunIds)
 {
   // ModelState'ten GunId ve Gun hatalarýný temizle (NotMapped olduðu için)
        ModelState.Remove("GunId");
  ModelState.Remove("Gun");
        
        if (ModelState.IsValid)
   {
try
  {
    // En az bir gün seçilmeli
   if (SelectedGunIds == null || !SelectedGunIds.Any())
    {
      ModelState.AddModelError("", "En az bir gün seçmelisiniz.");
   await LoadDropdownsAsync();
      return View(seans);
      }

    // Seansý kaydet
await _seanslarService.AddSeansAsync(seans);

  // SeansGunler kayýtlarýný oluþtur
      foreach (var gunId in SelectedGunIds)
 {
      var seansGun = new SeansGunler
    {
    SeansId = seans.Id,
    GunId = gunId,
  Aktif = true,
    IsDeleted = false,
    Version = 0
        };
      _context.SeansGunler.Add(seansGun);
        }

  await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Seans baþarýyla eklendi!";
         return RedirectToAction(nameof(Index));
  }
      catch (Exception ex)
      {
      var innerMessage = ex.InnerException?.Message ?? ex.Message;
                var stackTrace = ex.InnerException?.StackTrace ?? ex.StackTrace;
ModelState.AddModelError("", $"Seans eklenirken bir hata oluþtu: {innerMessage}");
    
    // Log the full error for debugging
        Console.WriteLine($"Error: {ex.Message}");
              Console.WriteLine($"Inner Error: {innerMessage}");
           Console.WriteLine($"Stack Trace: {stackTrace}");
    }
    }

   await LoadDropdownsAsync();
      return View(seans);
     }

    // GET: Seanslar/Details/5
        public async Task<IActionResult> Details(long id)
      {
     var seans = await _context.Seanslar
       .Include(s => s.SeansGunler)
       .ThenInclude(sg => sg.Gun)
    .Where(s => !s.IsDeleted && s.Id == id)
        .FirstOrDefaultAsync();

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

  // Seçili günleri yükle
  var selectedGunIds = await _context.SeansGunler
 .Where(sg => sg.SeansId == id && !sg.IsDeleted)
   .Select(sg => sg.GunId)
    .ToListAsync();

ViewBag.SelectedGunIds = selectedGunIds;

    await LoadDropdownsAsync();
     return View(seans);
        }

        // POST: Seanslar/Edit/5
     [HttpPost]
 [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Seanslar seans, List<long> SelectedGunIds)
   {
     if (id != seans.Id)
       {
        return NotFound();
  }

    if (ModelState.IsValid)
    {
   try
{
    // En az bir gün seçilmeli
      if (SelectedGunIds == null || !SelectedGunIds.Any())
{
ModelState.AddModelError("", "En az bir gün seçmelisiniz.");
      ViewBag.SelectedGunIds = new List<long>();
       await LoadDropdownsAsync();
   return View(seans);
     }

  var updatedSeans = await _seanslarService.UpdateSeansAsync(seans);
         if (updatedSeans == null)
    {
       return NotFound();
   }

     // Mevcut SeansGunler kayýtlarýný sil
     var existingSeansGunler = await _context.SeansGunler
  .Where(sg => sg.SeansId == id)
   .ToListAsync();
  _context.SeansGunler.RemoveRange(existingSeansGunler);

// Yeni SeansGunler kayýtlarýný oluþtur
foreach (var gunId in SelectedGunIds)
   {
   var seansGun = new SeansGunler
{
 SeansId = seans.Id,
      GunId = gunId,
 Aktif = true,
 IsDeleted = false,
 Version = 0
       };
   await _context.SeansGunler.AddAsync(seansGun);
 }

  await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Seans baþarýyla güncellendi!";
        return RedirectToAction(nameof(Index));
  }
catch (Exception ex)
      {
   ModelState.AddModelError("", $"Seans güncellenirken bir hata oluþtu: {ex.Message}");
  }
        }

        // Hata durumunda seçili günleri tekrar yükle
     var selectedGunIds = await _context.SeansGunler
   .Where(sg => sg.SeansId == id && !sg.IsDeleted)
 .Select(sg => sg.GunId)
       .ToListAsync();
 ViewBag.SelectedGunIds = selectedGunIds;

        await LoadDropdownsAsync();
  return View(seans);
    }

        // GET: Seanslar/Delete/5
     public async Task<IActionResult> Delete(long id)
  {
       var seans = await _context.Seanslar
     .Include(s => s.SeansGunler)
        .ThenInclude(sg => sg.Gun)
      .Where(s => !s.IsDeleted && s.Id == id)
    .FirstOrDefaultAsync();

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
