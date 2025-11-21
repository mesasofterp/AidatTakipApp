using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentApp.Models;

namespace StudentApp.Data
{
    public static class SeedData
    {
     public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
     using var scope = serviceProvider.CreateScope();
   var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
  var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Veritaban�n�n olu�turuldu�undan emin ol
            await context.Database.MigrateAsync();

        // Admin rol� olu�tur
    if (!await roleManager.RoleExistsAsync("Admin"))
      {
    await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // User rol� olu�tur
 if (!await roleManager.RoleExistsAsync("User"))
  {
 await roleManager.CreateAsync(new IdentityRole("User"));
            }

    // Admin kullan�c�s� olu�tur
            var adminEmail = "admin@aidattakip.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

         if (adminUser == null)
       {
        adminUser = new IdentityUser
                {
UserName = adminEmail,
      Email = adminEmail,
         EmailConfirmed = true
      };

    var result = await userManager.CreateAsync(adminUser, "123456");

   if (result.Succeeded)
       {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        Console.WriteLine("Admin kullan�c�s� ba�ar�yla olu�turuldu!");
           Console.WriteLine($"Email: {adminEmail}");
 Console.WriteLine("�ifre: 123456");
             }
    else
    {
                    Console.WriteLine("Admin kullan�c�s� olu�turulamad�:");
               foreach (var error in result.Errors)
         {
              Console.WriteLine($"- {error.Description}");
            }
    }
  }
       else
     {
                Console.WriteLine("Admin kullan�c�s� zaten mevcut.");
 
           // Admin rol�ne sahip de�ilse ekle
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
              {
           await userManager.AddToRoleAsync(adminUser, "Admin");
      Console.WriteLine("Mevcut kullan�c�ya Admin rol� eklendi.");
       }
 }

            // Cinsiyetler seed data
         if (!context.Cinsiyetler.Any())
         {
            context.Cinsiyetler.AddRange(
            new Cinsiyetler { Cinsiyet = "Erkek" },
            new Cinsiyetler { Cinsiyet = "Kad�n" }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("Cinsiyet verileri olu�turuldu.");
         }
        }
 }
}
