using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models.ViewModels;

public class CreateUserViewModel
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

    [Required]
    [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifre ve onay şifresi eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Roller")]
    public List<string> SelectedRoles { get; set; } = new List<string>();

    [Display(Name = "E-posta Onaylı mı?")]
    public bool EmailConfirmed { get; set; } = true;
}

