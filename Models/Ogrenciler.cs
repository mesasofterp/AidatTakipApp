using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class Ogrenciler : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden fazla olamaz")]
        [Display(Name = "Ad")]
        public string OgrenciAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden fazla olamaz")]
        [Display(Name = "Soyad")]
        public string OgrenciSoyadi { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi")]
        [StringLength(100, ErrorMessage = "E-posta 100 karakterden fazla olamaz")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçersiz telefon numarası")]
        [StringLength(20, ErrorMessage = "Telefon 20 karakterden fazla olamaz")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Required(ErrorMessage = "Kayıt tarihi zorunludur")]
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        public DateTime KayitTarihi { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "Ödeme planı seçimi zorunludur")]
        [Display(Name = "Ödeme Planı")]
        public long OdemePlanlariId { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur")]
        [Display(Name = "Cinsiyet")]
        public long CinsiyetId { get; set; }

        [ValidateNever]
        public OdemePlanlari OdemePlanlari { get; set; }
        
        [ValidateNever]
        public Cinsiyetler Cinsiyet { get; set; }
    }
}
