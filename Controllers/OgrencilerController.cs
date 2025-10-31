using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentApp.Models;
using StudentApp.Models.ViewModels;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    public class OgrencilerController : Controller
    {
        private readonly IOgrencilerService _ogrenciService;
        private readonly IOdemePlanlariService _odemePlanlariService;
        private readonly ICinsiyetlerService _cinsiyetlerService;
        private readonly ISmsService _smsService;
        private readonly StudentApp.Data.AppDbContext _context;
        private readonly IZamanlayiciService _schedulerService;

        public OgrencilerController(
            IOgrencilerService ogrencilerService,
            IOdemePlanlariService odemePlanlariService,
            ICinsiyetlerService cinsiyetlerService,
            ISmsService smsService,
            StudentApp.Data.AppDbContext context,
            IZamanlayiciService schedulerService)
        {
            _ogrenciService = ogrencilerService;
            _odemePlanlariService = odemePlanlariService;
            _cinsiyetlerService = cinsiyetlerService;
            _smsService = smsService;
            _context = context;
            _schedulerService = schedulerService;
        }

        // GET: Student
    public async Task<IActionResult> Index(OgrencilerFilterViewModel filter)
  {
      var ogrenciler = await _ogrenciService.GetAllOgrenciAsync(filter.ShowPasif);
  
    // Filtreleme
    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
   {
      var searchTerm = filter.SearchTerm.ToLower();
     ogrenciler = ogrenciler.Where(o =>
     o.OgrenciAdi.ToLower().Contains(searchTerm) ||
   o.OgrenciSoyadi.ToLower().Contains(searchTerm) ||
      o.Email.ToLower().Contains(searchTerm)
    );
 }

   if (filter.CinsiyetId.HasValue && filter.CinsiyetId.Value > 0)
      {
      ogrenciler = ogrenciler.Where(o => o.CinsiyetId == filter.CinsiyetId.Value);
    }

  if (filter.OdemePlanlariId.HasValue && filter.OdemePlanlariId.Value > 0)
    {
    ogrenciler = ogrenciler.Where(o => o.OdemePlanlariId == filter.OdemePlanlariId.Value);
        }

     // Yaş filtreleme
  if (filter.MinYas.HasValue || filter.MaxYas.HasValue)
  {
var today = DateTime.Now;
     ogrenciler = ogrenciler.Where(o =>
    {
var yas = today.Year - o.DogumTarihi.Year;
         if (today.DayOfYear < o.DogumTarihi.DayOfYear) yas--;
       
         var minOk = !filter.MinYas.HasValue || yas >= filter.MinYas.Value;
    var maxOk = !filter.MaxYas.HasValue || yas <= filter.MaxYas.Value;
  return minOk && maxOk;
  });
     }

   // Sıralama
  ogrenciler = filter.SortBy?.ToLower() switch
   {
         "ogrenciadi" => filter.SortOrder == "desc" 
  ? ogrenciler.OrderByDescending(o => o.OgrenciAdi)
   : ogrenciler.OrderBy(o => o.OgrenciAdi),
      "email" => filter.SortOrder == "desc"
  ? ogrenciler.OrderByDescending(o => o.Email)
      : ogrenciler.OrderBy(o => o.Email),
   "dogumtarihi" => filter.SortOrder == "desc"
         ? ogrenciler.OrderByDescending(o => o.DogumTarihi)
         : ogrenciler.OrderBy(o => o.DogumTarihi),
    "kayittarihi" => filter.SortOrder == "desc"
      ? ogrenciler.OrderByDescending(o => o.KayitTarihi)
  : ogrenciler.OrderBy(o => o.KayitTarihi),
   _ => filter.SortOrder == "desc"
    ? ogrenciler.OrderByDescending(o => o.OgrenciSoyadi)
  : ogrenciler.OrderBy(o => o.OgrenciSoyadi)
        };

      var viewModel = new OgrencilerFilterViewModel
       {
    Ogrenciler = ogrenciler.ToList(),
     SearchTerm = filter.SearchTerm,
    CinsiyetId = filter.CinsiyetId,
  OdemePlanlariId = filter.OdemePlanlariId,
  MinYas = filter.MinYas,
 MaxYas = filter.MaxYas,
        ShowPasif = filter.ShowPasif,
   SortBy = filter.SortBy ?? "OgrenciSoyadi",
 SortOrder = filter.SortOrder ?? "asc",
     Cinsiyetler = await _cinsiyetlerService.GetAllCinsiyetlerAsync(),
  OdemePlanlari = await _odemePlanlariService.GetAllOdemePlanlariAsync()
  };

      return View(viewModel);
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

        // POST: Student/ToggleAktif/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAktif(long id, bool aktif)
        {
  try
 {
        var result = await _ogrenciService.ToggleAktifAsync(id, aktif);
   if (result)
   {
        TempData["SuccessMessage"] = aktif 
   ? "Öğrenci başarıyla aktif hale getirildi!" 
     : "Öğrenci başarıyla pasif hale getirildi!";
        }
    else
  {
TempData["ErrorMessage"] = "Öğrenci bulunamadı.";
}
    }
    catch (Exception ex)
{
  TempData["ErrorMessage"] = "Durum güncellenirken bir hata oluştu.";
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

        // POST: Ogrenciler/SendSmsSelected
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSmsSelected([FromForm] long[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    return Json(new { success = false, message = "SMS göndermek için en az bir öğrenci seçiniz." });
                }

                var today = DateTime.Today;
                var activeSettings = await _schedulerService.GetActiveSchedulerAsync();
                var template = activeSettings?.MesajSablonu ?? "Sayın [ÖĞRENCİ_ADI] [ÖĞRENCİ_SOYADI], ödemeniz [REFERANS_TARIH] tarihinden beri yapılmamıştır. Lütfen ödemenizi yapınız.";

                var students = await _context.Ogrenciler
                    .Include(s => s.OdemePlanlari)
                    .Where(s => selectedIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.OgrenciAdi, s.OgrenciSoyadi, s.Telefon, s.KayitTarihi, s.SonSmsTarihi, PlanTutar = (decimal?)s.OdemePlanlari.Tutar })
                    .ToListAsync();

                var latestPayments = await _context.OgrenciOdemeTakvimi
                    .Where(p => selectedIds.Contains(p.OgrenciId) && !p.IsDeleted && p.OdemeTarihi != null)
                    .OrderByDescending(p => p.OdemeTarihi).ThenByDescending(p => p.Id)
                    .ToListAsync();

                var paymentLookup = latestPayments
                    .GroupBy(x => x.OgrenciId)
                    .Select(g => g.First())
                    .ToDictionary(x => x.OgrenciId, x => x);

                var smsList = new List<(string phone, string message)>();
                var updatableIds = new List<long>();

                foreach (var s in students)
                {
                    if (string.IsNullOrWhiteSpace(s.Telefon))
                        continue;

                    var lastPay = paymentLookup.TryGetValue(s.Id, out var last) ? last : null;
                    var referenceDate = lastPay?.OdemeTarihi?.Date ?? s.KayitTarihi.Date;
                    var days = (today - referenceDate).Days;
                    var borc = lastPay?.BorcTutari ?? (s.PlanTutar ?? 0m);

                    var message = template
                        .Replace("[ÖĞRENCİ_ADI]", s.OgrenciAdi ?? "")
                        .Replace("[ÖĞRENCİ_SOYADI]", s.OgrenciSoyadi ?? "")
                        .Replace("[GEÇEN_GÜN]", days.ToString())
                        .Replace("[BORÇ_TUTARI]", borc.ToString("N2"))
                        .Replace("[REFERANS_TARIH]", referenceDate.ToString("dd.MM.yyyy"));
                    smsList.Add((s.Telefon!, message));
                    updatableIds.Add(s.Id);
                }

                if (!smsList.Any())
                {
                    return Json(new { success = false, message = "Kriterlere uyan seçili öğrenci bulunamadı veya gönderilecek SMS yok." });
                }

                var sendOk = await _smsService.SendBulkSmsAsync(smsList);
                if (!sendOk)
                {
                    return Json(new { success = false, message = "Toplu SMS gönderimi başarısız oldu." });
                }

                var toUpdate = await _context.Ogrenciler.Where(s => updatableIds.Contains(s.Id)).ToListAsync();
                foreach (var s in toUpdate)
                {
                    s.SonSmsTarihi = today;
                }
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{toUpdate.Count} öğrenciye SMS gönderildi ve SonSmsTarihi güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İşlem sırasında beklenmeyen bir hata oluştu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportSelectedToExcel([FromForm] long[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
                return BadRequest("Seçim yok");

            var rows = await _context.Ogrenciler
                .Where(s => selectedIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.OgrenciAdi,
                    s.OgrenciSoyadi,
                    s.Telefon,
                    s.Email,
                    s.KayitTarihi,
                    s.SonSmsTarihi
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ogrenciler");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Ad";
            ws.Cell(1, 3).Value = "Soyad";
            ws.Cell(1, 4).Value = "Telefon";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Kayıt Tarihi";
            ws.Cell(1, 7).Value = "Son SMS Tarihi";

            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.Id;
                ws.Cell(r, 2).Value = x.OgrenciAdi;
                ws.Cell(r, 3).Value = x.OgrenciSoyadi;
                ws.Cell(r, 4).Value = x.Telefon;
                ws.Cell(r, 5).Value = x.Email;
                ws.Cell(r, 6).Value = x.KayitTarihi;
                ws.Cell(r, 7).Value = x.SonSmsTarihi;
                r++;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;
            var bytes = ms.ToArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ogrenciler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportSelectedToPdf([FromForm] long[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
                return BadRequest("Seçim yok");

            var rows = await _context.Ogrenciler
                .Where(s => selectedIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.OgrenciAdi,
                    s.OgrenciSoyadi,
                    s.Telefon,
                    s.Email,
                    s.KayitTarihi,
                    s.SonSmsTarihi
                })
                .ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Öğrenci Listesi").SemiBold().FontSize(16);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Id");
                            header.Cell().Text("Ad");
                            header.Cell().Text("Soyad");
                            header.Cell().Text("Telefon");
                            header.Cell().Text("Email");
                            header.Cell().Text("Kayıt Tarihi");
                            header.Cell().Text("Son SMS");
                        });

                        foreach (var x in rows)
                        {
                            table.Cell().Text(x.Id.ToString());
                            table.Cell().Text(x.OgrenciAdi ?? "");
                            table.Cell().Text(x.OgrenciSoyadi ?? "");
                            table.Cell().Text(x.Telefon ?? "");
                            table.Cell().Text(x.Email ?? "");
                            table.Cell().Text(x.KayitTarihi.ToString("dd.MM.yyyy"));
                            table.Cell().Text(x.SonSmsTarihi?.ToString("dd.MM.yyyy") ?? "");
                        }
                    });
                    page.Footer().AlignRight().Text($"Oluşturma: {DateTime.Now:dd.MM.yyyy HH:mm}");
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            ms.Position = 0;
            return File(ms.ToArray(), "application/pdf", $"ogrenciler_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}
