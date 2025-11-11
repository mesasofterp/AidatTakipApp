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
        public int TaksitSayisi { get; set; }

        [Display(Name = "Vade (Gün)")]
        [Range(1, 3650, ErrorMessage = "Vade 1 ile 3650 gün arasýnda olmalýdýr")]
        public int? Vade { get; set; }

        [Required(ErrorMessage = "Taksit tutarý alaný zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Taksit tutarý 0'dan büyük olmalýdýr")]
        [Display(Name = "Taksit Tutarý")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaksitTutari { get; set; } = 0;

        [Display(Name = "Toplam Tutar")]
        [Column(TypeName = "decimal(18,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ToplamTutar { get; set; }
    }
}
