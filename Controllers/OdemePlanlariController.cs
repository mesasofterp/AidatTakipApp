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

        // GET: Student
        public async Task<IActionResult> Index()
        {
            var odemePlanlari = await _odemePlaniService.GetAllOdemePlanlariAsync();
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



        // GET: Student/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var odemePlani = await _odemePlaniService.GetOdemePlaniByIdAsync(id);
            if (odemePlani == null)
            {
                return NotFound();
            }

            return View(odemePlani);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var odemePlani = await _odemePlaniService.GetOdemePlaniByIdAsync(id);
            if (odemePlani == null)
            {
                return NotFound();
            }

            return View(odemePlani);
        }

        // POST: Student/Edit/5
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

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var odemePlani = await _odemePlaniService.GetOdemePlaniByIdAsync(id);
            if (odemePlani == null)
            {
                return NotFound();
            }

            return View(odemePlani);
        }

        // POST: Student/Delete/5
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
    }
}
