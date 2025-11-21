@echo off
echo ========================================
echo AidatTakipApp Windows Service Kaldirma
echo ========================================
echo.

REM Yonetici yetkisi kontrolu
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo HATA: Bu script yonetici yetkisi ile calistirilmalidir!
    echo Lutfen sag tiklayip "Yonetici olarak calistir" secenegini secin.
    pause
    exit /b 1
)

REM Servis kontrolu
sc query "AidatTakipApp" >nul 2>&1
if %errorLevel% neq 0 (
    echo Servis bulunamadi!
    pause
    exit /b 0
)

REM Servisi durdur
echo Servis durduruluyor...
net stop "AidatTakipApp"
timeout /t 2 /nobreak >nul

REM Servisi kaldir
echo Servis kaldiriliyor...
sc delete "AidatTakipApp"

if %errorLevel% equ 0 (
    echo.
    echo Servis basariyla kaldirildi!
) else (
    echo.
    echo HATA: Servis kaldirilirken hata olustu!
    echo Hata kodu: %errorLevel%
)

echo.
pause




