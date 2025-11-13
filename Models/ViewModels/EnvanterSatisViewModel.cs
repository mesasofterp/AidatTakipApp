namespace StudentApp.Models.ViewModels
{
    public class EnvanterSatisViewModel
    {
        public long Id { get; set; } // Mevcut kayýt için
        public long EnvanterId { get; set; }
        public string? EnvanterAdi { get; set; } // Gösterim için
        public DateTime? SatisTarihi { get; set; }
        public int SatisAdet { get; set; } = 1;
        public decimal OdenenTutar { get; set; }
        public string? Aciklama { get; set; }
        public bool SilinecekMi { get; set; } // Silme iþareti için
    }
}
