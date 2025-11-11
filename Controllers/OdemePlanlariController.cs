using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    [Authorize]
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
                    TempData["SuccessMessage"] = "Ödeme planı başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ödeme planı eklenirken bir hata oluştu. Lütfen tekrar deneyin.");
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

                    TempData["SuccessMessage"] = "Ödeme planı başarıyla güncellendi! Bu plana bağlı öğrencilerin ödenmemiş taksitleri de otomatik olarak güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ödeme planı güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
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
                    TempData["SuccessMessage"] = "Ödeme planı başarıyla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ödeme planı bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ödeme planı silinirken bir hata oluştu.";
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
                        ? "Ödeme planı başarıyla aktif hale getirildi!"
                        : "Ödeme planı başarıyla pasif hale getirildi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ödeme planı bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Durum güncellenirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: OdemePlanlari/ExportToExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToExcel(bool showPasif = false)
        {
            var list = await _odemePlaniService.GetAllOdemePlanlariAsync(showPasif);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("OdemePlanlari");

            // Başlık satırı
            int c = 1;
            ws.Cell(1, c++).Value = "Kurs Programı";
            ws.Cell(1, c++).Value = "Toplam Tutar";
            ws.Cell(1, c++).Value = "Taksit Sayısı";
            ws.Cell(1, c++).Value = "Taksit Tutarı";
            ws.Cell(1, c++).Value = "Vade (Gün)";
            ws.Cell(1, c++).Value = "Açıklama";
            ws.Cell(1, c++).Value = "Durum";

            // Başlık satırını stillendir
            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGreen;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Veri satırları
            int r = 2;
            foreach (var x in list.OrderBy(o => o.KursProgrami))
            {
                int col = 1;
                ws.Cell(r, col++).Value = x.KursProgrami;
                ws.Cell(r, col++).Value = x.ToplamTutar;
                ws.Cell(r, col++).Value = x.TaksitSayisi;
                ws.Cell(r, col++).Value = x.TaksitTutari;
                ws.Cell(r, col++).Value = x.Vade ?? 0;
                ws.Cell(r, col++).Value = x.Aciklama ?? "-";
                ws.Cell(r, col++).Value = x.Aktif ? "Aktif" : "Pasif";

                // Pasif planları vurgula
                if (!x.Aktif)
                {
                    ws.Row(r).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                r++;
            }

            // Para formatı uygula
            ws.Column(2).Style.NumberFormat.Format = "#,##0.00 ₺"; // Toplam Tutar
            ws.Column(4).Style.NumberFormat.Format = "#,##0.00 ₺"; // Taksit Tutarı

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var fileName = $"odeme_planlari_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // POST: OdemePlanlari/ExportToPdf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToPdf(bool showPasif = false)
        {
            var list = await _odemePlaniService.GetAllOdemePlanlariAsync(showPasif);

            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Ödeme Planları Listesi").SemiBold().FontSize(16);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Kurs Programı
                            columns.RelativeColumn(1.8f); // Toplam Tutar
                            columns.RelativeColumn(1.2f); // Taksit Sayısı
                            columns.RelativeColumn(1.8f); // Taksit Tutarı
                            columns.RelativeColumn(1.2f); // Vade
                            columns.RelativeColumn(2); // Açıklama
                            columns.RelativeColumn(1.2f); // Durum
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Kurs Programı").FontSize(9);
                            header.Cell().Element(CellStyle).Text("Toplam Tutar").FontSize(9);
                            header.Cell().Element(CellStyle).Text("Taksit").FontSize(9);
                            header.Cell().Element(CellStyle).Text("Taksit Tutarı").FontSize(9);
                            header.Cell().Element(CellStyle).Text("Vade").FontSize(9);
                            header.Cell().Element(CellStyle).Text("Açıklama").FontSize(9);
                            header.Cell().Element(CellStyle).Text("Durum").FontSize(9);

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5);
                            }
                        });

                        foreach (var x in list.OrderBy(o => o.KursProgrami))
                        {
                            table.Cell().Text(x.KursProgrami).FontSize(8);
                            table.Cell().Text(x.ToplamTutar.ToString("N2") + " ₺").FontSize(8);
                            table.Cell().Text(x.TaksitSayisi.ToString()).FontSize(8);
                            table.Cell().Text(x.TaksitTutari.ToString("N2") + " ₺").FontSize(8);
                            table.Cell().Text(x.Vade.HasValue ? x.Vade.Value.ToString() + " gün" : "-").FontSize(8);
                            table.Cell().Text(x.Aciklama ?? "-").FontSize(7);
                            table.Cell().Text(x.Aktif ? "✓ Aktif" : "○ Pasif")
                                .FontSize(8)
                                .FontColor(x.Aktif ? QuestPDF.Helpers.Colors.Green.Darken2 : QuestPDF.Helpers.Colors.Grey.Darken1);
                        }
                    });
                    page.Footer().AlignRight().Text($"Oluşturma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8);
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            ms.Position = 0;

            var fileName = $"odeme_planlari_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }
    }
}
