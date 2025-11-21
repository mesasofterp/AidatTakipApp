using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentApp.Models
{
    public class Seanslar : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Seans adý alaný zorunludur")]
        [StringLength(100, ErrorMessage = "Seans adý 100 karakterden fazla olamaz")]
        [Display(Name = "Seans Adý")]
        public string SeansAdi { get; set; } = string.Empty;

        [Display(Name = "Seans Baþlangýç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan SeansBaslangicSaati { get; set; }

        [Display(Name = "Seans Bitis Saati")]
        [DataType(DataType.Time)]
        public TimeSpan SeansBitisSaati { get; set; }

        [Display(Name = "Seans Kapasitesi")]
        public int? SeansKapasitesi { get; set; }

        [Display(Name = "Seans Mevcudu")]
        public int? SeansMevcudu { get; set; }

        public long GunId { get; set; }
        public Gunler Gun { get; set; }
    }
}
