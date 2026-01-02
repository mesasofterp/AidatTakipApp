using StudentApp.Models;

namespace StudentApp.Services
{
    public interface IOgrenciUyelikYenilemeService
    {
        /// <summary>
  /// Tüm yenileme kayýtlarýný getirir
        /// </summary>
        Task<IEnumerable<OgrenciUyelikYenileme>> GetAllAsync();

        /// <summary>
   /// Belirli bir öðrencinin yenileme geçmiþini getirir
        /// </summary>
        Task<IEnumerable<OgrenciUyelikYenileme>> GetByOgrenciIdAsync(long ogrenciId);

        /// <summary>
        /// ID'ye göre yenileme kaydý getirir
        /// </summary>
      Task<OgrenciUyelikYenileme?> GetByIdAsync(long id);

      /// <summary>
        /// Öðrenci üyeliðini yeniler
   /// </summary>
  Task<OgrenciUyelikYenileme> YenileAsync(long ogrenciId, long yeniOdemePlaniId, decimal? indirimTutari, string? indirimAciklama, string kullaniciAdi);

        /// <summary>
        /// Öðrencinin mevcut üyelik durumunu kontrol eder
        /// </summary>
   Task<UyelikDurumBilgi> GetUyelikDurumuAsync(long ogrenciId);

        /// <summary>
     /// Yenileme gerekli mi kontrol eder
        /// </summary>
        Task<bool> YenilemeGerekliMiAsync(long ogrenciId);

        /// <summary>
        /// Yenileme için uygun öðrencileri getirir
        /// </summary>
   Task<IEnumerable<OgrenciYenilemeBilgi>> GetYenilenebilirOgrencilerAsync();

 /// <summary>
        /// Toplu yenileme yapar
   /// </summary>
        Task<TopluYenilemesonuc> TopluYenileAsync(List<long> ogrenciIdList, long yeniOdemePlaniId, string kullaniciAdi);
 }

    public class UyelikDurumBilgi
    {
      public long OgrenciId { get; set; }
        public string OgrenciAdSoyad { get; set; } = string.Empty;
        public OdemePlanlari? MevcutOdemePlani { get; set; }
        public int ToplamTaksitSayisi { get; set; }
        public int OdenenTaksitSayisi { get; set; }
        public decimal ToplamOdenen { get; set; }
        public decimal KalanBorc { get; set; }
  public DateTime? SonOdemeTarihi { get; set; }
        public DateTime? TahminiBitisTarihi { get; set; }
 public bool YenilemeGerekli { get; set; }
        public string YenilemeNedeni { get; set; } = string.Empty;
 }

    public class OgrenciYenilemeBilgi
    {
        public long OgrenciId { get; set; }
        public string OgrenciAdSoyad { get; set; } = string.Empty;
        public string OdemePlaniAdi { get; set; } = string.Empty;
        public long OdemePlaniId { get; set; }
public int TamamlananTaksitSayisi { get; set; }
        public int ToplamTaksitSayisi { get; set; }
        public decimal ToplamOdenen { get; set; }
        public decimal KalanBorc { get; set; }
        public DateTime? SonOdemeTarihi { get; set; }
        public int KalanGun { get; set; }
        public bool TumTaksitlerOdendi { get; set; }
    }

    public class TopluYenilemesonuc
    {
     public int ToplamOgrenciSayisi { get; set; }
        public int BasariliYenilemeSayisi { get; set; }
        public int HataliYenilemeSayisi { get; set; }
        public List<string> Hatalar { get; set; } = new List<string>();
        public List<long> YenilenenOgrenciIdler { get; set; } = new List<long>();
    }
}
