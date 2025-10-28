-- Cinsiyetler tablosuna Erkek ve Kadýn kayýtlarýný ekle
-- Eðer kayýt yoksa ekle

IF NOT EXISTS (SELECT 1 FROM Cinsiyetler WHERE Cinsiyet = 'Erkek')
BEGIN
    INSERT INTO Cinsiyetler (Cinsiyet) VALUES ('Erkek')
    PRINT 'Erkek kaydý eklendi'
END
ELSE
BEGIN
    PRINT 'Erkek kaydý zaten mevcut'
END

IF NOT EXISTS (SELECT 1 FROM Cinsiyetler WHERE Cinsiyet = 'Kadýn')
BEGIN
    INSERT INTO Cinsiyetler (Cinsiyet) VALUES ('Kadýn')
    PRINT 'Kadýn kaydý eklendi'
END
ELSE
BEGIN
    PRINT 'Kadýn kaydý zaten mevcut'
END

-- Migration history'yi güncelle
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251028125633_SeedCinsiyetlerData')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251028125633_SeedCinsiyetlerData', '8.0.0')
    PRINT 'Migration kaydý eklendi'
END
ELSE
BEGIN
    PRINT 'Migration kaydý zaten mevcut'
END

SELECT * FROM Cinsiyetler
