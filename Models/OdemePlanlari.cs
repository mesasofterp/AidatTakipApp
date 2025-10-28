using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentApp.Models
{
    public class OdemePlanlari : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Kurs program� alan� zorunludur")]
        [StringLength(100, ErrorMessage = "Kurs program� 100 karakterden fazla olamaz")]
        [Display(Name = "Kurs Program�")]
        public string KursProgrami { get; set; } = string.Empty;

        [Required(ErrorMessage = "Taksit say�s� zorunludur")]
        [Range(1, 100, ErrorMessage = "Taksit say�s� 1 ile 100 aras�nda olmal�d�r")]
        [Display(Name = "Taksit Say�s�")]
        public int Taksit { get; set; }

        [Display(Name = "Vade (G�n)")]
        [Range(1, 3650, ErrorMessage = "Vade 1 ile 3650 g�n aras�nda olmal�d�r")]
        public int? Vade { get; set; }

        [Required(ErrorMessage = "Toplam tutar alan� zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam tutar 0'dan b�y�k olmal�d�r")]
        [Display(Name = "Toplam Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; } = 0;

        /// <summary>
        /// Taksit ba��na d��en tutar (Hesaplanm�� alan)
        /// </summary>
        [NotMapped]
        [Display(Name = "Taksit Tutar�")]
        public decimal TaksitTutari => Taksit > 0 ? Math.Round(Tutar / Taksit, 2) : 0;
    }
}
