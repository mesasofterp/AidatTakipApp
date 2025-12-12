using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class erol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cinsiyetler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cinsiyet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinsiyetler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Envanterler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvanterAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adet = table.Column<int>(type: "int", nullable: false),
                    AlisFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatisFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envanterler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gunler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gun = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gunler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OdemePlanlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KursProgrami = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaksitSayisi = table.Column<int>(type: "int", nullable: false),
                    Vade = table.Column<int>(type: "int", nullable: true),
                    TaksitTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[TaksitTutari] * [TaksitSayisi]", stored: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdemePlanlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZamanlayiciAyarlar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Isim = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Saat = table.Column<int>(type: "int", nullable: false),
                    Dakika = table.Column<int>(type: "int", nullable: false),
                    CronIfadesi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MesajSablonu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GorevCalismaGunuOfseti = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZamanlayiciAyarlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seanslar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeansAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SeansBaslangicSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    SeansBitisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    SeansKapasitesi = table.Column<int>(type: "int", nullable: true),
                    SeansMevcudu = table.Column<int>(type: "int", nullable: true),
                    GunId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seanslar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seanslar_Gunler_GunId",
                        column: x => x.GunId,
                        principalTable: "Gunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenciler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OgrenciSoyadi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TCNO = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Boy = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IlkTaksitSonOdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SonSmsTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OdemePlanlariId = table.Column<long>(type: "bigint", nullable: false),
                    CinsiyetId = table.Column<long>(type: "bigint", nullable: false),
                    Biyografi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeansId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ogrenciler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Cinsiyetler_CinsiyetId",
                        column: x => x.CinsiyetId,
                        principalTable: "Cinsiyetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_OdemePlanlari_OdemePlanlariId",
                        column: x => x.OdemePlanlariId,
                        principalTable: "OdemePlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Seanslar_SeansId",
                        column: x => x.SeansId,
                        principalTable: "Seanslar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciBasarilari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Turu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciBasarilari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciBasarilari_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciDetay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    VeliAdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VeliTelefonNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OkulAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OkulAdresi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Sinif = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OkulHocasiAdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OkulHocasiTelefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OkulGirisSaati = table.Column<TimeSpan>(type: "time", nullable: true),
                    OkulCikisSaati = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciDetay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciDetay_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciEnvanterSatis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OdenenTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KalanTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KalanTutarTahsilTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnvanterId = table.Column<long>(type: "bigint", nullable: false),
                    SatisAdet = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciEnvanterSatis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciEnvanterSatis_Envanterler_EnvanterId",
                        column: x => x.EnvanterId,
                        principalTable: "Envanterler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OgrenciEnvanterSatis_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciOdemeTakvimi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonOdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Odendi = table.Column<bool>(type: "bit", nullable: false),
                    SmsGittiMi = table.Column<bool>(type: "bit", nullable: false),
                    OdenenTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BorcTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaksitNo = table.Column<int>(type: "int", nullable: true),
                    TaksitTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciOdemeTakvimi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciOdemeTakvimi_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciBasariMaclari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasariId = table.Column<long>(type: "bigint", nullable: false),
                    RakipAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Skor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sonuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lokasyon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciBasariMaclari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciBasariMaclari_OgrenciBasarilari_BasariId",
                        column: x => x.BasariId,
                        principalTable: "OgrenciBasarilari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciBasarilari_OgrenciId",
                table: "OgrenciBasarilari",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciBasariMaclari_BasariId",
                table: "OgrenciBasariMaclari",
                column: "BasariId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDetay_OgrenciId",
                table: "OgrenciDetay",
                column: "OgrenciId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciEnvanterSatis_EnvanterId",
                table: "OgrenciEnvanterSatis",
                column: "EnvanterId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciEnvanterSatis_OgrenciId",
                table: "OgrenciEnvanterSatis",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_CinsiyetId",
                table: "Ogrenciler",
                column: "CinsiyetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_Email",
                table: "Ogrenciler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_OdemePlanlariId",
                table: "Ogrenciler",
                column: "OdemePlanlariId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_SeansId",
                table: "Ogrenciler",
                column: "SeansId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciOdemeTakvimi_OgrenciId",
                table: "OgrenciOdemeTakvimi",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_Seanslar_GunId",
                table: "Seanslar",
                column: "GunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "OgrenciBasariMaclari");

            migrationBuilder.DropTable(
                name: "OgrenciDetay");

            migrationBuilder.DropTable(
                name: "OgrenciEnvanterSatis");

            migrationBuilder.DropTable(
                name: "OgrenciOdemeTakvimi");

            migrationBuilder.DropTable(
                name: "ZamanlayiciAyarlar");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OgrenciBasarilari");

            migrationBuilder.DropTable(
                name: "Envanterler");

            migrationBuilder.DropTable(
                name: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "Cinsiyetler");

            migrationBuilder.DropTable(
                name: "OdemePlanlari");

            migrationBuilder.DropTable(
                name: "Seanslar");

            migrationBuilder.DropTable(
                name: "Gunler");
        }
    }
}
