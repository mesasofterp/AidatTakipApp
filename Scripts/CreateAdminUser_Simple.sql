-- BASIT VERSÝYON - AspNetUsers için Admin kullanýcýsý
-- Þifre: 123456
-- Email: admin@aidattakip.com

-- NOT: Bu script'i çalýþtýrdýktan sonra ilk giriþ yapacaðýnýzda
-- Identity þifre politikasý nedeniyle þifre deðiþtirme isteyebilir.
-- Eðer hash çalýþmazsa, uygulama içinden þifre sýfýrlama yapmanýz gerekebilir.

-- 1. Admin Rolü
DECLARE @AdminRoleId NVARCHAR(450) = '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@AdminRoleId, 'Admin', 'ADMIN', NEWID())
END
ELSE
BEGIN
    SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'
END

-- 2. Admin Kullanýcýsý
DECLARE @AdminUserId NVARCHAR(450) = '9f8e7d6c-5b4a-3c2d-1e0f-9a8b7c6d5e4f'

IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'admin@aidattakip.com')
BEGIN
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
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount
    )
  VALUES (
        @AdminUserId,     -- Id
  'admin@aidattakip.com',         -- UserName
   'ADMIN@AIDATTAKIP.COM',   -- NormalizedUserName
        'admin@aidattakip.com',       -- Email
        'ADMIN@AIDATTAKIP.COM',          -- NormalizedEmail
        1, -- EmailConfirmed
        'AQAAAAIAAYagAAAAEJZfhEH8tPXVhYqN8YS+xBSqYU3qxZqK2FPLqIRFGYmKH2bQEKjDM3iZPZVGEgFZhQ==', -- PasswordHash (123456)
        CONVERT(NVARCHAR(450), NEWID()),       -- SecurityStamp
        CONVERT(NVARCHAR(450), NEWID()),        -- ConcurrencyStamp
        NULL,    -- PhoneNumber
        0,     -- PhoneNumberConfirmed
        0,       -- TwoFactorEnabled
        NULL,              -- LockoutEnd
        1,-- LockoutEnabled
        0          -- AccessFailedCount
    )

    -- Kullanýcýya Admin rolünü ata
    INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@AdminUserId, @AdminRoleId)

    SELECT 'Admin kullanýcýsý baþarýyla oluþturuldu!' AS Sonuc,
           'admin@aidattakip.com' AS Email,
    '123456' AS Sifre
END
ELSE
BEGIN
    SELECT 'Admin kullanýcýsý zaten mevcut.' AS Sonuc
END

-- Sonucu göster
SELECT 
 u.Id,
    u.UserName,
    u.Email,
u.EmailConfirmed,
    STRING_AGG(r.Name, ', ') AS Roles
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@aidattakip.com'
GROUP BY u.Id, u.UserName, u.Email, u.EmailConfirmed
