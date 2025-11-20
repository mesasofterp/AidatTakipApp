using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;

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

                var toplamSatisDeger = envanterler.Sum(e => e.Adet * e.SatisFiyat);

                ViewBag.ToplamSatisDeger = toplamSatisDeger;
                ViewBag.ToplamKalem = envanterler.Count();
                ViewBag.ToplamAdet = envanterler.Sum(e => e.Adet);

                return View(envanterler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanterler listesi yüklenirken hata oluþtu");
                TempData["ErrorMessage"] = "Envanterler yüklenirken bir hata oluþtu.";
                return View(new List<Envanterler>());
            }
        }

        // GET: Envanterler/Details/5
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
                _logger.LogError(ex, "Envanter detayý yüklenirken hata oluþtu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter detayý yüklenirken bir hata oluþtu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Envanterler/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Envanterler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnvanterAdi,Adet,AlisFiyat,SatisFiyat,Aciklama")] Envanterler envanter)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _envanterlerService.CreateAsync(envanter);
                    TempData["SuccessMessage"] = "Envanter baþarýyla oluþturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Envanter oluþturulurken hata oluþtu");
                    ModelState.AddModelError("", "Envanter oluþturulurken bir hata oluþtu: " + ex.Message);
                }
            }

            return View(envanter);
        }

        // GET: Envanterler/Edit/5
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
                _logger.LogError(ex, "Envanter düzenleme sayfasý yüklenirken hata oluþtu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter yüklenirken bir hata oluþtu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Envanterler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    TempData["SuccessMessage"] = "Envanter baþarýyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Bu envanter baþka bir kullanýcý tarafýndan deðiþtirilmiþ. Lütfen sayfayý yenileyin.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Envanter güncellenirken hata oluþtu. ID: {Id}", id);
                    ModelState.AddModelError("", "Envanter güncellenirken bir hata oluþtu: " + ex.Message);
                }
            }

            return View(envanter);
        }

        // GET: Envanterler/Delete/5
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
                _logger.LogError(ex, "Envanter silme sayfasý yüklenirken hata oluþtu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter yüklenirken bir hata oluþtu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Envanterler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var result = await _envanterlerService.SoftDeleteAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Envanter baþarýyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Envanter bulunamadý.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Envanter silinirken hata oluþtu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Envanter silinirken bir hata oluþtu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
