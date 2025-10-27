using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    public class OdemePlanlariController : Controller
    {
        private readonly IOdemePlanlariService _odemePlaniService;

        public OdemePlanlariController(IOdemePlanlariService odemePlanlariService)
        {
            _odemePlaniService = odemePlanlariService;
        }

        // GET: OdemePlani
        public async Task<IActionResult> Index(bool showPasif = false)
        {
            var odemePlanlari = await _odemePlaniService.GetAllOdemePlanlariAsync(showPasif);
            ViewBag.ShowPasif = showPasif;
            return View(odemePlanlari);
        }

        // GET: OdemePlani/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OdemePlani/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OdemePlanlari odemePlani)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _odemePlaniService.AddOdemePlaniAsync(odemePlani);
                    TempData["SuccessMessage"] = "�deme plan� ba�ar�yla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "�deme plan� eklenirken bir hata olu�tu. L�tfen tekrar deneyin.");
                }
            }

            return View(odemePlani);
        }



        // GET: OdemePlani/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var odemePlani = await _odemePlaniService.GetOdemePlaniByIdAsync(id);
            if (odemePlani == null)
            {
                return NotFound();
            }

            return View(odemePlani);
        }

        // GET: OdemePlani/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var odemePlani = await _odemePlaniService.GetOdemePlaniByIdAsync(id);
            if (odemePlani == null)
            {
                return NotFound();
            }

            return View(odemePlani);
        }

        // POST: OdemePlani/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, OdemePlanlari odemePlani)
        {
            if (id != odemePlani.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedOdemePlani = await _odemePlaniService.UpdateOdemePlaniAsync(odemePlani);
                    if (updatedOdemePlani == null)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "�deme plan� ba�ar�yla g�ncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "�deme plan� g�ncellenirken bir hata olu�tu. L�tfen tekrar deneyin.");
                }
            }

            return View(odemePlani);
        }

        // GET: OdemePlani/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var odemePlani = await _odemePlaniService.GetOdemePlaniByIdAsync(id);
            if (odemePlani == null)
            {
                return NotFound();
            }

            return View(odemePlani);
        }

        // POST: OdemePlani/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var result = await _odemePlaniService.DeleteOdemePlaniAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "�deme plan� ba�ar�yla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "�deme plan� bulunamad�.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "�deme plan� silinirken bir hata olu�tu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: OdemePlani/ToggleAktif/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAktif(long id, bool aktif)
        {
            try
            {
                var result = await _odemePlaniService.ToggleAktifAsync(id, aktif);
                if (result)
                {
                    TempData["SuccessMessage"] = aktif
                        ? "�deme plan� ba�ar�yla aktif hale getirildi!"
                        : "�deme plan� ba�ar�yla pasif hale getirildi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "�deme plan� bulunamad�.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Durum g�ncellenirken bir hata olu�tu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
