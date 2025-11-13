using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers;

[Authorize]
public class ZamanlayiciController : Controller
{
    private readonly IZamanlayiciService _schedulerService;
    private readonly ILogger<ZamanlayiciController> _logger;

    public ZamanlayiciController(IZamanlayiciService schedulerService, ILogger<ZamanlayiciController> logger)
    {
        _schedulerService = schedulerService;
        _logger = logger;
    }

    // GET: Scheduler
    public async Task<IActionResult> Index()
    {
        var activeSchedulers = await _schedulerService.GetActiveSchedulersAsync();
        return View(activeSchedulers.ToList());
    }

    // GET: Scheduler/Create
    public IActionResult Create()
    {
        var model = new ZamanlayiciAyarlar
        {
            Saat = 9,
            Dakika = 0,
            CronIfadesi = "0 0 9 * * ?",
            Isim = "SMS HATIRLATICI",
            GorevCalismaGunuOfseti = 0
        };
        return View(model);
    }

    // POST: Scheduler/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ZamanlayiciAyarlar model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Cron expression oluştur
                model.CronIfadesi = await _schedulerService.GenerateCronExpressionAsync(
                    model.Saat, 
                    model.Dakika, 
                    true // Her gün
                );

                // Yeni scheduler oluştur
                await _schedulerService.CreateSchedulerAsync(model);

                _logger.LogInformation("Yeni Zamanlayıcı oluşturuldu: {Name} - {Hour}:{Minute}, Gün Ofseti: {Offset}", 
                    model.Isim, model.Saat, model.Dakika, model.GorevCalismaGunuOfseti);

                TempData["SuccessMessage"] = $"Zamanlayıcı '{model.Isim}' başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zamanlayıcı oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Zamanlayıcı oluşturulurken bir hata oluştu: " + ex.Message);
            }
        }

        return View(model);
    }

    // GET: Scheduler/Edit/5
    public async Task<IActionResult> Edit(long id)
    {
        var scheduler = await _schedulerService.GetSchedulerByIdAsync(id);
        if (scheduler == null)
        {
            TempData["ErrorMessage"] = "Zamanlayıcı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(scheduler);
    }

    // POST: Scheduler/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ZamanlayiciAyarlar model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Geçersiz zamanlayıcı ID'si.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Cron expression oluştur
                model.CronIfadesi = await _schedulerService.GenerateCronExpressionAsync(
                    model.Saat, 
                    model.Dakika, 
                    true // Her gün
                );

                // Scheduler'ı güncelle
                await _schedulerService.UpdateSchedulerAsync(model);

                _logger.LogInformation("Zamanlayıcı güncellendi: {Name} - {Hour}:{Minute}, Gün Ofseti: {Offset}", 
                    model.Isim, model.Saat, model.Dakika, model.GorevCalismaGunuOfseti);

                TempData["SuccessMessage"] = $"Zamanlayıcı '{model.Isim}' başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zamanlayıcı güncellenirken hata oluştu");
                ModelState.AddModelError("", "Zamanlayıcı güncellenirken bir hata oluştu: " + ex.Message);
            }
        }

        return View(model);
    }

    // POST: Scheduler/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var scheduler = await _schedulerService.GetSchedulerByIdAsync(id);
            if (scheduler == null)
            {
                TempData["ErrorMessage"] = "Zamanlayıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var deleted = await _schedulerService.DeleteSchedulerAsync(id);
            
            if (deleted)
            {
                TempData["SuccessMessage"] = $"Zamanlayıcı '{scheduler.Isim}' başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Zamanlayıcı silinemedi.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Zamanlayıcı silinirken hata oluştu");
            TempData["ErrorMessage"] = "Zamanlayıcı silinirken bir hata oluştu: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}

