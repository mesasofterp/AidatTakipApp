using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciBasarilari : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Öğrenci seçimi zorunludur")]
        [Display(Name = "Öğrenci")]
        public long OgrenciId { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur")]
        [StringLength(200, ErrorMessage = "Başlık 200 karakterden fazla olamaz")]
        [Display(Name = "Başlık")]
        public string Baslik { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Tür alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Tür 100 karakterden fazla olamaz")]
        [Display(Name = "Tür")]
        public string Turu { get; set; } = string.Empty;

        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime? Tarih { get; set; }

        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; }
    }
}

