using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Kayıt tarihi zorunludur")]
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.Date)]
        public DateTime KayitTarihi { get; set; }

        public DateTime DogumTarihi { get; set; }

        public long OdemePlanlariId { get; set; }

        public long CinsiyetId { get; set; }

        public OdemePlanlari OdemePlanlari { get; set; }
        public Cinsiyetler Cinsiyet { get; set; }
    }
}
