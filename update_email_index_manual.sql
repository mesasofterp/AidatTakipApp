-- Email unique index'ini kaldýr ve filtered index oluþtur
USE [AidatTakipDb2];
GO

-- Mevcut unique index'i kaldýr
IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Ogrenciler_Email' AND object_id = OBJECT_ID('Ogrenciler'))
BEGIN
    DROP INDEX [IX_Ogrenciler_Email] ON [Ogrenciler];
    PRINT 'IX_Ogrenciler_Email index kaldýrýldý.';
END
GO

-- Filtered unique index oluþtur (sadece IsDeleted = 0 olanlar için)
CREATE UNIQUE INDEX [IX_Ogrenciler_Email] 
ON [Ogrenciler]([Email]) 
WHERE [IsDeleted] = 0;
PRINT 'IX_Ogrenciler_Email filtered index oluþturuldu.';
GO

-- TCNO için filtered unique index oluþtur (eðer yoksa)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Ogrenciler_TCNO' AND object_id = OBJECT_ID('Ogrenciler'))
BEGIN
    CREATE UNIQUE INDEX [IX_Ogrenciler_TCNO] 
    ON [Ogrenciler]([TCNO]) 
    WHERE [IsDeleted] = 0 AND [TCNO] IS NOT NULL;
    PRINT 'IX_Ogrenciler_TCNO filtered index oluþturuldu.';
END
ELSE
BEGIN
    PRINT 'IX_Ogrenciler_TCNO index zaten mevcut.';
END
GO

PRINT 'Index güncellemesi tamamlandý!';
GO
