using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;
using StudentApp.Attributes;

namespace StudentApp.Controllers
{
    [Authorize]
    public class EnvanterlerController : Controller
    {
        private readonly IEnvanterlerService _envanterlerService;
        private readonly ILogger<EnvanterlerController> _logger;

        public EnvanterlerController(
        IEnvanterlerService envanterlerService,
       ILogger<EnvanterlerController> logger)
        {
            _envanterlerService = envanterlerService;
            _logger = logger;
        }

        // GET: Envanterler
        [PageAuthorize("Envanterler.Index")]
        public async Task<IActionResult> Index(string searchTerm, string stokDurumu)
        {
            try
            {
                var envanterler = await _envanterlerService.GetAllAsync();

                // Arama filtresi
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower().Trim();
                    envanterler = envanterler.Where(e =>
                          e.EnvanterAdi.ToLower().Contains(searchLower) ||
                 (e.Aciklama != null && e.Aciklama.ToLower().Contains(searchLower)) ||
               e.AlisFiyat.ToString().Contains(searchLower) ||
                e.SatisFiyat.ToString().Contains(searchLower)
                 ).ToList();
                    ViewBag.SearchTerm = searchTerm;
                }

                // Stok durumu filtresi
                 if (!string.IsNullOrWhiteSpace(stokDurumu))
       {
                envanterler = stokDurumu switch
             {
             "stokta" => envanterler.Where(e => e.Adet > 0).ToList(),
                      "tukendi" => envanterler.Where(e => e.Adet == 0).ToList(),
                  "azaldi" => envanterler.Where(e => e.Adet > 0 && e.Adet <= 5).ToList(),
             _ => envanterler
               };
                ViewBag.StokDurumu = stokDurumu;
                        }

               var toplamDeger = envanterler.Sum(e => e.Adet * e.AlisFiyat);

                 ViewBag.ToplamDeger = toplamDeger;
             ViewBag.ToplamKalem = envanterler.Count();
           ViewBag.ToplamAdet = envanterler.Sum(e => e.Adet);

            return View(envanterler);
       }
         catch (Exception ex)
              {
         _logger.LogError(ex, "Envanterler listesi y�klenirken hata olu�tu");
                TempData["ErrorMessage"] = "Envanterler y�klenirken bir hata olu�tu.";
          return View(new List<Envanterler>());
                if (!string.IsNullOrWhiteSpace(stokDurumu))
                {
                    envanterler = stokDurumu switch
                    {
                        "stokta" => envanterler.Where(e => e.Adet > 0).ToList(),
                        "tukendi" => envanterler.Where(e => e.Adet == 0).ToList(),
                        "azaldi" => envanterler.Where(e => e.Adet > 0 && e.Adet <= 5).ToList(),
                        _ => envanterler
                    };
                    ViewBag.StokDurumu = stokDurumu;
                }

                var toplamSatisDeger = envanterler.Sum(e => e.Adet * e.SatisFiyat);

                ViewBag.ToplamSatisDeger = toplamSatisDeger;
                ViewBag.ToplamKalem = envanterler.Count();
                ViewBag.ToplamAdet = envanterler.Sum(e => e.Adet);

                return View(envanterler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanterler listesi y�klenirken hata olu�tu");
                TempData["ErrorMessage"] = "Envanterler y�klenirken bir hata olu�tu.";
                return View(new List<Envanterler>());
            }
        }

        // GET: Envanterler/Details/5
        [PageAuthorize("Envanterler.Details")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var envanter = await _envanterlerService.GetByIdAsync(id.Value);

                if (envanter == null)
                {
                    return NotFound();
                }

                return View(envanter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanter detay� y�klenirken hata olu�tu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter detay� y�klenirken bir hata olu�tu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Envanterler/Create
        [PageAuthorize("Envanterler.Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Envanterler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PageAuthorize("Envanterler.Create")]
        public async Task<IActionResult> Create([Bind("EnvanterAdi,Adet,AlisFiyat,SatisFiyat,Aciklama")] Envanterler envanter)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _envanterlerService.CreateAsync(envanter);
                    TempData["SuccessMessage"] = "Envanter ba�ar�yla olu�turuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Envanter olu�turulurken hata olu�tu");
                    ModelState.AddModelError("", "Envanter olu�turulurken bir hata olu�tu: " + ex.Message);
                }
            }

            return View(envanter);
        }

        // GET: Envanterler/Edit/5
        [PageAuthorize("Envanterler.Edit")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var envanter = await _envanterlerService.GetByIdAsync(id.Value);

                if (envanter == null)
                {
                    return NotFound();
                }

                return View(envanter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanter d�zenleme sayfas� y�klenirken hata olu�tu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter y�klenirken bir hata olu�tu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Envanterler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PageAuthorize("Envanterler.Edit")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EnvanterAdi,Adet,AlisFiyat,SatisFiyat,Aktif,Aciklama,Version")] Envanterler envanter)
        {
            if (id != envanter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _envanterlerService.UpdateAsync(envanter);
                    TempData["SuccessMessage"] = "Envanter ba�ar�yla g�ncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Bu envanter ba�ka bir kullan�c� taraf�ndan de�i�tirilmi�. L�tfen sayfay� yenileyin.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Envanter g�ncellenirken hata olu�tu. ID: {Id}", id);
                    ModelState.AddModelError("", "Envanter g�ncellenirken bir hata olu�tu: " + ex.Message);
                }
            }

            return View(envanter);
        }

        // GET: Envanterler/Delete/5
        [PageAuthorize("Envanterler.Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var envanter = await _envanterlerService.GetByIdAsync(id.Value);

                if (envanter == null)
                {
                    return NotFound();
                }

                return View(envanter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanter silme sayfas� y�klenirken hata olu�tu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter y�klenirken bir hata olu�tu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Envanterler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PageAuthorize("Envanterler.Delete")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var result = await _envanterlerService.SoftDeleteAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Envanter ba�ar�yla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Envanter bulunamad�.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanter silinirken hata olu�tu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter silinirken bir hata olu�tu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
