using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    /// <summary>
    /// Öðrenci üyelik yenileme kayýtlarý
  /// </summary>
    public class OgrenciUyelikYenileme : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Öðrenci seçimi zorunludur")]
        [Display(Name = "Öðrenci")]
        public long OgrenciId { get; set; }

        [Required(ErrorMessage = "Eski ödeme planý bilgisi zorunludur")]
      [Display(Name = "Eski Ödeme Planý")]
        public long EskiOdemePlaniId { get; set; }

        [Required(ErrorMessage = "Yeni ödeme planý bilgisi zorunludur")]
    [Display(Name = "Yeni Ödeme Planý")]
     public long YeniOdemePlaniId { get; set; }

        [Required(ErrorMessage = "Yenileme tarihi zorunludur")]
        [Display(Name = "Yenileme Tarihi")]
[DataType(DataType.Date)]
        public DateTime YenilemeTarihi { get; set; }

        [Display(Name = "Yenileme Baþlangýç Tarihi")]
     [DataType(DataType.Date)]
        public DateTime YenilemeBaslangicTarihi { get; set; }

        [Required]
        [Display(Name = "Eski Dönem Toplam Tutar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal EskiDonemToplamTutar { get; set; }

   [Required]
        [Display(Name = "Yeni Dönem Toplam Tutar")]
      [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal YeniDonemToplamTutar { get; set; }

        [Display(Name = "Eski Dönem Kalan Borç")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal EskiDonemKalanBorc { get; set; }

[Display(Name = "Ýndirim Tutarý")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal? IndirimTutari { get; set; }

     [Display(Name = "Ýndirim Açýklamasý")]
        [StringLength(500)]
        public string? IndirimAciklama { get; set; }

        [Display(Name = "Otomatik Yenileme")]
        public bool OtomatikYenileme { get; set; } = false;

  [Display(Name = "Yenileme Durumu")]
public YenilemeDurumuEnum Durum { get; set; } = YenilemeDurumuEnum.Aktif;

     [Display(Name = "Yenileyen Kullanýcý")]
        [StringLength(100)]
        public string? YenileyenKullanici { get; set; }

        // Navigation Properties
  [ValidateNever]
      public Ogrenciler Ogrenci { get; set; }

    [ValidateNever]
        public OdemePlanlari EskiOdemePlani { get; set; }

        [ValidateNever]
        public OdemePlanlari YeniOdemePlani { get; set; }
    }

    public enum YenilemeDurumuEnum
    {
      [Display(Name = "Aktif")]
Aktif = 1,

  [Display(Name = "Tamamlandý")]
      Tamamlandi = 2,

        [Display(Name = "Ýptal Edildi")]
        IptalEdildi = 3
    }
}
