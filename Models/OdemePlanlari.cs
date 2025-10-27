using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Range(1, 3650, ErrorMessage = "Vade 1 ile 3650 gün arasýnda olmalýdýr")]
        public int? Vade { get; set; }

        [Required(ErrorMessage = "Toplam tutar alaný zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam tutar 0'dan büyük olmalýdýr")]
        [Display(Name = "Toplam Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; } = 0;

        /// <summary>
        /// Taksit baþýna düþen tutar (Hesaplanmýþ alan)
        /// </summary>
        [NotMapped]
        [Display(Name = "Taksit Tutarý")]
        public decimal TaksitTutari => Taksit > 0 ? Math.Round(Tutar / Taksit, 2) : 0;
    }
}
