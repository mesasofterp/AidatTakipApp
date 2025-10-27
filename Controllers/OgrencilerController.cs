using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    public class OgrencilerController : Controller
    {
        private readonly IOgrencilerService _ogrenciService;
        private readonly IOdemePlanlariService _odemePlanlariService;
        private readonly ICinsiyetlerService _cinsiyetlerService;

        public OgrencilerController(
            IOgrencilerService ogrencilerService,
            IOdemePlanlariService odemePlanlariService,
            ICinsiyetlerService cinsiyetlerService)
        {
            _ogrenciService = ogrencilerService;
            _odemePlanlariService = odemePlanlariService;
            _cinsiyetlerService = cinsiyetlerService;
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            var ogrenciler = await _ogrenciService.GetAllOgrenciAsync();
            return View(ogrenciler);
        }

        // GET: Student/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ogrenciler ogrenci)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _ogrenciService.AddOgrenciAsync(ogrenci);
                    TempData["SuccessMessage"] = "Öğrenci başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Öğrenci eklenirken bir hata oluştu: {ex.Message}");
                }
            }
            else
            {
                // ModelState hatalarını logla
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
            }

            await LoadDropdownsAsync();
            return View(ogrenci);
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(id);
            if (ogrenci == null)
            {
                return NotFound();
            }

            return View(ogrenci);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(id);
            if (ogrenci == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync();
            return View(ogrenci);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Ogrenciler ogrenci)
        {
            if (id != ogrenci.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedOgrenci = await _ogrenciService.UpdateOgrenciAsync(ogrenci);
                    if (updatedOgrenci == null)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "Öğrenci başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Öğrenci güncellenirken bir hata oluştu: {ex.Message}");
                }
            }
            else
            {
                // ModelState hatalarını logla
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
            }

            await LoadDropdownsAsync();
            return View(ogrenci);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(id);
            if (ogrenci == null)
            {
                return NotFound();
            }

            return View(ogrenci);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var result = await _ogrenciService.DeleteOgrenciAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Öğrenci başarıyla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Öğrenci bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Öğrenci silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var odemePlanlari = await _odemePlanlariService.GetAllOdemePlanlariAsync();
            ViewBag.OdemePlanlari = new SelectList(odemePlanlari, "Id", "KursProgrami");

            var cinsiyetler = await _cinsiyetlerService.GetAllCinsiyetlerAsync();
            ViewBag.Cinsiyetler = new SelectList(cinsiyetler, "Id", "Cinsiyet");
        }
    }
}
