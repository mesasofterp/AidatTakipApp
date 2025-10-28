using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models;

public class ZamanlayiciAyarlar : BaseEntity
{
    public long Id { get; set; }

    [Required]
    [Display(Name = "İsim")]
    [StringLength(100)]
    public string Isim { get; set; } = "Varsayılan Zamanlayıcı";


    [Required]
    [Display(Name = "Çalışma Saati")]
    [Range(0, 23, ErrorMessage = "Saat 0 ile 23 arasında olmalıdır.")]
    public int Saat { get; set; } = 9;

    [Required]
    [Display(Name = "Çalışma Dakikası")]
    [Range(0, 59, ErrorMessage = "Dakika 0 ile 59 arasında olmalıdır.")]
    public int Dakika { get; set; } = 0;

    [Required]
    [Display(Name = "Cron İfadesi")]
    [StringLength(100)]
    public string CronIfadesi { get; set; } = "0 0 9 * * ?"; // Her gün 09:00

    [Display(Name = "Açıklama")]
    [StringLength(500)]
    public string? Aciklama { get; set; }

    [Display(Name = "SMS Mesaj Şablonu")]
    [StringLength(500)]
    public string MesajSablonu { get; set; } =
        "Sayın [ÖĞRENCİ_ADI] [ÖĞRENCİ_SOYADI], ödemeniz [GEÇEN_GÜN] gündür beklemektedir. " +
        "Borç tutarınız: [BORÇ_TUTARI]. Lütfen en kısa sürede ödemenizi yapınız.";

    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    [Display(Name = "Güncellenme Tarihi")]
    public DateTime? GuncellenmeTarihi { get; set; }
}

