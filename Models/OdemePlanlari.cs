using System.ComponentModel.DataAnnotations;

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
        [Range(0, 365, ErrorMessage = "Vade 0 ile 365 g�n aras�nda olmal�d�r")]
        public int? Vade { get; set; }

        [Required(ErrorMessage = "Tutar alan� zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan b�y�k olmal�d�r")]
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; } = 0;
    }
}
