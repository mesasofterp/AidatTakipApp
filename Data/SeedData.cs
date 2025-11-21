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

            // Veritabanýnýn oluþturulduðundan emin ol
            await context.Database.MigrateAsync();

            // Admin rolü oluþtur
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // User rolü oluþtur
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Admin kullanýcýsý oluþtur
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
                    Console.WriteLine("Admin kullanýcýsý baþarýyla oluþturuldu!");
                    Console.WriteLine($"Email: {adminEmail}");
                    Console.WriteLine("Þifre: 123456");
                }
                else
                {
                    Console.WriteLine("Admin kullanýcýsý oluþturulamadý:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Admin kullanýcýsý zaten mevcut.");

                // Admin rolüne sahip deðilse ekle
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Mevcut kullanýcýya Admin rolü eklendi.");
                }
            }

            // Cinsiyetler seed data
            if (!context.Cinsiyetler.Any())
            {
                context.Cinsiyetler.AddRange(
                new Cinsiyetler { Cinsiyet = "Erkek" },
                new Cinsiyetler { Cinsiyet = "Kadýn" }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("Cinsiyet verileri oluþturuldu.");
            }

            // Günler seed data
            if (!context.Gunler.Any())
            {
                context.Gunler.AddRange(
                new Gunler { Gun = "Pazartesi" },
                new Gunler { Gun = "Salý" },
                new Gunler { Gun = "Çarþamba" },
                new Gunler { Gun = "Perþembe" },
                new Gunler { Gun = "Cuma" },
                new Gunler { Gun = "Cumartesi" },
                new Gunler { Gun = "Pazar" }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("Gün verileri oluþturuldu.");
            }
        }
    }
}
