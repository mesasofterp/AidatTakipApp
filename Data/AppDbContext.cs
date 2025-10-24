using Microsoft.EntityFrameworkCore;
using StudentApp.Models;

namespace StudentApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ogrenciler> Ogrenciler { get; set; }
        public DbSet<Cinsiyetler> Cinsiyetler { get; set; }
        public DbSet<OdemePlanlari> OdemePlanlari { get; set; }
        public DbSet<OgrenciOdemeTakvimi> OgrenciOdemeTakvimi { get; set; }

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
            });

            // Configure Cinsiyetler entity
            modelBuilder.Entity<Cinsiyetler>(entity =>
            {
                entity.ToTable("Cinsiyetler");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cinsiyet).IsRequired().HasMaxLength(50);
            });

            // Configure OdemePlanlari entity
            modelBuilder.Entity<OdemePlanlari>(entity =>
            {
                entity.ToTable("OdemePlanlari");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.KursProgrami).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Taksit).IsRequired();
                entity.Property(e => e.Tutar).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Configure OgrenciOdemeTakvimi entity
            modelBuilder.Entity<OgrenciOdemeTakvimi>(entity =>
            {
                entity.ToTable("OgrenciOdemeTakvimi");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OdenenTutar).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.BorcTutari).HasColumnType("decimal(18,2)").IsRequired();

                // Foreign key relationship
                entity.HasOne(e => e.Ogrenci)
                    .WithMany()
                    .HasForeignKey(e => e.OgrenciId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
