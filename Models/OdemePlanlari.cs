using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class OdemePlanlari : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Kurs programý alaný zorunludur")]
        [StringLength(100, ErrorMessage = "Kurs programý 100 karakterden fazla olamaz")]
        [Display(Name = "Kurs Programý")]
        public string KursProgrami { get; set; } = string.Empty;

        [Required(ErrorMessage = "Taksit sayýsý zorunludur")]
        [Range(1, 100, ErrorMessage = "Taksit sayýsý 1 ile 100 arasýnda olmalýdýr")]
        [Display(Name = "Taksit Sayýsý")]
        public int Taksit { get; set; }

        [Display(Name = "Vade (Gün)")]
        [Range(0, 365, ErrorMessage = "Vade 0 ile 365 gün arasýnda olmalýdýr")]
        public int? Vade { get; set; }

        [Required(ErrorMessage = "Tutar alaný zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalýdýr")]
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; } = 0;
    }
}
