using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefon Numarası")]
    [Phone]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Roller")]
    public List<string> Roles { get; set; } = new List<string>();

    [Display(Name = "E-posta Onaylı mı?")]
    public bool EmailConfirmed { get; set; }
}

