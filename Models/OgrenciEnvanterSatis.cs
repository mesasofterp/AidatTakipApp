using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciEnvanterSatis : BaseEntity
    {
      public long Id { get; set; }
        
         [Required(ErrorMessage = "Öğrenci seçimi zorunludur")]
         [Display(Name = "Öğrenci")]
         public long OgrenciId { get; set; }
 
        [Display(Name = "Satış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? SatisTarihi { get; set; }

        [Required(ErrorMessage = "Ödenen tutar zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Ödenen tutar 0'dan küçük olamaz")]
        [Display(Name = "Ödenen Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal OdenenTutar { get; set; } = 0;

        public long EnvanterId { get; set; }

        // Navigation property
        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; }
        public Envanterler Envanter { get; set; }
    }
}
