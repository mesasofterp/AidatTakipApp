using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class OdemePlanlari : BaseEntity
    {
        public long Id { get; set; }
        public string KursProgrami { get; set; }
        public int Taksit { get; set; }
        public int? Vade { get; set; }
        public decimal Tutar { get; set; } = 0;
    }
}
