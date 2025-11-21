using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class SeansGunler : BaseEntity
    {
      public long Id { get; set; }
        
         [Required(ErrorMessage = "Gün seçimi zorunludur")]
         [Display(Name = "Gün")]
         public long GunId { get; set; }

        [Required(ErrorMessage = "Seans seçimi zorunludur")]
        [Display(Name = "Seans")]
        public long SeansId { get; set; }

        // Navigation properties with explicit ForeignKey attributes
        [ValidateNever]
   [ForeignKey("GunId")]
        public Gunler Gun { get; set; }
        
        [ValidateNever]
    [ForeignKey("SeansId")]
        public Seanslar Seans { get; set; }
    }
}
