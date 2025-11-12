using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Services;
using StudentApp.Jobs;
using Quartz;
using AppSchedulerFactory = StudentApp.Services.IZamanlayiciFactory;
using Microsoft.AspNetCore.Identity;
using StudentApp.Helpers;

var builder = WebApplication.CreateBuilder(args);
string keyPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "public.key");
string licPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "license.lic");

if (!LicenseManager.CheckLicense(keyPath, licPath, out string failReason))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Lisans geçersiz: " + failReason);
    Console.ResetColor();
    return; // Uygulamayı başlatma
}


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
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
 {
        // Check if database exists and apply pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations();
     
        if (pendingMigrations.Any())
  {
          logger.LogInformation("Bekleyen migration'lar uygulanıyor: {Migrations}", 
  string.Join(", ", pendingMigrations));
        context.Database.Migrate();
  logger.LogInformation("Migration'lar başarıyla uygulandı.");
      }
  else
        {
      logger.LogInformation("Bekleyen migration yok, database güncel.");
        }
      
  // Seed data oluştur
        await SeedData.InitializeAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration veya seed data uygulanırken hata oluştu.");
    }
}

app.Run();
