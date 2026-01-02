using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciUyelikDondurma : BaseEntity
 {
  public long Id { get; set; }

        [Required(ErrorMessage = "Öðrenci seçimi zorunludur")]
        [Display(Name = "Öðrenci")]
        public long OgrenciId { get; set; }

     [Required(ErrorMessage = "Dondurma baþlangýç tarihi zorunludur")]
  [Display(Name = "Dondurma Baþlangýç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; }

   [Required(ErrorMessage = "Dondurma bitiþ tarihi zorunludur")]
        [Display(Name = "Dondurma Bitiþ Tarihi")]
  [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; }

        [Required(ErrorMessage = "Dondurma sebebi zorunludur")]
        [Display(Name = "Dondurma Sebebi")]
      [StringLength(500, ErrorMessage = "Sebep en fazla 500 karakter olabilir")]
  public string Sebep { get; set; } = string.Empty;

[Display(Name = "Detaylý Açýklama")]
        [StringLength(2000, ErrorMessage = "Açýklama en fazla 2000 karakter olabilir")]
        public string? Aciklama { get; set; }

      [Display(Name = "Durum")]
public DondurmaStatusEnum Status { get; set; } = DondurmaStatusEnum.Aktif;

        [Display(Name = "Ödeme Tarihleri Ayarlandý mý?")]
        public bool OdemeTarihleriAyarlandi { get; set; } = false;

   [Display(Name = "Kaydýrýlan Gün Sayýsý")]
        public int KaydirilanGunSayisi { get; set; } = 0;

        [Display(Name = "Ýptal Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime? IptalTarihi { get; set; }

    [Display(Name = "Ýptal Eden Kullanýcý")]
        [StringLength(100)]
    public string? IptalEdenKullanici { get; set; }

        [Display(Name = "Ýptal Nedeni")]
        [StringLength(500)]
        public string? IptalNedeni { get; set; }

 // Navigation property
        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; } = null!;
    }

    public enum DondurmaStatusEnum
    {
        [Display(Name = "Aktif")]
        Aktif = 1,

        [Display(Name = "Tamamlandý")]
        Tamamlandi = 2,

        [Display(Name = "Ýptal Edildi")]
IptalEdildi = 3
    }
}
