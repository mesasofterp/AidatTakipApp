using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class OgrenciOdemeTakvimi : BaseEntity
    {
        public long Id { get; set; }
        
        [Required]
        public long OgrenciId { get; set; }
        
        public DateTime? OdemeTarihi { get; set; }

        public decimal OdenenTutar { get; set; } = 0;

        public decimal BorcTutari { get; set; }

        // Navigation property
        public Ogrenciler Ogrenci { get; set; }
    }
}
