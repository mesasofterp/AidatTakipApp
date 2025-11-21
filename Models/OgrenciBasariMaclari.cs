using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciBasariMaclari : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Başarı seçimi zorunludur")]
        [Display(Name = "Başarı")]
        public long BasariId { get; set; }

        [Required(ErrorMessage = "Rakip adı zorunludur")]
        [StringLength(200, ErrorMessage = "Rakip adı 200 karakterden fazla olamaz")]
        [Display(Name = "Rakip Adı")]
        public string RakipAdi { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Tür 100 karakterden fazla olamaz")]
        [Display(Name = "Tür")]
        public string? Tur { get; set; }

        [StringLength(100, ErrorMessage = "Kategori 100 karakterden fazla olamaz")]
        [Display(Name = "Kategori")]
        public string? Kategori { get; set; }

        [StringLength(50, ErrorMessage = "Skor 50 karakterden fazla olamaz")]
        [Display(Name = "Skor")]
        public string? Skor { get; set; }

        [StringLength(50, ErrorMessage = "Sonuç 50 karakterden fazla olamaz")]
        [Display(Name = "Sonuç")]
        public string? Sonuc { get; set; }

        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime? Tarih { get; set; }

        [StringLength(200, ErrorMessage = "Lokasyon 200 karakterden fazla olamaz")]
        [Display(Name = "Lokasyon")]
        public string? Lokasyon { get; set; }

        [ValidateNever]
        public OgrenciBasarilari Basari { get; set; }
    }
}

