using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciOdemeTakvimi : BaseEntity
    {
      public long Id { get; set; }
        
        [Required(ErrorMessage = "Öðrenci seçimi zorunludur")]
        [Display(Name = "Öðrenci")]
         public long OgrenciId { get; set; }
 
      [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? OdemeTarihi { get; set; }

        [Required(ErrorMessage = "Ödenen tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödenen tutar 0'dan büyük olmalýdýr")]
        [Display(Name = "Ödenen Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
  public decimal OdenenTutar { get; set; } = 0;

        [Required(ErrorMessage = "Borç tutarý zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Borç tutarý negatif olamaz")]
    [Display(Name = "Kalan Borç")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal BorcTutari { get; set; }

        // Navigation property
        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; }
    }
}
