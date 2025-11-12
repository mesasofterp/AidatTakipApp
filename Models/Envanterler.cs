using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentApp.Models
{
    public class Envanterler : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Envanter adý alaný zorunludur")]
        [StringLength(100, ErrorMessage = "Envanter adý 100 karakterden fazla olamaz")]
        [Display(Name = "Envanter Adý")]
        public string EnvanterAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adet zorunludur")]
        [Range(1, 200, ErrorMessage = "Adet 1 ile 200 arasýnda olmalýdýr")]
        [Display(Name = "Adet")]
        public int Adet { get; set; }

        [Required(ErrorMessage = "Fiyat alaný zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalýdýr")]
        [Display(Name = "Birim Fiyat")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BirimFiyat { get; set; } = 0;
    }
}
