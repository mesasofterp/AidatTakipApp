using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudentApp.Models;
using StudentApp.Services;
using StudentApp.Attributes;

namespace StudentApp.Controllers
{
    [Authorize]
    public class OgrenciOdemeTakvimiController : Controller
    {
        private readonly IOgrenciOdemeTakvimiService _odemeService;
   private readonly IOgrencilerService _ogrenciService;

        public OgrenciOdemeTakvimiController(
            IOgrenciOdemeTakvimiService odemeService,
    IOgrencilerService ogrencilerService)
{
            _odemeService = odemeService;
       _ogrenciService = ogrencilerService;
        }

        // GET: OgrenciOdemeTakvimi
        [PageAuthorize("OgrenciOdemeTakvimi.Index")]
        public async Task<IActionResult> Index(long? ogrenciId, string searchTerm)
        {
   IEnumerable<OgrenciOdemeTakvimi> odemeler;
      
         if (ogrenciId.HasValue)
    {
   odemeler = await _odemeService.GetOdemelerByOgrenciIdAsync(ogrenciId.Value);
      var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
           ViewBag.OgrenciAdi = ogrenci != null ? $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}" : "";
        ViewBag.OgrenciId = ogrenciId.Value;
            }
     else
  {
        odemeler = await _odemeService.GetAllOdemelerAsync();
 
       // Arama filtresi uygula
          if (!string.IsNullOrWhiteSpace(searchTerm))
            {
     var searchLower = searchTerm.ToLower().Trim();
                odemeler = odemeler.Where(o => 
          o.Ogrenci != null && (
  o.Ogrenci.OgrenciAdi.ToLower().Contains(searchLower) ||
      o.Ogrenci.OgrenciSoyadi.ToLower().Contains(searchLower) ||
          (o.Ogrenci.TCNO != null && o.Ogrenci.TCNO.Contains(searchLower)) ||
               $"{o.Ogrenci.OgrenciAdi} {o.Ogrenci.OgrenciSoyadi}".ToLower().Contains(searchLower)
             )
          );
           ViewBag.SearchTerm = searchTerm;
   }
   
            // Tüm öğrenciler için toplam kalan borç
       ViewBag.ToplamKalanBorc = await _odemeService.GetToplamKalanBorcAsync();
       }

            // Filtre dropdown için öğrenci listesi (yalnızca aktifler)
          var ogrenciler = await _ogrenciService.GetAllOgrenciAsync(false);
            ViewBag.OgrenciList = new SelectList(
   ogrenciler.Select(o => new { Id = o.Id, AdSoyad = ($"{o.OgrenciAdi} {o.OgrenciSoyadi}") }),
     "Id",
"AdSoyad",
       ogrenciId
            );

return View(odemeler);
   }

        // GET: OgrenciOdemeTakvimi/Create
        [PageAuthorize("OgrenciOdemeTakvimi.Create")]
        public async Task<IActionResult> Create(long? ogrenciId)
 {
            // Öğrenci ID'si zorunlu - yoksa öğrenci listesine yönlendir
            if (!ogrenciId.HasValue)
    {
        TempData["ErrorMessage"] = "Ödeme girişi için önce bir öğrenci seçmelisiniz.";
           return RedirectToAction("Index", "Ogrenciler");
            }

            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
            if (ogrenci == null)
        {
     TempData["ErrorMessage"] = "Öğrenci bulunamadı.";
          return RedirectToAction("Index", "Ogrenciler");
        }
    
            ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
       ViewBag.OgrenciId = ogrenciId.Value;
    
            // Kalan borç hesapla
   var kalanBorc = await _odemeService.GetKalanBorcAsync(ogrenciId.Value);
     
       // Eğer hiç ödeme yoksa, toplam tutar = başlangıç borcu
            var toplamOdenen = await _odemeService.GetToplamOdenenTutarAsync(ogrenciId.Value);
  
   if (kalanBorc == 0 && toplamOdenen == 0 && ogrenci.OdemePlanlari != null && !ogrenci.OdemePlanlari.IsDeleted)
      {
         // İlk ödeme - kalan borç toplam tutara eşit
    kalanBorc = ogrenci.OdemePlanlari.ToplamTutar;
     }

   ViewBag.KalanBorc = kalanBorc;
       ViewBag.ToplamOdenen = toplamOdenen;
            ViewBag.OdemePlani = ogrenci.OdemePlanlari;

       return View(new OgrenciOdemeTakvimi 
   { 
    OgrenciId = ogrenciId.Value,
  OdemeTarihi = DateTime.Now
   });
 }

    // POST: OgrenciOdemeTakvimi/Create
      [HttpPost]
  [ValidateAntiForgeryToken]
        [PageAuthorize("OgrenciOdemeTakvimi.Create")]
        public async Task<IActionResult> Create(OgrenciOdemeTakvimi odeme)
        {
      // Öğrenci ID kontrolü
       if (odeme.OgrenciId <= 0)
          {
   TempData["ErrorMessage"] = "Geçersiz öğrenci bilgisi.";
     return RedirectToAction("Index", "Ogrenciler");
 }

            if (ModelState.IsValid)
            {
        try
    {
           await _odemeService.AddOdemeAsync(odeme);
  TempData["SuccessMessage"] = "Ödeme başarıyla kaydedildi!";
       return RedirectToAction(nameof(Index), new { ogrenciId = odeme.OgrenciId });
}
        catch (Exception ex)
            {
      ModelState.AddModelError("", $"Ödeme kaydedilirken bir hata oluştu: {ex.Message}");
  }
    }

            // Hata durumunda öğrenci bilgilerini tekrar yükle
            var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(odeme.OgrenciId);
       if (ogrenci != null)
   {
       ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
    ViewBag.OgrenciId = odeme.OgrenciId;
      
           var kalanBorc = await _odemeService.GetKalanBorcAsync(odeme.OgrenciId);
         var toplamOdenen = await _odemeService.GetToplamOdenenTutarAsync(odeme.OgrenciId);
          
          // Eğer hiç ödeme yoksa, toplam tutar = başlangıç borcu
                if (kalanBorc == 0 && toplamOdenen == 0 && ogrenci.OdemePlanlari != null && !ogrenci.OdemePlanlari.IsDeleted)
      {
   kalanBorc = ogrenci.OdemePlanlari.ToplamTutar;
                }

      ViewBag.KalanBorc = kalanBorc;
    ViewBag.ToplamOdenen = toplamOdenen;
ViewBag.OdemePlani = ogrenci.OdemePlanlari;
}

            return View(odeme);
        }

        // GET: OgrenciOdemeTakvimi/Details/5
        [PageAuthorize("OgrenciOdemeTakvimi.Details")]
  public async Task<IActionResult> Details(long id)
  {
      var odeme = await _odemeService.GetOdemeByIdAsync(id);
     if (odeme == null)
  {
      return NotFound();
       }

   return View(odeme);
     }

        // GET: OgrenciOdemeTakvimi/Edit/5
        [PageAuthorize("OgrenciOdemeTakvimi.Edit")]
     public async Task<IActionResult> Edit(long id)
   {
      var odeme = await _odemeService.GetOdemeByIdAsync(id);
   if (odeme == null)
   {
       return NotFound();
   }

       if (odeme.Ogrenci != null)
{
     ViewBag.OgrenciAdi = $"{odeme.Ogrenci.OgrenciAdi} {odeme.Ogrenci.OgrenciSoyadi}";
     }

  return View(odeme);
  }

        // POST: OgrenciOdemeTakvimi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PageAuthorize("OgrenciOdemeTakvimi.Edit")]
   public async Task<IActionResult> Edit(long id, OgrenciOdemeTakvimi odeme)
        {
   if (id != odeme.Id)
  {
          return NotFound();
   }

        if (ModelState.IsValid)
    {
    try
        {
      var updatedOdeme = await _odemeService.UpdateOdemeAsync(odeme);
if (updatedOdeme == null)
     {
     return NotFound();
      }

       TempData["SuccessMessage"] = "Ödeme başarıyla güncellendi! Kalan borç tutarları yeniden hesaplandı.";
           return RedirectToAction(nameof(Index), new { ogrenciId = odeme.OgrenciId });
          }
    catch (Exception ex)
      {
 ModelState.AddModelError("", $"Ödeme güncellenirken bir hata oluştu: {ex.Message}");
        }
  }

   var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(odeme.OgrenciId);
  if (ogrenci != null)
       {
          ViewBag.OgrenciAdi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
   }

            return View(odeme);
        }

        // GET: OgrenciOdemeTakvimi/Delete/5
        [PageAuthorize("OgrenciOdemeTakvimi.Delete")]
        public async Task<IActionResult> Delete(long id)
      {
       var odeme = await _odemeService.GetOdemeByIdAsync(id);
 if (odeme == null)
  {
        return NotFound();
      }

 return View(odeme);
     }

        // POST: OgrenciOdemeTakvimi/Delete/5
   [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
        [PageAuthorize("OgrenciOdemeTakvimi.Delete")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
      try
{
    var odeme = await _odemeService.GetOdemeByIdAsync(id);
       var result = await _odemeService.DeleteOdemeAsync(id);
    if (result)
      {
  TempData["SuccessMessage"] = "Ödeme başarıyla silindi! Kalan borç tutarları yeniden hesaplandı.";
    if (odeme != null)
       {
      return RedirectToAction(nameof(Index), new { ogrenciId = odeme.OgrenciId });
   }
   }
  else
   {
  TempData["ErrorMessage"] = "Ödeme bulunamadı.";
  }
       }
   catch (Exception ex)
  {
 TempData["ErrorMessage"] = "Ödeme silinirken bir hata oluştu.";
       }

       return RedirectToAction(nameof(Index));
 }

        // Utility: Tüm öğrenciler için kalan borcu yeniden hesapla
        [HttpPost]
        [ValidateAntiForgeryToken]
 public async Task<IActionResult> RecalculateAllBorcs()
        {
  try
            {
 var ogrenciler = await _ogrenciService.GetAllOgrenciAsync(true); // true = pasif öğrenciler dahil
     int hesaplananOgrenciSayisi = 0;

      foreach (var ogrenci in ogrenciler)
     {
         await _odemeService.RecalculateKalanBorcForOgrenciAsync(ogrenci.Id);
   hesaplananOgrenciSayisi++;
       }

   TempData["SuccessMessage"] = $"Tüm öğrencilerin ({hesaplananOgrenciSayisi}) kalan borç tutarları başarıyla yeniden hesaplandı!";
            }
     catch (Exception ex)
  {
TempData["ErrorMessage"] = $"Kalan borç hesaplanırken bir hata oluştu: {ex.Message}";
  }

  return RedirectToAction(nameof(Index));
   }

        // POST: OgrenciOdemeTakvimi/MarkAsOdendi/5
        [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> MarkAsOdendi(long id, long? ogrenciId)
      {
  try
       {
            var result = await _odemeService.MarkAsOdendiAsync(id);
         if (result)
     {
      TempData["SuccessMessage"] = "Taksit başarıyla ödendi olarak işaretlendi!";
  }
   else
            {
         TempData["ErrorMessage"] = "Ödeme kaydı bulunamadı veya zaten ödenmişti.";
    }
            }
    catch (Exception ex)
            {
         TempData["ErrorMessage"] = $"Ödeme işaretlenirken bir hata oluştu: {ex.Message}";
          }

            if (ogrenciId.HasValue)
        {
   return RedirectToAction(nameof(Index), new { ogrenciId });
  }

 return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToExcel(long? ogrenciId)
        {
       IEnumerable<OgrenciOdemeTakvimi> list;
 string sheetName;

         if (ogrenciId.HasValue)
   {
             list = await _odemeService.GetOdemelerByOgrenciIdAsync(ogrenciId.Value);
 var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
      sheetName = (ogrenci != null ? ($"{ogrenci.OgrenciAdi}_{ogrenci.OgrenciSoyadi}") : "Ogrenci") + "_Odemeler";
       }
       else
          {
     list = await _odemeService.GetAllOdemelerAsync();
           sheetName = "TumOdemeler";
       }

            using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(sheetName);

            // Başlık satırı
        int c = 1;
         if (!ogrenciId.HasValue)
 {
      ws.Cell(1, c++).Value = "Öğrenci";
            }
        ws.Cell(1, c++).Value = "Taksit No";
     ws.Cell(1, c++).Value = "Taksit Tutarı";
       ws.Cell(1, c++).Value = "Son Ödeme Tarihi";
   ws.Cell(1, c++).Value = "Ödeme Tarihi";
            ws.Cell(1, c++).Value = "Ödenen Tutar";
  ws.Cell(1, c++).Value = "Kalan Borç";
     ws.Cell(1, c++).Value = "Durum";
 ws.Cell(1, c++).Value = "Açıklama";

     // Başlık satırını stillendir
 var headerRow = ws.Row(1);
     headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Veri satırları - Öğrenci ve Taksit No'ya göre sıralı
 int r = 2;
      var sortedList = ogrenciId.HasValue 
         ? list.OrderBy(o => o.TaksitNo ?? 0)
           : list.OrderBy(o => o.Ogrenci != null ? o.Ogrenci.OgrenciSoyadi : "")
   .ThenBy(o => o.Ogrenci != null ? o.Ogrenci.OgrenciAdi : "")
              .ThenBy(o => o.TaksitNo ?? 0);

            foreach (var x in sortedList)
      {
   int col = 1;
       if (!ogrenciId.HasValue)
     {
      ws.Cell(r, col++).Value = x.Ogrenci != null ? ($"{x.Ogrenci.OgrenciAdi} {x.Ogrenci.OgrenciSoyadi}") : "";
    }
          ws.Cell(r, col++).Value = x.TaksitNo.HasValue ? $"Taksit {x.TaksitNo}" : "-";
ws.Cell(r, col++).Value = x.TaksitTutari ?? 0;
        ws.Cell(r, col++).Value = x.SonOdemeTarihi?.ToString("dd.MM.yyyy") ?? "-";
  ws.Cell(r, col++).Value = x.OdemeTarihi?.ToString("dd.MM.yyyy") ?? "-";
    ws.Cell(r, col++).Value = x.OdenenTutar;
    ws.Cell(r, col++).Value = x.BorcTutari;
     ws.Cell(r, col++).Value = x.Odendi ? "Ödendi" : "Bekliyor";
   ws.Cell(r, col++).Value = x.Aciklama ?? "";

      // Ödenmemiş satırları vurgula
          if (!x.Odendi)
    {
        ws.Row(r).Style.Fill.BackgroundColor = XLColor.LightYellow;
          }

   // Gecikmiş ödemeleri kırmızı yap
   if (!x.Odendi && x.SonOdemeTarihi.HasValue && x.SonOdemeTarihi.Value.Date < DateTime.Today)
     {
           ws.Row(r).Style.Fill.BackgroundColor = XLColor.LightPink;
    }

    r++;
         }

 // Para formatı uygula
  ws.Column(ogrenciId.HasValue ? 3 : 4).Style.NumberFormat.Format = "#,##0.00 ₺";
     ws.Column(ogrenciId.HasValue ? 5 : 6).Style.NumberFormat.Format = "#,##0.00 ₺";
 ws.Column(ogrenciId.HasValue ? 6 : 7).Style.NumberFormat.Format = "#,##0.00 ₺";

      ws.Columns().AdjustToContents();

    using var ms = new MemoryStream();
  wb.SaveAs(ms);
          ms.Position = 0;

var fileName = (ogrenciId.HasValue ? $"odeme_{ogrenciId.Value}_" : "odemeler_") + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToPdf(long? ogrenciId)
        {
            IEnumerable<OgrenciOdemeTakvimi> list;
            string title;

            if (ogrenciId.HasValue)
            {
                list = await _odemeService.GetOdemelerByOgrenciIdAsync(ogrenciId.Value);
                var ogrenci = await _ogrenciService.GetOgrenciByIdAsync(ogrenciId.Value);
                title = (ogrenci != null ? ($"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}") : "Öğrenci") + " Ödemeleri";
            }
            else
            {
                list = await _odemeService.GetAllOdemelerAsync();
                title = "Tüm Ödemeler";
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text(title).SemiBold().FontSize(16);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            if (!ogrenciId.HasValue)
                                columns.RelativeColumn(2.5f); // Öğrenci
columns.RelativeColumn(1.5f); // Taksit No
   columns.RelativeColumn(2); // Taksit Tutarı
 columns.RelativeColumn(2); // Son Ödeme Tarihi
           columns.RelativeColumn(2); // Ödeme Tarihi
          columns.RelativeColumn(2); // Ödenen Tutar
columns.RelativeColumn(2); // Kalan Borç
      columns.RelativeColumn(1.5f); // Durum
    columns.RelativeColumn(3); // Açıklama
     });

  table.Header(header =>
    {
    if (!ogrenciId.HasValue)
         header.Cell().Element(CellStyle).Text("Öğrenci").FontSize(10);
  header.Cell().Element(CellStyle).Text("Taksit No").FontSize(10);
 header.Cell().Element(CellStyle).Text("Taksit Tutarı").FontSize(10);
   header.Cell().Element(CellStyle).Text("Son Ödeme").FontSize(10);
     header.Cell().Element(CellStyle).Text("Ödeme Tarihi").FontSize(10);
    header.Cell().Element(CellStyle).Text("Ödenen").FontSize(10);
header.Cell().Element(CellStyle).Text("Kalan Borç").FontSize(10);
     header.Cell().Element(CellStyle).Text("Durum").FontSize(10);
 header.Cell().Element(CellStyle).Text("Açıklama").FontSize(10);

 static IContainer CellStyle(IContainer container)
      {
   return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5);
 }
    });

 // Veri satırları - Öğrenci ve Taksit No'ya göre sıralı
            var sortedList = ogrenciId.HasValue 
  ? list.OrderBy(o => o.TaksitNo ?? 0)
      : list.OrderBy(o => o.Ogrenci != null ? o.Ogrenci.OgrenciSoyadi : "")
    .ThenBy(o => o.Ogrenci != null ? o.Ogrenci.OgrenciAdi : "")
     .ThenBy(o => o.TaksitNo ?? 0);

     foreach (var x in sortedList)
  {
        if (!ogrenciId.HasValue)
      table.Cell().Text(x.Ogrenci != null ? ($"{x.Ogrenci.OgrenciAdi} {x.Ogrenci.OgrenciSoyadi}") : "").FontSize(9);
table.Cell().Text(x.TaksitNo.HasValue ? $"T{x.TaksitNo}" : "-").FontSize(9);
       table.Cell().Text(x.TaksitTutari.HasValue ? x.TaksitTutari.Value.ToString("N2") + " ₺" : "-").FontSize(9);
       table.Cell().Text(x.SonOdemeTarihi?.ToString("dd.MM.yyyy") ?? "-").FontSize(9);
table.Cell().Text(x.OdemeTarihi?.ToString("dd.MM.yyyy") ?? "-").FontSize(9);
    table.Cell().Text(x.OdenenTutar.ToString("N2") + " ₺").FontSize(9);
   table.Cell().Text(x.BorcTutari.ToString("N2") + " ₺").FontSize(9);
  table.Cell().Text(x.Odendi ? "✓ Ödendi" : "⏱ Bekliyor")
   .FontSize(9)
     .FontColor(x.Odendi ? QuestPDF.Helpers.Colors.Green.Darken2 : QuestPDF.Helpers.Colors.Orange.Darken2);
       table.Cell().Text(x.Aciklama ?? "").FontSize(8);
   }
      });
      page.Footer().AlignRight().Text($"Oluşturma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8);
 });
   });

            using var ms = new MemoryStream();
doc.GeneratePdf(ms);
       ms.Position = 0;
    var fileName = (ogrenciId.HasValue ? $"odeme_{ogrenciId.Value}_" : "odemeler_") + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf";
return File(ms.ToArray(), "application/pdf", fileName);
        }
    }
}
