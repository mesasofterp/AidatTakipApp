CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Cinsiyetler] (
    [Id] bigint NOT NULL IDENTITY,
    [Cinsiyet] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Cinsiyetler] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [OdemePlanlari] (
    [Id] bigint NOT NULL IDENTITY,
    [KursProgrami] nvarchar(100) NOT NULL,
    [Taksit] int NOT NULL,
    [Vade] int NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [IsDeleted] bit NOT NULL,
    [Aktif] bit NOT NULL,
    [Version] int NOT NULL,
    [Aciklama] nvarchar(max) NULL,
    CONSTRAINT [PK_OdemePlanlari] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ZamanlayiciAyarlar] (
    [Id] bigint NOT NULL IDENTITY,
    [Isim] nvarchar(100) NOT NULL,
    [Saat] int NOT NULL,
    [Dakika] int NOT NULL,
    [CronIfadesi] nvarchar(100) NOT NULL,
    [Aciklama] nvarchar(500) NULL,
    [MesajSablonu] nvarchar(500) NOT NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [GuncellenmeTarihi] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [Aktif] bit NOT NULL,
    [Version] int NOT NULL,
    CONSTRAINT [PK_ZamanlayiciAyarlar] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Ogrenciler] (
    [Id] bigint NOT NULL IDENTITY,
    [OgrenciAdi] nvarchar(50) NOT NULL,
    [OgrenciSoyadi] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Telefon] nvarchar(20) NULL,
    [TCNO] nvarchar(11) NULL,
    [Boy] int NULL,
    [Kilo] decimal(5,2) NULL,
    [Adres] nvarchar(500) NULL,
    [KayitTarihi] datetime2 NOT NULL,
    [DogumTarihi] datetime2 NOT NULL,
    [SonSmsTarihi] datetime2 NULL,
    [OdemePlanlariId] bigint NOT NULL,
    [CinsiyetId] bigint NOT NULL,
    [IsDeleted] bit NOT NULL,
    [Aktif] bit NOT NULL,
    [Version] int NOT NULL,
    [Aciklama] nvarchar(max) NULL,
    CONSTRAINT [PK_Ogrenciler] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ogrenciler_Cinsiyetler_CinsiyetId] FOREIGN KEY ([CinsiyetId]) REFERENCES [Cinsiyetler] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ogrenciler_OdemePlanlari_OdemePlanlariId] FOREIGN KEY ([OdemePlanlariId]) REFERENCES [OdemePlanlari] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [OgrenciOdemeTakvimi] (
    [Id] bigint NOT NULL IDENTITY,
    [OgrenciId] bigint NOT NULL,
    [OdemeTarihi] datetime2 NULL,
    [OlusturmaTarihi] datetime2 NOT NULL,
    [SonOdemeTarihi] datetime2 NULL,
    [Odendi] bit NOT NULL,
    [SmsGittiMi] bit NOT NULL,
    [OdenenTutar] decimal(18,2) NOT NULL,
    [BorcTutari] decimal(18,2) NOT NULL,
    [TaksitNo] int NULL,
    [TaksitTutari] decimal(18,2) NULL,
    [IsDeleted] bit NOT NULL,
    [Aktif] bit NOT NULL,
    [Version] int NOT NULL,
    [Aciklama] nvarchar(max) NULL,
    CONSTRAINT [PK_OgrenciOdemeTakvimi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OgrenciOdemeTakvimi_Ogrenciler_OgrenciId] FOREIGN KEY ([OgrenciId]) REFERENCES [Ogrenciler] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_Ogrenciler_CinsiyetId] ON [Ogrenciler] ([CinsiyetId]);
GO


CREATE UNIQUE INDEX [IX_Ogrenciler_Email] ON [Ogrenciler] ([Email]);
GO


CREATE INDEX [IX_Ogrenciler_OdemePlanlariId] ON [Ogrenciler] ([OdemePlanlariId]);
GO


CREATE INDEX [IX_OgrenciOdemeTakvimi_OgrenciId] ON [OgrenciOdemeTakvimi] ([OgrenciId]);
GO


