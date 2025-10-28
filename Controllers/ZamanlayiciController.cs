using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers;

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
        var activeScheduler = await _schedulerService.GetActiveSchedulerAsync();
        return View(activeScheduler);
    }

    // GET: Scheduler/Create
    public IActionResult Create()
    {
        var model = new ZamanlayiciAyarlar
        {
            Saat = 9,
            Dakika = 0,
            CronIfadesi = "0 0 9 * * ?",
            Isim = "SMS HATIRLATICI"
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

                // Aktif scheduler varsa sil ve yenisi oluştur
                await _schedulerService.CreateOrUpdateSchedulerAsync(model);

                _logger.LogInformation("Yeni Zamanlayıcı oluşturuldu: {Name} - {Hour}:{Minute}", 
                    model.Isim, model.Saat, model.Dakika);

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

    // POST: Scheduler/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete()
    {
        try
        {
            var deleted = await _schedulerService.DeleteActiveSchedulerAsync();
            
            if (deleted)
            {
                TempData["SuccessMessage"] = "Zamanlayıcı başarıyla durduruldu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Aktif zamanlayıcı bulunamadı.";
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

