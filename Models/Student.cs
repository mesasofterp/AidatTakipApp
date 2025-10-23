using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden fazla olamaz")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden fazla olamaz")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi")]
        [StringLength(100, ErrorMessage = "E-posta 100 karakterden fazla olamaz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kayıt tarihi zorunludur")]
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }
    }
}
