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
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^(0)(\d{3})(\d{3})(\d{2})(\d{2})$", ErrorMessage = "Telefon numarası 0xxx xxx xx xx formatında olmalıdır (örn: 05301234567)")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası 11 haneli olmalıdır")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string? TCNO { get; set; }

        [Range(0, 300, ErrorMessage = "Boy 0-300 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? Boy { get; set; }

        [Range(0, 500, ErrorMessage = "Kilo 0-500 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public decimal? Kilo { get; set; }

        [StringLength(500, ErrorMessage = "Adres 500 karakterden fazla olamaz")]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Required(ErrorMessage = "Kayıt tarihi zorunludur")]
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        public DateTime KayitTarihi { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime DogumTarihi { get; set; }

        [Display(Name = "İlk Taksit Son Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? IlkTaksitSonOdemeTarihi { get; set; }

        public DateTime? SonSmsTarihi { get; set; }

        [Required(ErrorMessage = "Ödeme planı seçimi zorunludur")]
        [Display(Name = "Ödeme Planı")]
        public long OdemePlanlariId { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur")]
        [Display(Name = "Cinsiyet")]
        public long CinsiyetId { get; set; }

        [Display(Name = "Biyografi")]
        [DataType(DataType.MultilineText)]
        public string? Biyografi { get; set; }

        [Display(Name = "Seans")]
        public long? SeansId { get; set; }

        [ValidateNever]
        [Display(Name = "Ödeme Planı")]
        public OdemePlanlari OdemePlanlari { get; set; }
        
        [ValidateNever]
        public Cinsiyetler Cinsiyet { get; set; }

        [ValidateNever]
        public ICollection<OgrenciBasarilari> OgrenciBasarilari { get; set; } = new List<OgrenciBasarilari>();

        [ValidateNever]
        public OgrenciDetay? OgrenciDetay { get; set; }

        [ValidateNever]
        public Seanslar Seans { get; set; }
    }
}
