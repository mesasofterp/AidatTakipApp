using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;

namespace StudentApp.Services
{
    public class PagePermissionService : IPagePermissionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PagePermissionService(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<PagePermission> GetAllPages()
        {
            // Hard-coded sayfa listesi
            return new List<PagePermission>
            {
                // Home
                new PagePermission { Controller = "Home", Action = "Index", DisplayName = "Ana Sayfa (Hızlı Menü)" },
                
                // Öğrenciler
                new PagePermission { Controller = "Ogrenciler", Action = "Index", DisplayName = "Öğrenciler - Liste" },
                new PagePermission { Controller = "Ogrenciler", Action = "Create", DisplayName = "Öğrenciler - Yeni Ekle" },
                new PagePermission { Controller = "Ogrenciler", Action = "Edit", DisplayName = "Öğrenciler - Düzenle" },
                new PagePermission { Controller = "Ogrenciler", Action = "Details", DisplayName = "Öğrenciler - Detay" },
                new PagePermission { Controller = "Ogrenciler", Action = "Delete", DisplayName = "Öğrenciler - Sil" },
                
                // Ödeme Planları
                new PagePermission { Controller = "OdemePlanlari", Action = "Index", DisplayName = "Ödeme Planları - Liste" },
                new PagePermission { Controller = "OdemePlanlari", Action = "Create", DisplayName = "Ödeme Planları - Yeni Ekle" },
                new PagePermission { Controller = "OdemePlanlari", Action = "Edit", DisplayName = "Ödeme Planları - Düzenle" },
                new PagePermission { Controller = "OdemePlanlari", Action = "Details", DisplayName = "Ödeme Planları - Detay" },
                new PagePermission { Controller = "OdemePlanlari", Action = "Delete", DisplayName = "Ödeme Planları - Sil" },
                
                // Öğrenci Ödeme Takvimi
                new PagePermission { Controller = "OgrenciOdemeTakvimi", Action = "Index", DisplayName = "Ödeme Takvimleri - Liste" },
                new PagePermission { Controller = "OgrenciOdemeTakvimi", Action = "Create", DisplayName = "Ödeme Takvimleri - Yeni Ekle" },
                new PagePermission { Controller = "OgrenciOdemeTakvimi", Action = "Edit", DisplayName = "Ödeme Takvimleri - Düzenle" },
                new PagePermission { Controller = "OgrenciOdemeTakvimi", Action = "Details", DisplayName = "Ödeme Takvimleri - Detay" },
                new PagePermission { Controller = "OgrenciOdemeTakvimi", Action = "Delete", DisplayName = "Ödeme Takvimleri - Sil" },
                
                // Envanterler
                new PagePermission { Controller = "Envanterler", Action = "Index", DisplayName = "Envanter - Liste" },
                new PagePermission { Controller = "Envanterler", Action = "Create", DisplayName = "Envanter - Yeni Ekle" },
                new PagePermission { Controller = "Envanterler", Action = "Edit", DisplayName = "Envanter - Düzenle" },
                new PagePermission { Controller = "Envanterler", Action = "Details", DisplayName = "Envanter - Detay" },
                new PagePermission { Controller = "Envanterler", Action = "Delete", DisplayName = "Envanter - Sil" },
                
                // Zamanlayıcı
                new PagePermission { Controller = "Zamanlayici", Action = "Index", DisplayName = "SMS Zamanlayıcı" },
                new PagePermission { Controller = "Zamanlayici", Action = "Create", DisplayName = "SMS Zamanlayıcı - Yeni Ekle" },
                new PagePermission { Controller = "Zamanlayici", Action = "Edit", DisplayName = "SMS Zamanlayıcı - Düzenle" },
                
                // Hesap Yönetimi
                new PagePermission { Controller = "Account", Action = "Profile", DisplayName = "Profil" },
                new PagePermission { Controller = "Account", Action = "ChangePassword", DisplayName = "Şifre Değiştir" },
                new PagePermission { Controller = "Account", Action = "Users", DisplayName = "Kullanıcı Yönetimi (Admin)" },
                new PagePermission { Controller = "Account", Action = "CreateUser", DisplayName = "Kullanıcı Ekle (Admin)" },
                new PagePermission { Controller = "Account", Action = "EditUser", DisplayName = "Kullanıcı Düzenle (Admin)" },
            };
        }

        public List<string> GetUserPageClaims(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return new List<string>();

            // AspNetUserClaims tablosundan "Page" tipindeki claim'leri al
            var claims = _context.UserClaims
                .Where(c => c.UserId == userId && c.ClaimType == "Page")
                .Select(c => c.ClaimValue ?? string.Empty)
                .ToList();

            return claims;
        }

        public async Task AddPageClaimsToUserAsync(string userId, List<string> pageClaimValues)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            // Mevcut page claim'lerini temizle
            await RemovePageClaimsFromUserAsync(userId);

            // Yeni claim'leri ekle
            foreach (var claimValue in pageClaimValues)
            {
                if (!string.IsNullOrWhiteSpace(claimValue))
                {
                    var claim = new System.Security.Claims.Claim("Page", claimValue);
                    await _userManager.AddClaimAsync(user, claim);
                }
            }
        }

        public async Task UpdatePageClaimsForUserAsync(string userId, List<string> newPageClaimValues)
        {
            await AddPageClaimsToUserAsync(userId, newPageClaimValues);
        }

        private async Task RemovePageClaimsFromUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var existingClaims = (await _userManager.GetClaimsAsync(user))
                .Where(c => c.Type == "Page")
                .ToList();

            foreach (var claim in existingClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }
        }
    }
}

