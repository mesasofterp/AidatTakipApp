@echo off
echo ========================================
echo AidatTakipApp Windows Service Yukleme
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

REM Mevcut servisi durdur ve kaldir (varsa)
echo Mevcut servis kontrol ediliyor...
sc query "AidatTakipApp" >nul 2>&1
if %errorLevel% equ 0 (
    echo Mevcut servis durduruluyor...
    net stop "AidatTakipApp"
    timeout /t 2 /nobreak >nul
    echo Mevcut servis kaldiriliyor...
    sc delete "AidatTakipApp"
    timeout /t 2 /nobreak >nul
)

REM EXE dosyasinin yolunu al
set "SERVICE_PATH=%~dp0StudentApp.exe"
if not exist "%SERVICE_PATH%" (
    echo HATA: StudentApp.exe dosyasi bulunamadi!
    echo Lutfen bu scripti publish edilmis klasorun icinde calistirin.
    pause
    exit /b 1
)

REM Servisi yukle
echo.
echo Servis yukleniyor: %SERVICE_PATH%
sc create "AidatTakipApp" binPath= "\"%SERVICE_PATH%\"" start= auto DisplayName= "Aidat Takip Uygulamasi"

if %errorLevel% equ 0 (
    echo.
    echo Servis basariyla yuklendi!
    echo.
    echo Servisi baslatmak icin: net start AidatTakipApp
    echo Servisi durdurmak icin: net stop AidatTakipApp
    echo Servisi kaldirmak icin: sc delete AidatTakipApp
    echo.
    echo Servisi simdi baslatmak ister misiniz? (E/H)
    set /p START_SERVICE=
    if /i "%START_SERVICE%"=="E" (
        net start "AidatTakipApp"
        if %errorLevel% equ 0 (
            echo Servis basariyla baslatildi!
        ) else (
            echo Servis baslatilirken hata olustu!
        )
    )
) else (
    echo.
    echo HATA: Servis yuklenirken hata olustu!
    echo Hata kodu: %errorLevel%
)

echo.
pause

