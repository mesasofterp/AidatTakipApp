namespace StudentApp.Models.ViewModels
{
    public class EnvanterSatisViewModel
    {
        public long EnvanterId { get; set; }
        public DateTime? SatisTarihi { get; set; }
        public int SatisAdet { get; set; } = 1;
        public decimal OdenenTutar { get; set; }
        public string? Aciklama { get; set; }
    }
}
