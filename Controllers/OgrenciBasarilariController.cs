using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Controllers
{
    [Authorize]
    public class OgrenciBasarilariController : Controller
    {
        private readonly AppDbContext _context;

        public OgrenciBasarilariController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OgrenciBasarilari
        public async Task<IActionResult> Index(long? ogrenciId)
        {
            IQueryable<OgrenciBasarilari> query = _context.OgrenciBasarilari
                .Include(b => b.Ogrenci)
                .Where(b => !b.IsDeleted);

            if (ogrenciId.HasValue)
            {
                query = query.Where(b => b.OgrenciId == ogrenciId.Value);
                var ogrenci = await _context.Ogrenciler.FindAsync(ogrenciId.Value);
                ViewBag.OgrenciAdi = ogrenci != null ? $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}" : "";
                ViewBag.OgrenciId = ogrenciId.Value;
            }

            var basarilar = await query.OrderByDescending(b => b.Tarih).ThenByDescending(b => b.Id).ToListAsync();
            return View(basarilar);
        }

        // GET: OgrenciBasarilari/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenciBasarilari = await _context.OgrenciBasarilari
                .Include(b => b.Ogrenci)
                .Include(b => b.OgrenciBasariMaclari.Where(m => !m.IsDeleted))
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (ogrenciBasarilari == null)
            {
                return NotFound();
            }

            return View(ogrenciBasarilari);
        }

        // GET: OgrenciBasarilari/Create
        public async Task<IActionResult> Create(long? ogrenciId)
        {
            var ogrenciler = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Aktif)
                .OrderBy(o => o.OgrenciSoyadi)
                .ThenBy(o => o.OgrenciAdi)
                .ToListAsync();

            ViewBag.OgrenciId = new SelectList(
                ogrenciler.Select(o => new { Id = o.Id, AdSoyad = $"{o.OgrenciAdi} {o.OgrenciSoyadi}" }),
                "Id",
                "AdSoyad",
                ogrenciId
            );

            var model = new OgrenciBasarilari();
            if (ogrenciId.HasValue)
            {
                model.OgrenciId = ogrenciId.Value;
            }

            return View(model);
        }

        // POST: OgrenciBasarilari/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OgrenciId,Baslik,Aciklama,Turu,Tarih")] OgrenciBasarilari ogrenciBasarilari)
        {
            if (ModelState.IsValid)
            {
                ogrenciBasarilari.Aktif = true;
                ogrenciBasarilari.IsDeleted = false;
                _context.Add(ogrenciBasarilari);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Başarı başarıyla eklendi!";
                return RedirectToAction(nameof(Index), new { ogrenciId = ogrenciBasarilari.OgrenciId });
            }

            var ogrenciler = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Aktif)
                .OrderBy(o => o.OgrenciSoyadi)
                .ThenBy(o => o.OgrenciAdi)
                .ToListAsync();

            ViewBag.OgrenciId = new SelectList(
                ogrenciler.Select(o => new { Id = o.Id, AdSoyad = $"{o.OgrenciAdi} {o.OgrenciSoyadi}" }),
                "Id",
                "AdSoyad",
                ogrenciBasarilari.OgrenciId
            );

            return View(ogrenciBasarilari);
        }

        // GET: OgrenciBasarilari/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenciBasarilari = await _context.OgrenciBasarilari
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (ogrenciBasarilari == null)
            {
                return NotFound();
            }

            var ogrenciler = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Aktif)
                .OrderBy(o => o.OgrenciSoyadi)
                .ThenBy(o => o.OgrenciAdi)
                .ToListAsync();

            ViewBag.OgrenciId = new SelectList(
                ogrenciler.Select(o => new { Id = o.Id, AdSoyad = $"{o.OgrenciAdi} {o.OgrenciSoyadi}" }),
                "Id",
                "AdSoyad",
                ogrenciBasarilari.OgrenciId
            );

            return View(ogrenciBasarilari);
        }

        // POST: OgrenciBasarilari/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,OgrenciId,Baslik,Aciklama,Turu,Tarih,Aktif,IsDeleted,Version")] OgrenciBasarilari ogrenciBasarilari)
        {
            if (id != ogrenciBasarilari.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ogrenciBasarilari);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Başarı başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index), new { ogrenciId = ogrenciBasarilari.OgrenciId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OgrenciBasarilariExists(ogrenciBasarilari.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var ogrenciler = await _context.Ogrenciler
                .Where(o => !o.IsDeleted && o.Aktif)
                .OrderBy(o => o.OgrenciSoyadi)
                .ThenBy(o => o.OgrenciAdi)
                .ToListAsync();

            ViewBag.OgrenciId = new SelectList(
                ogrenciler.Select(o => new { Id = o.Id, AdSoyad = $"{o.OgrenciAdi} {o.OgrenciSoyadi}" }),
                "Id",
                "AdSoyad",
                ogrenciBasarilari.OgrenciId
            );

            return View(ogrenciBasarilari);
        }

        // GET: OgrenciBasarilari/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenciBasarilari = await _context.OgrenciBasarilari
                .Include(b => b.Ogrenci)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (ogrenciBasarilari == null)
            {
                return NotFound();
            }

            return View(ogrenciBasarilari);
        }

        // POST: OgrenciBasarilari/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ogrenciBasarilari = await _context.OgrenciBasarilari.FindAsync(id);
            if (ogrenciBasarilari != null)
            {
                ogrenciBasarilari.IsDeleted = true;
                ogrenciBasarilari.Aktif = false;
                _context.Update(ogrenciBasarilari);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Başarı başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index), new { ogrenciId = ogrenciBasarilari?.OgrenciId });
        }

        private bool OgrenciBasarilariExists(long id)
        {
            return _context.OgrenciBasarilari.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}

