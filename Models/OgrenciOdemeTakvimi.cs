using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class OgrenciOdemeTakvimi : BaseEntity
    {
        public long Id { get; set; }
        
        [Required]
        public long OgrenciId { get; set; }
        
        public DateTime? OdemeTarihi { get; set; }

        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Son Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? SonOdemeTarihi { get; set; }

        public decimal OdenenTutar { get; set; } = 0;

        public decimal BorcTutari { get; set; }

        // Navigation property
        public Ogrenciler Ogrenci { get; set; }
    }
}
