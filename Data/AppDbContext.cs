using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using StudentApp.Models;

namespace StudentApp.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ogrenciler> Ogrenciler { get; set; }
        public DbSet<Cinsiyetler> Cinsiyetler { get; set; }
        public DbSet<OdemePlanlari> OdemePlanlari { get; set; }
        public DbSet<OgrenciOdemeTakvimi> OgrenciOdemeTakvimi { get; set; }
        public DbSet<ZamanlayiciAyarlar> ZamanlayiciAyarlar { get; set; }
        public DbSet<Envanterler> Envanterler { get; set; }
        public DbSet<OgrenciEnvanterSatis> OgrenciEnvanterSatis { get; set; }
        public DbSet<OgrenciBasarilari> OgrenciBasarilari { get; set; }
        public DbSet<OgrenciDetay> OgrenciDetay { get; set; }
        public DbSet<Gunler> Gunler { get; set; }
        public DbSet<Seanslar> Seanslar { get; set; }
        public DbSet<SeansGunler> SeansGunler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Ogrenciler entity
            modelBuilder.Entity<Ogrenciler>(entity =>
            {
                entity.ToTable("Ogrenciler");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OgrenciAdi).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OgrenciSoyadi).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefon).HasMaxLength(20);
                entity.Property(e => e.TCNO).HasMaxLength(11);
                entity.Property(e => e.Adres).HasMaxLength(500);
                entity.Property(e => e.Biyografi).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Kilo).HasColumnType("decimal(5,2)");
                entity.HasIndex(e => e.Email).IsUnique();

                // Foreign key relationships
                entity.HasOne(e => e.Cinsiyet)
                    .WithMany()
                    .HasForeignKey(e => e.CinsiyetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OdemePlanlari)
                    .WithMany()
                    .HasForeignKey(e => e.OdemePlanlariId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Seans)
                    .WithMany()
                    .HasForeignKey(e => e.SeansId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One-to-Many relationship with OgrenciBasarilari
                entity.HasMany(e => e.OgrenciBasarilari)
                    .WithOne(b => b.Ogrenci)
                    .HasForeignKey(b => b.OgrenciId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Cinsiyetler entity
            modelBuilder.Entity<Cinsiyetler>(entity =>
            {
                entity.ToTable("Cinsiyetler");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cinsiyet).IsRequired().HasMaxLength(50);
            });

            // Configure Günler entity
            modelBuilder.Entity<Gunler>(entity =>
            {
                entity.ToTable("Gunler");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Gun).IsRequired().HasMaxLength(50);
            });

            // Configure OdemePlanlari entity
            modelBuilder.Entity<OdemePlanlari>(entity =>
            {
                entity.ToTable("OdemePlanlari");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.KursProgrami).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TaksitSayisi).IsRequired();
                entity.Property(e => e.TaksitTutari).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.ToplamTutar)
                    .HasColumnType("decimal(18,2)")
                    .HasComputedColumnSql("[TaksitTutari] * [TaksitSayisi]", stored: true);
            });

            // Configure OgrenciOdemeTakvimi entity
            modelBuilder.Entity<OgrenciOdemeTakvimi>(entity =>
            {
                entity.ToTable("OgrenciOdemeTakvimi");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OlusturmaTarihi).IsRequired();
                entity.Property(e => e.SonOdemeTarihi);
                entity.Property(e => e.OdenenTutar).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.BorcTutari).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.TaksitTutari).HasColumnType("decimal(18,2)");

                // Foreign key relationship
                entity.HasOne(e => e.Ogrenci)
                    .WithMany()
                    .HasForeignKey(e => e.OgrenciId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SchedulerSettings entity
            modelBuilder.Entity<ZamanlayiciAyarlar>(entity =>
            {
                entity.ToTable("ZamanlayiciAyarlar");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Isim).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CronIfadesi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Aciklama).HasMaxLength(500);
                entity.Property(e => e.MesajSablonu).HasMaxLength(500);
                entity.Property(e => e.OlusturmaTarihi).IsRequired();
            });

            // Configure Envanterler entity
            modelBuilder.Entity<Envanterler>(entity =>
            {
                entity.ToTable("Envanterler");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EnvanterAdi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Adet).IsRequired();
                entity.Property(e => e.AlisFiyat).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.SatisFiyat).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Configure OgrenciEnvanterSatis entity
            modelBuilder.Entity<OgrenciEnvanterSatis>(entity =>
            {
                entity.ToTable("OgrenciEnvanterSatis");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SatisTarihi);
                entity.Property(e => e.OdenenTutar).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.KalanTutar).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.KalanTutarTahsilTarihi);
                entity.Property(e => e.SatisAdet).IsRequired();

                // Foreign key relationships
                entity.HasOne(e => e.Ogrenci)
                    .WithMany()
                    .HasForeignKey(e => e.OgrenciId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Envanter)
                    .WithMany()
                    .HasForeignKey(e => e.EnvanterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure OgrenciBasarilari entity
            modelBuilder.Entity<OgrenciBasarilari>(entity =>
            {
                entity.ToTable("OgrenciBasarilari");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Baslik).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Aciklama).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Turu).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Tarih);

                // Foreign key relationship
                entity.HasOne(e => e.Ogrenci)
                    .WithMany(o => o.OgrenciBasarilari)
                    .HasForeignKey(e => e.OgrenciId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OgrenciDetay entity
            modelBuilder.Entity<OgrenciDetay>(entity =>
            {
                entity.ToTable("OgrenciDetay");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VeliAdSoyad).HasMaxLength(200);
                entity.Property(e => e.VeliTelefonNumarasi).HasMaxLength(20);
                entity.Property(e => e.OkulAdi).HasMaxLength(200);
                entity.Property(e => e.OkulAdresi).HasMaxLength(300);
                entity.Property(e => e.Sinif).HasMaxLength(50);
                entity.Property(e => e.OkulHocasiAdSoyad).HasMaxLength(200);
                entity.Property(e => e.OkulHocasiTelefon).HasMaxLength(20);
                entity.Property(e => e.OkulGirisSaati).HasColumnType("time");
                entity.Property(e => e.OkulCikisSaati).HasColumnType("time");

                // Foreign key relationship
                entity.HasOne(e => e.Ogrenci)
                    .WithOne(o => o.OgrenciDetay)
                    .HasForeignKey<OgrenciDetay>(e => e.OgrenciId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Seanslar entity
            modelBuilder.Entity<Seanslar>(entity =>
            {
                entity.ToTable("Seanslar");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SeansAdi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SeansBaslangicSaati).IsRequired().HasColumnType("time");
                entity.Property(e => e.SeansBitisSaati).IsRequired().HasColumnType("time");
                entity.Property(e => e.SeansKapasitesi);
                entity.Property(e => e.SeansMevcudu);

                // GunId and Gun are NotMapped - no configuration needed
                // Many-to-Many relationship through SeansGunler is configured separately
            });

            // Configure SeansGunler entity (Many-to-Many junction table)
            modelBuilder.Entity<SeansGunler>(entity =>
            {
                entity.ToTable("SeansGunler");
                entity.HasKey(e => e.Id);

                // Explicitly define foreign key columns
                entity.Property(e => e.SeansId).HasColumnName("SeansId").IsRequired();
                entity.Property(e => e.GunId).HasColumnName("GunId").IsRequired();

                // Composite unique index - Bir seans bir günde sadece bir kez olabilir
                entity.HasIndex(e => new { e.SeansId, e.GunId })
                    .IsUnique()
                    .HasDatabaseName("IX_SeansGunler_SeansId_GunId");

                // Foreign key: SeansGunler -> Seanslar (explicitly use SeansId and navigation property)
                entity.HasOne(e => e.Seans)
                    .WithMany(s => s.SeansGunler)
                    .HasForeignKey(e => e.SeansId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SeansGunler_Seanslar");

                // Foreign key: SeansGunler -> Gunler
                entity.HasOne(e => e.Gun)
                    .WithMany()
                    .HasForeignKey(e => e.GunId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_SeansGunler_Gunler");
            });

        }
    }
}
