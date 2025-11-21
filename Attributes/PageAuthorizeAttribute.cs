using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace StudentApp.Attributes
{
    public class PageAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string? _requiredPage;

        public PageAuthorizeAttribute(string? requiredPage = null)
        {
            _requiredPage = requiredPage;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Kullanıcı giriş yapmamışsa
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Admin ise tüm sayfalara erişebilir
            if (user.IsInRole("Admin"))
            {
                return;
            }

            // Eğer sayfa belirtilmemişse, controller ve action'dan oluştur
            var requiredPageClaim = _requiredPage;
            if (string.IsNullOrWhiteSpace(requiredPageClaim))
            {
                var controller = context.RouteData.Values["controller"]?.ToString();
                var action = context.RouteData.Values["action"]?.ToString();
                if (!string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action))
                {
                    requiredPageClaim = $"{controller}.{action}";
                }
            }

            // Sayfa belirtilmemişse erişim ver
            if (string.IsNullOrWhiteSpace(requiredPageClaim))
            {
                return;
            }

            // Kullanıcının bu sayfaya erişim yetkisi var mı kontrol et
            var hasPageAccess = user.HasClaim("Page", requiredPageClaim);
            if (!hasPageAccess)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
        }
    }
}

