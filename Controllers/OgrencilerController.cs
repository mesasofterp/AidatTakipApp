using Microsoft.AspNetCore.Authorization;
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
using StudentApp.Attributes;

namespace StudentApp.Controllers
{
    [Authorize]
    public class OgrencilerController : Controller
    {
        private readonly IOgrencilerService _ogrenciService;
        private readonly IOdemePlanlariService _odemePlanlariService;
        private readonly ICinsiyetlerService _cinsiyetlerService;
        private readonly ISmsService _smsService;
        private readonly StudentApp.Data.AppDbContext _context;
        private readonly IZamanlayiciService _schedulerService;
        private readonly IEnvanterlerService _envanterlerService;

        public OgrencilerController(
            IOgrencilerService ogrencilerService,
            IOdemePlanlariService odemePlanlariService,
            ICinsiyetlerService cinsiyetlerService,
            ISmsService smsService,
            StudentApp.Data.AppDbContext context,
            IZamanlayiciService schedulerService,
            IEnvanterlerService envanterlerService)
        {
            _ogrenciService = ogrencilerService;
            _odemePlanlariService = odemePlanlariService;
            _cinsiyetlerService = cinsiyetlerService;
            _smsService = smsService;
            _context = context;
            _schedulerService = schedulerService;
            _envanterlerService = envanterlerService;
        }

        // GET: Student
        [PageAuthorize("Ogrenciler.Index")]
        public async Task<IActionResult> Index(OgrencilerFilterViewModel filter)
        {
            IEnumerable<Ogrenciler> ogrenciler;

            // Eğer hiçbir filtre uygulanmadıysa boş liste göster
            bool hicbirFiltre = string.IsNullOrWhiteSpace(filter.SearchTerm) &&
          !filter.CinsiyetId.HasValue &&
           !filter.OdemePlanlariId.HasValue &&
              !filter.MinYas.HasValue &&
                 !filter.MaxYas.HasValue &&
              !filter.BaslangicKayitTarihi.HasValue &&
                     !filter.BitisKayitTarihi.HasValue &&
           !filter.ShowList;


                ogrenciler = await _ogrenciService.GetAllOgrenciAsync(filter.ShowPasif);

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

                // Kayıt Tarihi filtreleme
                if (filter.BaslangicKayitTarihi.HasValue || filter.BitisKayitTarihi.HasValue)
                {
                    ogrenciler = ogrenciler.Where(o =>
             {
                 var baslangicOk = !filter.BaslangicKayitTarihi.HasValue || o.KayitTarihi.Date >= filter.BaslangicKayitTarihi.Value.Date;
                 var bitisOk = !filter.BitisKayitTarihi.HasValue || o.KayitTarihi.Date <= filter.BitisKayitTarihi.Value.Date;
                 return baslangicOk && bitisOk;
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
                BaslangicKayitTarihi = filter.BaslangicKayitTarihi,
                BitisKayitTarihi = filter.BitisKayitTarihi,
                ShowPasif = filter.ShowPasif,
                ShowList = filter.ShowList,
                SortBy = filter.SortBy ?? "OgrenciSoyadi",
                SortOrder = filter.SortOrder ?? "asc",
                Cinsiyetler = await _cinsiyetlerService.GetAllCinsiyetlerAsync(),
                OdemePlanlari = await _odemePlanlariService.GetAllOdemePlanlariAsync()
            };

            return View(viewModel);
        }

        // GET: Student/Create
        [PageAuthorize("Ogrenciler.Create")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            // Envanter fiyatlarını ViewBag'e ekle
            var envanterler = await _envanterlerService.GetActiveAsync();
            var envanterFiyatlari = envanterler.ToDictionary(e => e.Id, e => e.SatisFiyat);
            ViewBag.EnvanterFiyatlari = envanterFiyatlari;

            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PageAuthorize("Ogrenciler.Create")]
        public async Task<IActionResult> Create(Ogrenciler ogrenci, OgrenciDetay? ogrenciDetay, List<EnvanterSatisViewModel>? EnvanterSatislari)
        {
            // Model State'deki tarih hatalarını temizle (culture sorunu için)
            if (ModelState.ContainsKey(nameof(ogrenci.IlkTaksitSonOdemeTarihi)))
            {
                var ilkTaksitError = ModelState[nameof(ogrenci.IlkTaksitSonOdemeTarihi)];
                if (ilkTaksitError?.Errors.Any() == true)
                {
                    // Eğer sadece format hatası varsa temizle
                    if (ilkTaksitError.Errors.All(e => e.ErrorMessage.Contains("not valid") || e.ErrorMessage.Contains("geçerli değil")))
                    {
                        ModelState.Remove(nameof(ogrenci.IlkTaksitSonOdemeTarihi));
                    }
                }
            }

            // Kilo için InvariantCulture ile parse et
            if (Request.Form.ContainsKey("Kilo"))
            {
                var kiloValue = Request.Form["Kilo"].ToString();
                if (!string.IsNullOrWhiteSpace(kiloValue))
                {
                    if (decimal.TryParse(kiloValue, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal parsedKilo))
                    {
                        ogrenci.Kilo = parsedKilo;
                    }
                }
                else
                {
                    ogrenci.Kilo = null;
                }
            }

            // EnvanterSatislari için OdenenTutar ve KalanTutar değerlerini Request.Form'dan al ve InvariantCulture ile parse et
            if (EnvanterSatislari != null && EnvanterSatislari.Any())
            {
                for (int i = 0; i < EnvanterSatislari.Count; i++)
                {
                    // OdenenTutar
                    var odenenFormKey = $"EnvanterSatislari[{i}].OdenenTutar";
                    if (Request.Form.ContainsKey(odenenFormKey))
                    {
                        var formValue = Request.Form[odenenFormKey].ToString();
                        if (decimal.TryParse(formValue, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal parsedValue))
                        {
                            EnvanterSatislari[i].OdenenTutar = parsedValue;
                        }
                    }

                    // KalanTutar
                    var kalanFormKey = $"EnvanterSatislari[{i}].KalanTutar";
                    if (Request.Form.ContainsKey(kalanFormKey))
                    {
                        var formValue = Request.Form[kalanFormKey].ToString();
                        if (decimal.TryParse(formValue, System.Globalization.NumberStyles.Number,
                             System.Globalization.CultureInfo.InvariantCulture, out decimal parsedKalan))
                        {
                            EnvanterSatislari[i].KalanTutar = parsedKalan;
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // İlk Taksit Son Ödeme Tarihi girilmemişse, Kayıt Tarihi ile doldur
                    if (!ogrenci.IlkTaksitSonOdemeTarihi.HasValue)
                    {
                        ogrenci.IlkTaksitSonOdemeTarihi = ogrenci.KayitTarihi;
                    }

                    await _ogrenciService.AddOgrenciAsync(ogrenci);

                    // OgrenciDetay kaydı oluştur (varsa)
                    if (ogrenciDetay != null && (ogrenciDetay.VeliAdSoyad != null || ogrenciDetay.VeliTelefonNumarasi != null ||
                        ogrenciDetay.OkulAdi != null || ogrenciDetay.OkulAdresi != null || ogrenciDetay.Sinif != null ||
                        ogrenciDetay.OkulHocasiAdSoyad != null || ogrenciDetay.OkulHocasiTelefon != null ||
         ogrenciDetay.OkulGirisSaati.HasValue || ogrenciDetay.OkulCikisSaati.HasValue))
                    {
                        ogrenciDetay.OgrenciId = ogrenci.Id;
                        ogrenciDetay.Aktif = true;
                        ogrenciDetay.IsDeleted = false;
                        ogrenciDetay.Version = 0;
                        await _context.OgrenciDetay.AddAsync(ogrenciDetay);
                        await _context.SaveChangesAsync();
                    }

                    // Envanter satışları varsa işle
                    if (EnvanterSatislari != null && EnvanterSatislari.Any())
                    {
                        foreach (var satisViewModel in EnvanterSatislari)
                        {
                            // Boş kayıtları atla
                            if (satisViewModel.EnvanterId <= 0)
                                continue;

                            var satisAdedi = satisViewModel.SatisAdet > 0 ? satisViewModel.SatisAdet : 1;

                            // Envanter stok kontrolü
                            var envanter = await _context.Envanterler.FindAsync(satisViewModel.EnvanterId);
                            if (envanter == null)
                            {
                                ModelState.AddModelError("", $"Seçilen envanter (ID: {satisViewModel.EnvanterId}) bulunamadı.");
                                continue;
                            }

                            if (envanter.Adet < satisAdedi)
                            {
                                ModelState.AddModelError("", $"{envanter.EnvanterAdi}: Yetersiz stok! Mevcut: {envanter.Adet}, İstenen: {satisAdedi}");
                                continue;
                            }

                            // Envanter satış kaydı oluştur
                            var envanterSatis = new OgrenciEnvanterSatis
                            {
                                OgrenciId = ogrenci.Id,
                                EnvanterId = satisViewModel.EnvanterId,
                                SatisTarihi = satisViewModel.SatisTarihi ?? DateTime.Now,
                                OdenenTutar = satisViewModel.OdenenTutar,
                                KalanTutar = satisViewModel.KalanTutar,
                                KalanTutarTahsilTarihi = satisViewModel.KalanTutarTahsilTarihi,
                                SatisAdet = satisAdedi,
                                Aciklama = satisViewModel.Aciklama,
                                Aktif = true,
                                IsDeleted = false
                            };

                            await _context.OgrenciEnvanterSatis.AddAsync(envanterSatis);

                            // Envanter stoğunu güncelle
                            envanter.Adet -= satisAdedi;
                            _context.Envanterler.Update(envanter);
                        }

                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Öğrenci başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Inner exception'ı da kontrol et
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    var fullMessage = $"Öğrenci eklenirken bir hata oluştu: {innerMessage}";

                    // Daha detaylı log
                    Console.WriteLine($"Hata: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                    ModelState.AddModelError("", fullMessage);
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
        [PageAuthorize("Ogrenciler.Details")]
        public async Task<IActionResult> Details(long id)
        {
            var ogrenci = await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Include(s => s.OgrenciDetay)
                .Where(s => !s.IsDeleted && s.Id == id)
                .FirstOrDefaultAsync();

            if (ogrenci == null)
            {
                return NotFound();
            }

            // Öğrencinin başarılarını getir
            var basarilar = await _context.OgrenciBasarilari
                .Where(b => b.OgrenciId == id && !b.IsDeleted)
                .OrderByDescending(b => b.Tarih)
                .ThenByDescending(b => b.Id)
                .ToListAsync();

            ViewBag.Basarilar = basarilar;

            // Öğrencinin envanter satışlarını getir
            var envanterSatislari = await _context.OgrenciEnvanterSatis
                .Include(e => e.Envanter)
                .Where(e => e.OgrenciId == id && !e.IsDeleted)
                .OrderByDescending(e => e.SatisTarihi)
                .ToListAsync();

            ViewBag.EnvanterSatislari = envanterSatislari;
            ViewBag.ToplamEnvanterHarcama = envanterSatislari.Sum(e => e.OdenenTutar);

            return View(ogrenci);
        }

        // GET: Student/Edit/5
        [PageAuthorize("Ogrenciler.Edit")]
        public async Task<IActionResult> Edit(long id)
        {
            var ogrenci = await _context.Ogrenciler
                .Include(s => s.Cinsiyet)
                .Include(s => s.OdemePlanlari)
                .Include(s => s.OgrenciDetay)
                .Where(s => !s.IsDeleted && s.Id == id)
                .FirstOrDefaultAsync();

            if (ogrenci == null)
            {
                return NotFound();
            }

            // Mevcut envanter satışlarını yükle
            var mevcutSatislar = await _context.OgrenciEnvanterSatis
    .Include(e => e.Envanter)
            .Where(e => e.OgrenciId == id && !e.IsDeleted && e.Aktif)
    .OrderByDescending(e => e.SatisTarihi)
           .Select(e => new EnvanterSatisViewModel
           {
               Id = e.Id,
               EnvanterId = e.EnvanterId,
               EnvanterAdi = e.Envanter.EnvanterAdi,
               SatisTarihi = e.SatisTarihi,
               SatisAdet = e.SatisAdet,
               OdenenTutar = e.OdenenTutar,
               KalanTutar = e.KalanTutar,
               KalanTutarTahsilTarihi = e.KalanTutarTahsilTarihi,
               Aciklama = e.Aciklama
           })
                .ToListAsync();

            ViewBag.MevcutEnvanterSatislari = mevcutSatislar;

            // Envanter fiyatlarını ViewBag'e ekle
            var envanterler = await _envanterlerService.GetActiveAsync();
            var envanterFiyatlari = envanterler.ToDictionary(e => e.Id, e => e.SatisFiyat);
            ViewBag.EnvanterFiyatlari = envanterFiyatlari;

            await LoadDropdownsAsync();
            return View(ogrenci);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Ogrenciler ogrenci, OgrenciDetay? ogrenciDetay, List<EnvanterSatisViewModel>? EnvanterSatislari)
        {
            if (id != ogrenci.Id)
            {
                return NotFound();
            }

            // İlk Taksit Son Ödeme Tarihini koru (değiştirilemez)
            var mevcutOgrenci = await _ogrenciService.GetOgrenciByIdAsync(id);
            if (mevcutOgrenci != null)
            {
                ogrenci.IlkTaksitSonOdemeTarihi = mevcutOgrenci.IlkTaksitSonOdemeTarihi;
            }

            // Model State'deki IlkTaksitSonOdemeTarihi hatalarını temizle (form'da yok artık)
            if (ModelState.ContainsKey(nameof(ogrenci.IlkTaksitSonOdemeTarihi)))
            {
                ModelState.Remove(nameof(ogrenci.IlkTaksitSonOdemeTarihi));
            }

            // Kilo için InvariantCulture ile parse et
            if (Request.Form.ContainsKey("Kilo"))
            {
                var kiloValue = Request.Form["Kilo"].ToString();
                if (!string.IsNullOrWhiteSpace(kiloValue))
                {
                    if (decimal.TryParse(kiloValue, System.Globalization.NumberStyles.Number,
                     System.Globalization.CultureInfo.InvariantCulture, out decimal parsedKilo))
                    {
                        ogrenci.Kilo = parsedKilo;
                    }
                }
                else
                {
                    ogrenci.Kilo = null;
                }
            }

            // EnvanterSatislari için OdenenTutar ve KalanTutar değerlerini Request.Form'dan al ve InvariantCulture ile parse et
            if (EnvanterSatislari != null && EnvanterSatislari.Any())
            {
                for (int i = 0; i < EnvanterSatislari.Count; i++)
                {
                    var formKey = $"EnvanterSatislari[{i}].OdenenTutar";
                    if (Request.Form.ContainsKey(formKey))
                    {
                        var formValue = Request.Form[formKey].ToString();
                        if (decimal.TryParse(formValue, System.Globalization.NumberStyles.Number,
      System.Globalization.CultureInfo.InvariantCulture, out decimal parsedValue))
                        {
                            EnvanterSatislari[i].OdenenTutar = parsedValue;
                        }
                    }

                    formKey = $"EnvanterSatislari[{i}].KalanTutar";
                    if (Request.Form.ContainsKey(formKey))
                    {
                        var formValue = Request.Form[formKey].ToString();
                        if (decimal.TryParse(formValue, System.Globalization.NumberStyles.Number,
      System.Globalization.CultureInfo.InvariantCulture, out decimal parsedKalan))
                        {
                            EnvanterSatislari[i].KalanTutar = parsedKalan;
                        }
                    }
                }
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

                    // OgrenciDetay kaydını güncelle veya oluştur
                    // Form'dan gelen verileri oku (önce model binding, sonra Request.Form)
                    string? GetFormValue(string key, string? modelValue)
                    {
                        // Önce model binding'den gelen değeri kontrol et
                        if (!string.IsNullOrWhiteSpace(modelValue))
                            return modelValue;

                        // Model binding'den değer yoksa Request.Form'dan oku
                        var formValue = Request.Form[$"OgrenciDetay.{key}"].ToString();
                        return !string.IsNullOrWhiteSpace(formValue) ? formValue : null;
                    }

                    TimeSpan? GetTimeSpanValue(string key, TimeSpan? modelValue)
                    {
                        // Önce model binding'den gelen değeri kontrol et
                        if (modelValue.HasValue)
                            return modelValue;

                        // Model binding'den değer yoksa Request.Form'dan oku
                        var formValue = Request.Form[$"OgrenciDetay.{key}"].ToString();
                        if (!string.IsNullOrWhiteSpace(formValue) && TimeSpan.TryParse(formValue, out var timeValue))
                            return timeValue;

                        return null;
                    }

                    var veliAdSoyad = GetFormValue("VeliAdSoyad", ogrenciDetay?.VeliAdSoyad);
                    var veliTelefon = GetFormValue("VeliTelefonNumarasi", ogrenciDetay?.VeliTelefonNumarasi);
                    var okulAdi = GetFormValue("OkulAdi", ogrenciDetay?.OkulAdi);
                    var okulAdresi = GetFormValue("OkulAdresi", ogrenciDetay?.OkulAdresi);
                    var sinif = GetFormValue("Sinif", ogrenciDetay?.Sinif);
                    var okulHocasiAdSoyad = GetFormValue("OkulHocasiAdSoyad", ogrenciDetay?.OkulHocasiAdSoyad);
                    var okulHocasiTelefon = GetFormValue("OkulHocasiTelefon", ogrenciDetay?.OkulHocasiTelefon);
                    var okulGirisSaati = GetTimeSpanValue("OkulGirisSaati", ogrenciDetay?.OkulGirisSaati);
                    var okulCikisSaati = GetTimeSpanValue("OkulCikisSaati", ogrenciDetay?.OkulCikisSaati);

                    // En az bir alan dolu mu kontrol et
                    bool enAzBirAlanDolu = veliAdSoyad != null || veliTelefon != null || okulAdi != null ||
                            okulAdresi != null || sinif != null || okulHocasiAdSoyad != null || okulHocasiTelefon != null ||
                      okulGirisSaati.HasValue || okulCikisSaati.HasValue;

                    var mevcutDetay = await _context.OgrenciDetay
                  .FirstOrDefaultAsync(d => d.OgrenciId == ogrenci.Id && !d.IsDeleted);

                    if (mevcutDetay != null)
                    {
                        // Güncelle
                        mevcutDetay.VeliAdSoyad = veliAdSoyad;
                        mevcutDetay.VeliTelefonNumarasi = veliTelefon;
                        mevcutDetay.OkulAdi = okulAdi;
                        mevcutDetay.OkulAdresi = okulAdresi;
                        mevcutDetay.Sinif = sinif;
                        mevcutDetay.OkulHocasiAdSoyad = okulHocasiAdSoyad;
                        mevcutDetay.OkulHocasiTelefon = okulHocasiTelefon;
                        mevcutDetay.OkulGirisSaati = okulGirisSaati;
                        mevcutDetay.OkulCikisSaati = okulCikisSaati;
                        mevcutDetay.Version++;
                        _context.OgrenciDetay.Update(mevcutDetay);
                    }
                    else if (enAzBirAlanDolu)
                    {
                        // Yeni kayıt oluştur
                        var yeniDetay = new OgrenciDetay
                        {
                            OgrenciId = ogrenci.Id,
                            VeliAdSoyad = veliAdSoyad,
                            VeliTelefonNumarasi = veliTelefon,
                            OkulAdi = okulAdi,
                            OkulAdresi = okulAdresi,
                            Sinif = sinif,
                            OkulHocasiAdSoyad = okulHocasiAdSoyad,
                            OkulHocasiTelefon = okulHocasiTelefon,
                            OkulGirisSaati = okulGirisSaati,
                            OkulCikisSaati = okulCikisSaati,
                            Aktif = true,
                            IsDeleted = false,
                            Version = 0
                        };
                        await _context.OgrenciDetay.AddAsync(yeniDetay);
                    }

                    // Envanter satışları varsa işle
                    if (EnvanterSatislari != null && EnvanterSatislari.Any())
                    {
                        foreach (var satisViewModel in EnvanterSatislari)
                        {
                            // Silinecek kayıtları işle
                            if (satisViewModel.SilinecekMi && satisViewModel.Id > 0)
                            {
                                var silinecekSatis = await _context.OgrenciEnvanterSatis.FindAsync(satisViewModel.Id);
                                if (silinecekSatis != null)
                                {
                                    // Envanter stoğunu geri ekle
                                    var envanter = await _context.Envanterler.FindAsync(silinecekSatis.EnvanterId);
                                    if (envanter != null)
                                    {
                                        envanter.Adet += silinecekSatis.SatisAdet;
                                        _context.Envanterler.Update(envanter);
                                    }

                                    // Soft delete
                                    silinecekSatis.IsDeleted = true;
                                    silinecekSatis.Aktif = false;
                                    _context.OgrenciEnvanterSatis.Update(silinecekSatis);
                                }
                                continue;
                            }

                            // Güncelleme işlemi (Id > 0 ve değişiklik varsa)
                            if (satisViewModel.Id > 0)
                            {
                                var mevcutSatis = await _context.OgrenciEnvanterSatis.FindAsync(satisViewModel.Id);
                                if (mevcutSatis != null)
                                {
                                    // Adet değiştiyse stok güncelle
                                    if (mevcutSatis.SatisAdet != satisViewModel.SatisAdet)
                                    {
                                        var envanter = await _context.Envanterler.FindAsync(mevcutSatis.EnvanterId);
                                        if (envanter != null)
                                        {
                                            int fark = satisViewModel.SatisAdet - mevcutSatis.SatisAdet;
                                            if (envanter.Adet < fark)
                                            {
                                                ModelState.AddModelError("", $"{envanter.EnvanterAdi}: Yetersiz stok! Mevcut: {envanter.Adet}, Gerekli: {fark}");
                                                continue;
                                            }
                                            envanter.Adet -= fark;
                                            _context.Envanterler.Update(envanter);
                                        }
                                    }

                                    // Diğer alanları güncelle
                                    mevcutSatis.SatisTarihi = satisViewModel.SatisTarihi ?? DateTime.Now;
                                    mevcutSatis.OdenenTutar = satisViewModel.OdenenTutar;
                                    mevcutSatis.KalanTutar = satisViewModel.KalanTutar;
                                    mevcutSatis.KalanTutarTahsilTarihi = satisViewModel.KalanTutarTahsilTarihi;
                                    mevcutSatis.SatisAdet = satisViewModel.SatisAdet;
                                    mevcutSatis.Aciklama = satisViewModel.Aciklama;
                                    _context.OgrenciEnvanterSatis.Update(mevcutSatis);
                                }
                                continue;
                            }

                            // Yeni kayıt ekleme (Id == 0)
                            // Boş kayıtları atla
                            if (satisViewModel.EnvanterId <= 0)
                                continue;

                            var satisAdedi = satisViewModel.SatisAdet > 0 ? satisViewModel.SatisAdet : 1;

                            // Envanter stok kontrolü
                            var yeniEnvanter = await _context.Envanterler.FindAsync(satisViewModel.EnvanterId);
                            if (yeniEnvanter == null)
                            {
                                ModelState.AddModelError("", $"Seçilen envanter (ID: {satisViewModel.EnvanterId}) bulunamadı.");
                                continue;
                            }

                            if (yeniEnvanter.Adet < satisAdedi)
                            {
                                ModelState.AddModelError("", $"{yeniEnvanter.EnvanterAdi}: Yetersiz stok! Mevcut: {yeniEnvanter.Adet}, İstenen: {satisAdedi}");
                                continue;
                            }

                            // Envanter satış kaydı oluştur
                            var envanterSatis = new OgrenciEnvanterSatis
                            {
                                OgrenciId = ogrenci.Id,
                                EnvanterId = satisViewModel.EnvanterId,
                                SatisTarihi = satisViewModel.SatisTarihi ?? DateTime.Now,
                                OdenenTutar = satisViewModel.OdenenTutar,
                                KalanTutar = satisViewModel.KalanTutar,
                                KalanTutarTahsilTarihi = satisViewModel.KalanTutarTahsilTarihi,
                                SatisAdet = satisAdedi,
                                Aciklama = satisViewModel.Aciklama,
                                Aktif = true,
                                IsDeleted = false
                            };

                            await _context.OgrenciEnvanterSatis.AddAsync(envanterSatis);

                            // Envanter stoğunu güncelle
                            yeniEnvanter.Adet -= satisAdedi;
                            _context.Envanterler.Update(yeniEnvanter);
                        }

                        await _context.SaveChangesAsync();
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

            // Hata durumunda mevcut satışları tekrar yükle
            var mevcutSatislar = await _context.OgrenciEnvanterSatis
  .Include(e => e.Envanter)
                .Where(e => e.OgrenciId == id && !e.IsDeleted && e.Aktif)
        .OrderByDescending(e => e.SatisTarihi)
   .Select(e => new EnvanterSatisViewModel
   {
       Id = e.Id,
       EnvanterId = e.EnvanterId,
       EnvanterAdi = e.Envanter.EnvanterAdi,
       SatisTarihi = e.SatisTarihi,
       SatisAdet = e.SatisAdet,
       OdenenTutar = e.OdenenTutar,
       KalanTutar = e.KalanTutar,
       KalanTutarTahsilTarihi = e.KalanTutarTahsilTarihi,
       Aciklama = e.Aciklama
   })
     .ToListAsync();

            ViewBag.MevcutEnvanterSatislari = mevcutSatislar;

            // Envanter fiyatlarını yeniden yükle
            var envanterler = await _envanterlerService.GetActiveAsync();
            var envanterFiyatlari = envanterler.ToDictionary(e => e.Id, e => e.SatisFiyat);
            ViewBag.EnvanterFiyatlari = envanterFiyatlari;

            await LoadDropdownsAsync();
            return View(ogrenci);
        }

        // GET: Student/Delete/5
        [PageAuthorize("Ogrenciler.Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var envanter = await _ogrenciService.GetOgrenciByIdAsync(id.Value);

                if (envanter == null)
                {
                    return NotFound();
                }

                return View(envanter);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Öğrenci yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PageAuthorize("Ogrenciler.Delete")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var result = await _ogrenciService.DeleteOgrenciAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Öğrenci başarıyla silindi.";
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

            var envanterler = await _envanterlerService.GetActiveAsync();
            ViewBag.Envanterler = new SelectList(envanterler, "Id", "EnvanterAdi");
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
                var activeSchedulers = await _schedulerService.GetActiveSchedulersAsync();
                var activeSettings = activeSchedulers.FirstOrDefault();
                var template = activeSettings?.MesajSablonu ?? "Sayın [ÖĞRENCİ_ADI] [ÖĞRENCİ_SOYADI], ödemeniz [REFERANS_TARIH] tarihinden beri yapılmamıştır. Lütfen ödemenizi yapınız.";

                var students = await _context.Ogrenciler
           .Include(s => s.OdemePlanlari)
     .Where(s => selectedIds.Contains(s.Id))
          .Select(s => new { s.Id, s.OgrenciAdi, s.OgrenciSoyadi, s.Telefon, s.KayitTarihi, s.SonSmsTarihi, PlanTutar = (decimal?)s.OdemePlanlari.ToplamTutar })
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
                   .Include(s => s.Cinsiyet)
                      .Include(s => s.OdemePlanlari)
                      .Where(s => selectedIds.Contains(s.Id))
                 .Select(s => new
                 {
                     s.OgrenciAdi,
                     s.OgrenciSoyadi,
                     s.TCNO,
                     s.Telefon,
                     s.Email,
                     s.DogumTarihi,
                     CinsiyetAdi = s.Cinsiyet != null ? s.Cinsiyet.Cinsiyet : "",
                     s.Adres,
                     s.KayitTarihi,
                     OdemePlani = s.OdemePlanlari != null ? s.OdemePlanlari.KursProgrami : "",
                     ToplamTutar = s.OdemePlanlari != null ? s.OdemePlanlari.ToplamTutar : 0,
                     TaksitSayisi = s.OdemePlanlari != null ? s.OdemePlanlari.TaksitSayisi : 0,
                     s.SonSmsTarihi,
                     s.Aktif
                 })
                    .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ogrenciler");

            // Başlık satırı
            int c = 1;
            ws.Cell(1, c++).Value = "Ad";
            ws.Cell(1, c++).Value = "Soyad";
            ws.Cell(1, c++).Value = "TC Kimlik No";
            ws.Cell(1, c++).Value = "Telefon";
            ws.Cell(1, c++).Value = "Email";
            ws.Cell(1, c++).Value = "Doğum Tarihi";
            ws.Cell(1, c++).Value = "Yaş";
            ws.Cell(1, c++).Value = "Cinsiyet";
            ws.Cell(1, c++).Value = "Adres";
            ws.Cell(1, c++).Value = "Kayıt Tarihi";
            ws.Cell(1, c++).Value = "Ödeme Planı";
            ws.Cell(1, c++).Value = "Toplam Tutar";
            ws.Cell(1, c++).Value = "Taksit Sayısı";
            ws.Cell(1, c++).Value = "Son SMS Tarihi";
            ws.Cell(1, c++).Value = "Durum";

            // Başlık satırını stillendir
            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Veri satırları
            int r = 2;
            var today = DateTime.Today;
            foreach (var x in rows.OrderBy(o => o.OgrenciSoyadi).ThenBy(o => o.OgrenciAdi))
            {
                int col = 1;
                var yas = today.Year - x.DogumTarihi.Year;
                if (today.DayOfYear < x.DogumTarihi.DayOfYear) yas--;

                ws.Cell(r, col++).Value = x.OgrenciAdi;
                ws.Cell(r, col++).Value = x.OgrenciSoyadi;
                ws.Cell(r, col++).Value = x.TCNO ?? "-";
                ws.Cell(r, col++).Value = x.Telefon ?? "-";
                ws.Cell(r, col++).Value = x.Email;
                ws.Cell(r, col++).Value = x.DogumTarihi.ToString("dd.MM.yyyy");
                ws.Cell(r, col++).Value = yas;
                ws.Cell(r, col++).Value = x.CinsiyetAdi;
                ws.Cell(r, col++).Value = x.Adres ?? "-";
                ws.Cell(r, col++).Value = x.KayitTarihi.ToString("dd.MM.yyyy");
                ws.Cell(r, col++).Value = x.OdemePlani;
                ws.Cell(r, col++).Value = x.ToplamTutar;
                ws.Cell(r, col++).Value = x.TaksitSayisi;
                ws.Cell(r, col++).Value = x.SonSmsTarihi?.ToString("dd.MM.yyyy") ?? "-";
                ws.Cell(r, col++).Value = x.Aktif ? "Aktif" : "Pasif";

                // Pasif öğrencileri vurgula
                if (!x.Aktif)
                {
                    ws.Row(r).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                r++;
            }

            // Para formatı uygula (Toplam Tutar kolonu)
            ws.Column(12).Style.NumberFormat.Format = "#,##0.00 ₺";

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
         .Include(s => s.Cinsiyet)
          .Include(s => s.OdemePlanlari)
            .Where(s => selectedIds.Contains(s.Id))
           .Select(s => new
           {
               s.OgrenciAdi,
               s.OgrenciSoyadi,
               s.Telefon,
               s.Email,
               s.DogumTarihi,
               CinsiyetAdi = s.Cinsiyet != null ? s.Cinsiyet.Cinsiyet : "",
               s.KayitTarihi,
               OdemePlani = s.OdemePlanlari != null ? s.OdemePlanlari.KursProgrami : "",
               ToplamTutar = s.OdemePlanlari != null ? s.OdemePlanlari.ToplamTutar : 0,
               TaksitSayisi = s.OdemePlanlari != null ? s.OdemePlanlari.TaksitSayisi : 0,
               s.SonSmsTarihi,
               s.Aktif
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
                    columns.RelativeColumn(2); // Ad Soyad
                    columns.RelativeColumn(1.5f); // Telefon
                    columns.RelativeColumn(2.5f); // Email
                    columns.RelativeColumn(1.5f); // Doğum Tarihi
                    columns.RelativeColumn(1); // Yaş
                    columns.RelativeColumn(1); // Cinsiyet
                    columns.RelativeColumn(2); // Ödeme Planı
                    columns.RelativeColumn(1.5f); // ToplamTutar
                    columns.RelativeColumn(1); // Taksit Sayısı
                    columns.RelativeColumn(1); // Durum
                });

         table.Header(header =>
     {
         header.Cell().Element(CellStyle).Text("Ad Soyad").FontSize(9);
         header.Cell().Element(CellStyle).Text("Telefon").FontSize(9);
         header.Cell().Element(CellStyle).Text("Email").FontSize(9);
         header.Cell().Element(CellStyle).Text("Doğum").FontSize(9);
         header.Cell().Element(CellStyle).Text("Yaş").FontSize(9);
         header.Cell().Element(CellStyle).Text("Cinsiyet").FontSize(9);
         header.Cell().Element(CellStyle).Text("Ödeme Planı").FontSize(9);
         header.Cell().Element(CellStyle).Text("ToplamTutar").FontSize(9);
         header.Cell().Element(CellStyle).Text("Taksit Sayısı").FontSize(9);
         header.Cell().Element(CellStyle).Text("Durum").FontSize(9);

         static IContainer CellStyle(IContainer container)
         {
             return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5);
         }
     });

         var today = DateTime.Today;
         foreach (var x in rows.OrderBy(o => o.OgrenciSoyadi).ThenBy(o => o.OgrenciAdi))
         {
             var yas = today.Year - x.DogumTarihi.Year;
             if (today.DayOfYear < x.DogumTarihi.DayOfYear) yas--;

             table.Cell().Text($"{x.OgrenciAdi} {x.OgrenciSoyadi}").FontSize(8);
             table.Cell().Text(x.Telefon ?? "-").FontSize(8);
             table.Cell().Text(x.Email ?? "-").FontSize(7);
             table.Cell().Text(x.DogumTarihi.ToString("dd.MM.yyyy")).FontSize(8);
             table.Cell().Text(yas.ToString()).FontSize(8);
             table.Cell().Text(x.CinsiyetAdi).FontSize(8);
             table.Cell().Text(x.OdemePlani).FontSize(7);
             table.Cell().Text(x.ToplamTutar.ToString("N0") + " ₺").FontSize(8);
             table.Cell().Text(x.TaksitSayisi.ToString()).FontSize(8);
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
            return File(ms.ToArray(), "application/pdf", $"ogrenciler_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}
