-- AspNetUsers için Admin kullanýcýsý oluþturma scripti
-- Þifre: 123456
-- Email: admin@aidattakip.com

-- 1. Önce rolleri oluþtur (varsa atla)
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
END

-- 2. Admin kullanýcýsýný oluþtur
DECLARE @UserId NVARCHAR(450) = NEWID()
DECLARE @AdminRoleId NVARCHAR(450)

-- Admin rolünün ID'sini al
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'

-- Kullanýcý zaten var mý kontrol et
IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'admin@aidattakip.com')
BEGIN
    -- Kullanýcýyý ekle
    -- Þifre hash'i: 123456 için Identity default hasher kullanýlarak üretilmiþ
    -- NOT: Bu hash, AspNetCore Identity V3 formatýnda üretilmiþtir
    INSERT INTO AspNetUsers (
        Id, 
    UserName, 
        NormalizedUserName, 
     Email, 
        NormalizedEmail, 
  EmailConfirmed, 
        PasswordHash, 
     SecurityStamp, 
        ConcurrencyStamp, 
        PhoneNumberConfirmed, 
  TwoFactorEnabled, 
  LockoutEnabled, 
        AccessFailedCount
    )
    VALUES (
        @UserId,
        'admin@aidattakip.com',
  'ADMIN@AIDATTAKIP.COM',
        'admin@aidattakip.com',
        'ADMIN@AIDATTAKIP.COM',
        1, -- EmailConfirmed = true
        'AQAAAAIAAYagAAAAEJZfhEH8tPXVhYqN8YS+xBSqYU3qxZqK2FPLqIRFGYmKH2bQEKjDM3iZPZVGEgFZhQ==', -- Þifre: 123456
        NEWID(), -- SecurityStamp
        NEWID(), -- ConcurrencyStamp
     0, -- PhoneNumberConfirmed = false
        0, -- TwoFactorEnabled = false
        1, -- LockoutEnabled = true
        0  -- AccessFailedCount = 0
    )

    -- Kullanýcýya Admin rolünü ata
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @AdminRoleId)

    PRINT 'Admin kullanýcýsý baþarýyla oluþturuldu!'
    PRINT 'Email: admin@aidattakip.com'
    PRINT 'Þifre: 123456'
END
ELSE
BEGIN
    PRINT 'Admin kullanýcýsý zaten mevcut.'
    
    -- Mevcut kullanýcýnýn ID'sini al
    SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'admin@aidattakip.com'
    
    -- Admin rolüne sahip deðilse ekle
    IF NOT EXISTS (SELECT * FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
   VALUES (@UserId, @AdminRoleId)
        PRINT 'Mevcut kullanýcýya Admin rolü eklendi.'
    END
END

-- Kontrol sorgusu
SELECT 
    u.UserName,
    u.Email,
    u.EmailConfirmed,
    r.Name AS RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@aidattakip.com'
