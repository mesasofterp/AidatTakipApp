using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentApp.Models
{
    public class OgrenciDetay : BaseEntity
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Öğrenci seçimi zorunludur")]
        [Display(Name = "Öğrenci")]
        public long OgrenciId { get; set; }

        [StringLength(200, ErrorMessage = "Veli Ad Soyad 200 karakterden fazla olamaz")]
        [Display(Name = "Veli Ad Soyad")]
        public string? VeliAdSoyad { get; set; }

        [StringLength(20, ErrorMessage = "Veli Telefon Numarası 20 karakterden fazla olamaz")]
        [Display(Name = "Veli Telefon Numarası")]
        public string? VeliTelefonNumarasi { get; set; }

        [StringLength(200, ErrorMessage = "Okul Adı 200 karakterden fazla olamaz")]
        [Display(Name = "Okul Adı")]
        public string? OkulAdi { get; set; }

        [StringLength(300, ErrorMessage = "Okul Adresi 300 karakterden fazla olamaz")]
        [Display(Name = "Okul Adresi")]
        public string? OkulAdresi { get; set; }

        [StringLength(50, ErrorMessage = "Sınıf 50 karakterden fazla olamaz")]
        [Display(Name = "Sınıf")]
        public string? Sinif { get; set; }

        [StringLength(200, ErrorMessage = "Okul Hocası Ad Soyad 200 karakterden fazla olamaz")]
        [Display(Name = "Okul Hocası Ad Soyad")]
        public string? OkulHocasiAdSoyad { get; set; }

        [StringLength(20, ErrorMessage = "Okul Hocası Telefon 20 karakterden fazla olamaz")]
        [Display(Name = "Okul Hocası Telefon")]
        public string? OkulHocasiTelefon { get; set; }

        [Display(Name = "Okul Giriş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan? OkulGirisSaati { get; set; }

        [Display(Name = "Okul Çıkış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan? OkulCikisSaati { get; set; }
        [StringLength(200, ErrorMessage = "Veli Ad Soyad 200 karakterden fazla olamaz")]
        [Display(Name = "Veli Ad Soyad")]
        public string? Veli2AdSoyad { get; set; }

        [StringLength(20, ErrorMessage = "Veli Telefon Numarası 20 karakterden fazla olamaz")]
        [Display(Name = "Veli Telefon Numarası")]
        public string? Veli2TelefonNumarasi { get; set; }

        [ValidateNever]
        public Ogrenciler Ogrenci { get; set; }
    }
}

