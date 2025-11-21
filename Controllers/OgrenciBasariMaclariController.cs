using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Controllers
{
    [Authorize]
    public class OgrenciBasariMaclariController : Controller
    {
        private readonly AppDbContext _context;

        public OgrenciBasariMaclariController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OgrenciBasariMaclari
        public async Task<IActionResult> Index(long? basariId)
        {
            IQueryable<OgrenciBasariMaclari> query = _context.OgrenciBasariMaclari
                .Include(m => m.Basari)
                .ThenInclude(b => b.Ogrenci)
                .Where(m => !m.IsDeleted);

            if (basariId.HasValue)
            {
                query = query.Where(m => m.BasariId == basariId.Value);
                var basari = await _context.OgrenciBasarilari
                    .Include(b => b.Ogrenci)
                    .FirstOrDefaultAsync(b => b.Id == basariId.Value);
                if (basari != null)
                {
                    ViewBag.BasariBaslik = basari.Baslik;
                    ViewBag.BasariId = basariId.Value;
                    ViewBag.OgrenciAdi = $"{basari.Ogrenci.OgrenciAdi} {basari.Ogrenci.OgrenciSoyadi}";
                }
            }

            var maclar = await query.OrderByDescending(m => m.Tarih).ThenByDescending(m => m.Id).ToListAsync();
            return View(maclar);
        }

        // GET: OgrenciBasariMaclari/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenciBasariMaclari = await _context.OgrenciBasariMaclari
                .Include(m => m.Basari)
                .ThenInclude(b => b.Ogrenci)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (ogrenciBasariMaclari == null)
            {
                return NotFound();
            }

            return View(ogrenciBasariMaclari);
        }

        // GET: OgrenciBasariMaclari/Create
        public async Task<IActionResult> Create(long? basariId)
        {
            if (!basariId.HasValue)
            {
                return NotFound();
            }

            var basari = await _context.OgrenciBasarilari
                .Include(b => b.Ogrenci)
                .FirstOrDefaultAsync(b => b.Id == basariId.Value && !b.IsDeleted);

            if (basari == null)
            {
                return NotFound();
            }

            ViewBag.BasariBaslik = basari.Baslik;
            ViewBag.BasariId = basariId.Value;

            var model = new OgrenciBasariMaclari
            {
                BasariId = basariId.Value
            };

            return View(model);
        }

        // POST: OgrenciBasariMaclari/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BasariId,RakipAdi,Tur,Kategori,Skor,Sonuc,Tarih,Lokasyon")] OgrenciBasariMaclari ogrenciBasariMaclari)
        {
            if (ModelState.IsValid)
            {
                ogrenciBasariMaclari.Aktif = true;
                ogrenciBasariMaclari.IsDeleted = false;
                _context.Add(ogrenciBasariMaclari);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Maç başarıyla eklendi!";
                return RedirectToAction(nameof(Index), new { basariId = ogrenciBasariMaclari.BasariId });
            }

            var basari = await _context.OgrenciBasarilari
                .Include(b => b.Ogrenci)
                .FirstOrDefaultAsync(b => b.Id == ogrenciBasariMaclari.BasariId && !b.IsDeleted);

            if (basari != null)
            {
                ViewBag.BasariBaslik = basari.Baslik;
                ViewBag.BasariId = ogrenciBasariMaclari.BasariId;
            }

            return View(ogrenciBasariMaclari);
        }

        // GET: OgrenciBasariMaclari/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenciBasariMaclari = await _context.OgrenciBasariMaclari
                .Include(m => m.Basari)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (ogrenciBasariMaclari == null)
            {
                return NotFound();
            }

            ViewBag.BasariBaslik = ogrenciBasariMaclari.Basari?.Baslik;
            ViewBag.BasariId = ogrenciBasariMaclari.BasariId;

            return View(ogrenciBasariMaclari);
        }

        // POST: OgrenciBasariMaclari/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,BasariId,RakipAdi,Tur,Kategori,Skor,Sonuc,Tarih,Lokasyon,Aktif,IsDeleted,Version")] OgrenciBasariMaclari ogrenciBasariMaclari)
        {
            if (id != ogrenciBasariMaclari.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ogrenciBasariMaclari);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Maç başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index), new { basariId = ogrenciBasariMaclari.BasariId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OgrenciBasariMaclariExists(ogrenciBasariMaclari.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var basari = await _context.OgrenciBasarilari
                .FirstOrDefaultAsync(b => b.Id == ogrenciBasariMaclari.BasariId && !b.IsDeleted);

            if (basari != null)
            {
                ViewBag.BasariBaslik = basari.Baslik;
                ViewBag.BasariId = ogrenciBasariMaclari.BasariId;
            }

            return View(ogrenciBasariMaclari);
        }

        // GET: OgrenciBasariMaclari/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenciBasariMaclari = await _context.OgrenciBasariMaclari
                .Include(m => m.Basari)
                .ThenInclude(b => b.Ogrenci)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (ogrenciBasariMaclari == null)
            {
                return NotFound();
            }

            return View(ogrenciBasariMaclari);
        }

        // POST: OgrenciBasariMaclari/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ogrenciBasariMaclari = await _context.OgrenciBasariMaclari.FindAsync(id);
            if (ogrenciBasariMaclari != null)
            {
                ogrenciBasariMaclari.IsDeleted = true;
                ogrenciBasariMaclari.Aktif = false;
                _context.Update(ogrenciBasariMaclari);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Maç başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index), new { basariId = ogrenciBasariMaclari?.BasariId });
        }

        private bool OgrenciBasariMaclariExists(long id)
        {
            return _context.OgrenciBasariMaclari.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}

