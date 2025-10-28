-- Cinsiyetler tablosuna Erkek ve Kad�n kay�tlar�n� ekle
-- E�er kay�t yoksa ekle

IF NOT EXISTS (SELECT 1 FROM Cinsiyetler WHERE Cinsiyet = 'Erkek')
BEGIN
    INSERT INTO Cinsiyetler (Cinsiyet) VALUES ('Erkek')
    PRINT 'Erkek kayd� eklendi'
END
ELSE
BEGIN
    PRINT 'Erkek kayd� zaten mevcut'
END

IF NOT EXISTS (SELECT 1 FROM Cinsiyetler WHERE Cinsiyet = 'Kad�n')
BEGIN
    INSERT INTO Cinsiyetler (Cinsiyet) VALUES ('Kad�n')
    PRINT 'Kad�n kayd� eklendi'
END
ELSE
BEGIN
    PRINT 'Kad�n kayd� zaten mevcut'
END

-- Migration history'yi g�ncelle
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251028125633_SeedCinsiyetlerData')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251028125633_SeedCinsiyetlerData', '8.0.0')
    PRINT 'Migration kayd� eklendi'
END
ELSE
BEGIN
    PRINT 'Migration kayd� zaten mevcut'
END

SELECT * FROM Cinsiyetler
