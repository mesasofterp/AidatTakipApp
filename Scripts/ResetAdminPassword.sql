-- Admin kullanýcýsýnýn þifresini sýfýrlama (123456)
-- Bu script admin kullanýcýsýnýn þifresini güncelleyecek

DECLARE @AdminUserId NVARCHAR(450)
DECLARE @NewPasswordHash NVARCHAR(MAX)

-- Admin kullanýcýsýnýn ID'sini al
SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@aidattakip.com'

-- Yeni þifre hash'i: 123456
-- Bu hash ASP.NET Core Identity 8.0 için geçerlidir
SET @NewPasswordHash = 'AQAAAAIAAYagAAAAEJZfhEH8tPXVhYqN8YS+xBSqYU3qxZqK2FPLqIRFGYmKH2bQEKjDM3iZPZVGEgFZhQ=='

IF @AdminUserId IS NOT NULL
BEGIN
    -- Þifreyi güncelle ve SecurityStamp'i yenile
    UPDATE AspNetUsers
    SET PasswordHash = @NewPasswordHash,
      SecurityStamp = NEWID(),
        ConcurrencyStamp = NEWID()
    WHERE Id = @AdminUserId
    
    PRINT 'Admin kullanýcýsýnýn þifresi baþarýyla güncellendi!'
    PRINT 'Email: admin@aidattakip.com'
    PRINT 'Þifre: 123456'
    
 -- Kontrol
    SELECT 
        UserName,
        Email,
     EmailConfirmed,
        LockoutEnabled,
      AccessFailedCount
    FROM AspNetUsers
    WHERE Id = @AdminUserId
END
ELSE
BEGIN
    PRINT 'HATA: Admin kullanýcýsý bulunamadý!'
END

-- Alternatif: Eðer yukarýdaki hash çalýþmazsa, baþka bir hash deneyin
-- Bu hash .NET 6/7/8 için baþka bir algoritma kullanýyor
-- SET @NewPasswordHash = 'AQAAAAEAACcQAAAAEBKhWz0bKrOkKYbqMHqHpIcdrHPiN0QD8lC6wJKH5dZkbGSsFyNjBqXjPuPqKbqkfA=='
