namespace StudentApp.Services
{
    public interface IPagePermissionService
    {
        List<PagePermission> GetAllPages();
        List<string> GetUserPageClaims(string userId);
        Task AddPageClaimsToUserAsync(string userId, List<string> pageClaimValues);
        Task UpdatePageClaimsForUserAsync(string userId, List<string> newPageClaimValues);
    }

    public class PagePermission
    {
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ClaimValue => $"{Controller}.{Action}";
    }
}

