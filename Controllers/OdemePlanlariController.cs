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
                    TempData["SuccessMessage"] = "Ödeme planý baþarýyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ödeme planý eklenirken bir hata oluþtu. Lütfen tekrar deneyin.");
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

                    TempData["SuccessMessage"] = "Ödeme planý baþarýyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ödeme planý güncellenirken bir hata oluþtu. Lütfen tekrar deneyin.");
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
                    TempData["SuccessMessage"] = "Ödeme planý baþarýyla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ödeme planý bulunamadý.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ödeme planý silinirken bir hata oluþtu.";
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
                        ? "Ödeme planý baþarýyla aktif hale getirildi!"
                        : "Ödeme planý baþarýyla pasif hale getirildi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ödeme planý bulunamadý.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Durum güncellenirken bir hata oluþtu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
