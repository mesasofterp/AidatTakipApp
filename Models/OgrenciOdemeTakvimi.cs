using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciOdemeTakvimi : BaseEntity
    {
      public long Id { get; set; }
        
        [Required(ErrorMessage = "Öğrenci seçimi zorunludur")]
        [Display(Name = "Öğrenci")]
         public long OgrenciId { get; set; }
 
      [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? OdemeTarihi { get; set; }

        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Son Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? SonOdemeTarihi { get; set; }

        [Display(Name = "Ödendi")]
     public bool Odendi { get; set; } = false;

        [Required(ErrorMessage = "Ödenen tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödenen tutar 0'dan büyük olmalıdır")]
        [Display(Name = "Ödenen Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal OdenenTutar { get; set; } = 0;

        [Required(ErrorMessage = "Borç tutarı zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Borç tutarı negatif olamaz")]
    [Display(Name = "Kalan Borç")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal BorcTutari { get; set; }

        [Display(Name = "Taksit Numarası")]
  public int? TaksitNo { get; set; }

        [Display(Name = "Taksit Tutarı")]
     [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal? TaksitTutari { get; set; }

        // Navigation property
        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; }
    }
}
