namespace StudentApp.Models.ViewModels
{
    public class OgrencilerFilterViewModel
    {
        public IEnumerable<Ogrenciler> Ogrenciler { get; set; } = new List<Ogrenciler>();
   
        // Filtreleme parametreleri
    public string? SearchTerm { get; set; }
        public long? CinsiyetId { get; set; }
        public long? OdemePlanlariId { get; set; }
        public int? MinYas { get; set; }
        public int? MaxYas { get; set; }
        public DateTime? BaslangicKayitTarihi { get; set; }
        public DateTime? BitisKayitTarihi { get; set; }
  public bool ShowPasif { get; set; } = false;
     
        // Sýralama parametreleri
        public string SortBy { get; set; } = "OgrenciSoyadi";
        public string SortOrder { get; set; } = "asc";
    
        // Dropdown listeleri
        public IEnumerable<Cinsiyetler>? Cinsiyetler { get; set; }
        public IEnumerable<OdemePlanlari>? OdemePlanlari { get; set; }
    }
}
