using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentApp.Models.ViewModels;
using StudentApp.Services;
using StudentApp.Attributes;

namespace StudentApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPagePermissionService _pagePermissionService;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IPagePermissionService pagePermissionService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _pagePermissionService = pagePermissionService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel { ReturnUrl = returnUrl };

            if (Request.Cookies.TryGetValue("RememberMeUser", out var userName))
            {
                model.UserName = userName;
                model.RememberMe = true;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Kullanıcının claim'lerini yükle (Identity otomatik yapar ama emin olmak için)
                await _signInManager.SignInAsync(user, model.RememberMe);

                if (model.RememberMe)
                {
                    Response.Cookies.Append("RememberMeUser", model.UserName, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        HttpOnly = true,
                        Secure = Request.IsHttps
                    });
                }
                else
                {
                    Response.Cookies.Delete("RememberMeUser");
                }

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlenmiştir.");
            else
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("RememberMeUser");
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [PageAuthorize("Account.Profile")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [PageAuthorize("Account.Profile")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [PageAuthorize("Account.ChangePassword")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [PageAuthorize("Account.ChangePassword")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [PageAuthorize("Account.Users")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return View(userViewModels);
        }

        [PageAuthorize("Account.CreateUser")]
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            ViewBag.AllPages = _pagePermissionService.GetAllPages();
            return View();
        }

        [PageAuthorize("Account.CreateUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = _roleManager.Roles.ToList();
                ViewBag.Roles = new SelectList(roles, "Name", "Name");
                ViewBag.AllPages = _pagePermissionService.GetAllPages();
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = model.EmailConfirmed
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Rolleri ekle
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    // Geçerli rolleri filtrele (null, boş veya "on" değerlerini çıkar)
                    var validRoles = model.SelectedRoles
                        .Where(r => !string.IsNullOrWhiteSpace(r) && r.ToLower() != "on")
                        .ToList();
                    
                    if (validRoles.Any())
                    {
                        // Rollerin var olup olmadığını kontrol et
                        var existingRoles = _roleManager.Roles
                            .Where(r => validRoles.Contains(r.Name))
                            .Select(r => r.Name)
                            .ToList();
                        
                        if (existingRoles.Any())
                        {
                            await _userManager.AddToRolesAsync(user, existingRoles);
                        }
                    }
                }

                // Admin değilse sayfa izinlerini ekle
                var isAdmin = model.SelectedRoles != null && model.SelectedRoles.Contains("Admin");
                if (!isAdmin && model.SelectedPages != null && model.SelectedPages.Any())
                {
                    var validPages = model.SelectedPages
                        .Where(p => !string.IsNullOrWhiteSpace(p) && p.ToLower() != "on")
                        .ToList();
                    
                    if (validPages.Any())
                    {
                        await _pagePermissionService.AddPageClaimsToUserAsync(user.Id, validPages);
                    }
                }

                TempData["SuccessMessage"] = $"Kullanıcı '{model.UserName}' başarıyla oluşturuldu.";
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var rolesList = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(rolesList, "Name", "Name");
            ViewBag.AllPages = _pagePermissionService.GetAllPages();
            return View(model);
        }

        [PageAuthorize("Account.EditUser")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Users");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            var userPageClaims = _pagePermissionService.GetUserPageClaims(user.Id);

            var model = new CreateUserViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                SelectedRoles = userRoles.ToList(),
                SelectedPages = userPageClaims,
                EmailConfirmed = user.EmailConfirmed
            };

            ViewBag.Roles = new SelectList(allRoles, "Name", "Name");
            ViewBag.AllPages = _pagePermissionService.GetAllPages();
            ViewBag.UserId = user.Id;
            return View(model);
        }

        [PageAuthorize("Account.EditUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, CreateUserViewModel model)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Users");
            }

            // EditUser'da Password ve ConfirmPassword alanları yok, bu yüzden validation hatalarını temizle
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                var allRoles = _roleManager.Roles.ToList();
                ViewBag.Roles = new SelectList(allRoles, "Name", "Name");
                ViewBag.AllPages = _pagePermissionService.GetAllPages();
                ViewBag.UserId = user.Id;
                return View(model);
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Rolleri güncelle
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    // Geçerli rolleri filtrele (null, boş veya "on" değerlerini çıkar)
                    var validRoles = model.SelectedRoles
                        .Where(r => !string.IsNullOrWhiteSpace(r) && r.ToLower() != "on")
                        .ToList();
                    
                    if (validRoles.Any())
                    {
                        // Rollerin var olup olmadığını kontrol et
                        var existingRoles = _roleManager.Roles
                            .Where(r => validRoles.Contains(r.Name))
                            .Select(r => r.Name)
                            .ToList();
                        
                        if (existingRoles.Any())
                        {
                            await _userManager.AddToRolesAsync(user, existingRoles);
                        }
                    }
                }

                // Admin kontrolü - güncellenmiş rollerden kontrol et
                var updatedRoles = model.SelectedRoles != null 
                    ? model.SelectedRoles
                        .Where(r => !string.IsNullOrWhiteSpace(r) && r.ToLower() != "on")
                        .ToList()
                    : new List<string>();
                
                var isAdmin = updatedRoles.Contains("Admin");
                
                if (isAdmin)
                {
                    // Admin ise tüm sayfa claim'lerini temizle
                    await _pagePermissionService.UpdatePageClaimsForUserAsync(user.Id, new List<string>());
                }
                else
                {
                    // Admin değilse sayfa izinlerini güncelle
                    // Form'dan SelectedPages'i al - model binding'den veya Request.Form'dan
                    List<string> selectedPages = new List<string>();
                    
                    // Önce model binding'den dene
                    if (model.SelectedPages != null && model.SelectedPages.Any())
                    {
                        selectedPages = model.SelectedPages.ToList();
                    }
                    // Eğer model'de yoksa Request.Form'dan al
                    else if (Request.Form.ContainsKey("SelectedPages"))
                    {
                        var formValues = Request.Form["SelectedPages"];
                        if (formValues.Count > 0)
                        {
                            selectedPages = formValues.ToList();
                        }
                    }
                    
                    // Geçerli sayfaları filtrele
                    var validPages = selectedPages
                        .Where(p => !string.IsNullOrWhiteSpace(p) && p.ToLower() != "on")
                        .Distinct()
                        .ToList();
                    
                    // Her zaman güncelle (boş liste de olsa, claim'leri temizlemek için)
                    await _pagePermissionService.UpdatePageClaimsForUserAsync(user.Id, validPages);
                }

                TempData["SuccessMessage"] = $"Kullanıcı '{model.UserName}' başarıyla güncellendi.";
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var rolesList = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(rolesList, "Name", "Name");
            ViewBag.AllPages = _pagePermissionService.GetAllPages();
            ViewBag.UserId = user.Id;
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Users");
            }

            // Kendi hesabını silmesini engelle
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser?.Id)
            {
                TempData["ErrorMessage"] = "Kendi hesabınızı silemezsiniz.";
                return RedirectToAction("Users");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Kullanıcı '{user.UserName}' başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
            }

            return RedirectToAction("Users");
        }

        [PageAuthorize("Account.Users")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Şifre başarıyla sıfırlandı." });
            }

            return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
    }
}
 