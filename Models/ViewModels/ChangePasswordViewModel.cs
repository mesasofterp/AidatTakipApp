using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Şifre")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre Tekrar")]
    [Compare("NewPassword", ErrorMessage = "Yeni şifre ve onay şifresi eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

