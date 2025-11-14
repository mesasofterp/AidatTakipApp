using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Services;
using StudentApp.Jobs;
using Quartz;
using AppSchedulerFactory = StudentApp.Services.IZamanlayiciFactory;
using Microsoft.AspNetCore.Identity;
using StudentApp.Helpers;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Windows Service desteği ekle
    builder.Host.UseWindowsService(options =>
    {
        options.ServiceName = "AidatTakipApp";
    });

    // Logging yapılandırması - Event Log ve Console
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddEventLog(settings =>
    {
        settings.SourceName = "AidatTakipApp";
        settings.LogName = "Application";
    });
    builder.Logging.SetMinimumLevel(LogLevel.Information);

    string keyPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "public.key");
    string licPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "license.lic");

    // Logger'ı erken oluştur (Windows Service için gerekli)
    var loggerFactory = LoggerFactory.Create(logging => 
    {
        logging.AddConsole();
        logging.AddEventLog(settings =>
        {
            settings.SourceName = "AidatTakipApp";
            settings.LogName = "Application";
        });
        logging.SetMinimumLevel(LogLevel.Information);
    });
    var earlyLogger = loggerFactory.CreateLogger<Program>();

    earlyLogger.LogInformation("Uygulama başlatılıyor... ContentRootPath: {Path}", builder.Environment.ContentRootPath);
    earlyLogger.LogInformation("Lisans dosyası kontrol ediliyor: {LicPath}", licPath);
    earlyLogger.LogInformation("Public key kontrol ediliyor: {KeyPath}", keyPath);

    if (!LicenseManager.CheckLicense(keyPath, licPath, out string failReason))
    {
        earlyLogger.LogError("Lisans geçersiz: {FailReason}", failReason);
        // Windows Service olarak çalışıyorsa Console yok, EventLog kullan
        if (Environment.UserInteractive)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Lisans geçersiz: " + failReason);
            Console.ResetColor();
        }
        // Windows Service için 30 saniye bekle ki hata loglanabilsin
        if (!Environment.UserInteractive)
        {
            Thread.Sleep(30000);
        }
        return; // Uygulamayı başlatma
    }

    earlyLogger.LogInformation("Lisans doğrulandı.");

    // Tarih formatını ayarla - Türkiye lokalizasyonu
    var cultureInfo = new CultureInfo("tr-TR");
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

    //var app = builder.Build();
    //app.MapGet("/", () => "Lisans doğrulandı, uygulama çalışıyor.");
    // Add services to the container.
    builder.Services.AddControllersWithViews();

// Identity
builder.Services
    .AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<IOgrencilerService, OgrenciService>();
builder.Services.AddScoped<IOdemePlanlariService, OdemePlanlariService>();
builder.Services.AddScoped<ICinsiyetlerService, CinsiyetlerService>();
builder.Services.AddScoped<IOgrenciOdemeTakvimiService, OgrenciOdemeTakvimiService>();
builder.Services.AddScoped<IEnvanterlerService, EnvanterlerService>();

// Add Daily Task Service and SMS Service
builder.Services.AddScoped<IGunlukZamanlayiciService, GunlukZamanlayiciService>();
builder.Services.AddSingleton<ISmsService, SmsService>();
builder.Services.AddHttpClient();

// Add Quartz.NET
builder.Services.AddQuartz(q =>
{
    // Job key oluştur
    var jobKey = new JobKey("DailyJob");

    // Job'ı kaydet
    q.AddJob<DailyJob>(opts => opts.WithIdentity(jobKey));

    // Trigger oluştur - Her gün saat 09:00'da çalışacak
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailyJob-trigger")
        .WithCronSchedule("0 0 9 * * ?") // Her gün saat 09:00'da (cron: saniye dakika saat gün ay hafta)
    );

    // Durability ayarla - Uygulama kapanıp açılsa bile job'lar devam eder
    q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

    // Persistence kullan (opsiyonel - RAM'de tutulacaksa kullanmayabilirsiniz)
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
});

// Quartz hosted service ekle
builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true; // Uygulama kapanırken job'ların tamamlanmasını bekler
});

// Add Scheduler Factory - IScheduler'ı kullanmak için  
builder.Services.AddSingleton<AppSchedulerFactory>(sp =>
{
    var quartzSchedulerFactory = sp.GetRequiredService<Quartz.ISchedulerFactory>();
    var logger = sp.GetRequiredService<ILogger<StudentApp.Services.ZamanlayiciFactory>>();
    
    // IScheduler'ı Quartz'dan al - bu lazy loading için
    Task<IScheduler> schedulerTask = null;
    IScheduler scheduler = null;
    try
    {
        schedulerTask = quartzSchedulerFactory.GetScheduler();
        scheduler = schedulerTask.GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Zamanlayıcı alınırken hata oluştu");
        throw;
    }
    
    return new StudentApp.Services.ZamanlayiciFactory(scheduler, logger);
});

// Add Scheduler Service
builder.Services.AddScoped<IZamanlayiciService>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    var logger = sp.GetRequiredService<ILogger<StudentApp.Services.ZamanlayiciService>>();
    var schedulerFactory = sp.GetRequiredService<AppSchedulerFactory>();
    return new StudentApp.Services.ZamanlayiciService(context, logger, schedulerFactory);
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
  name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger2 = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
 {
        // Check if database exists and apply pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations();
     
        if (pendingMigrations.Any())
  {
            logger2.LogInformation("Bekleyen migration'lar uygulanıyor: {Migrations}", 
  string.Join(", ", pendingMigrations));
        context.Database.Migrate();
            logger2.LogInformation("Migration'lar başarıyla uygulandı.");
      }
  else
        {
            logger2.LogInformation("Bekleyen migration yok, database güncel.");
        }
      
  // Seed data oluştur
        await SeedData.InitializeAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger2.LogError(ex, "Migration veya seed data uygulanırken hata oluştu.");
    }
}

    // Port yapılandırması - IP + 2961 portu ile erişim için
    var port = builder.Configuration["Port"] ?? "2961";
    var urls = $"http://0.0.0.0:{port}";

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Uygulama başlatılıyor: {Urls}", urls);
    logger.LogInformation("Windows Service modu: {IsWindowsService}", Environment.UserInteractive == false);
    logger.LogInformation("Content Root Path: {Path}", builder.Environment.ContentRootPath);
    logger.LogInformation("Working Directory: {Path}", Directory.GetCurrentDirectory());

    try
    {
        app.Run(urls);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Uygulama çalışırken kritik hata oluştu!");
        throw;
    }
}
catch (Exception ex)
{
    // Tüm başlatma hatalarını yakala ve logla
    try
    {
        var loggerFactory = LoggerFactory.Create(logging => 
        {
            logging.AddConsole();
            logging.AddEventLog(settings =>
            {
                settings.SourceName = "AidatTakipApp";
                settings.LogName = "Application";
            });
            logging.SetMinimumLevel(LogLevel.Critical);
        });
        var errorLogger = loggerFactory.CreateLogger<Program>();
        errorLogger.LogCritical(ex, "Uygulama başlatılırken kritik hata oluştu! Hata: {Message}\nStack Trace: {StackTrace}", 
            ex.Message, ex.StackTrace);
    }
    catch
    {
        // Logger bile çalışmıyorsa, dosyaya yazmayı dene
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup-error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] CRITICAL ERROR: {ex.Message}\n{ex.StackTrace}\n\n");
        }
        catch { }
    }

    // Windows Service için biraz bekle ki hata loglanabilsin
    if (!Environment.UserInteractive)
    {
        Thread.Sleep(30000);
    }
    
    throw; // Hata kodunu döndür
}
