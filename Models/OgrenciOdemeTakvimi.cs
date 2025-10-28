using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciOdemeTakvimi : BaseEntity
    {
      public long Id { get; set; }
        
        [Required(ErrorMessage = "��renci se�imi zorunludur")]
        [Display(Name = "��renci")]
         public long OgrenciId { get; set; }
 
      [Display(Name = "�deme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? OdemeTarihi { get; set; }

        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Son Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? SonOdemeTarihi { get; set; }

        [Required(ErrorMessage = "�denen tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "�denen tutar 0'dan b�y�k olmal�d�r")]
        [Display(Name = "�denen Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal OdenenTutar { get; set; } = 0;

        [Required(ErrorMessage = "Bor� tutar� zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Bor� tutar� negatif olamaz")]
    [Display(Name = "Kalan Bor�")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal BorcTutari { get; set; }

        // Navigation property
        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; }
    }
}
