using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models.ViewModels;

public class ProfileViewModel
{
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
}

