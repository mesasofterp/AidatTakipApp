# Windows Service Kurulum Kılavuzu

## Gereksinimler
- Windows Server veya Windows 10/11 (Pro veya üzeri)
- Yönetici yetkisi
- .NET 8.0 Runtime yüklü olmalı

## Kurulum Adımları

### 1. Projeyi Publish Etme
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```
veya Visual Studio'dan:
- Projeye sağ tıklayın
- **Publish** seçeneğini seçin
- **Folder** profilini seçin
- **Release** modunda publish edin

### 2. Windows Service Olarak Yükleme

#### Yöntem 1: Otomatik Script ile (Önerilen)
1. Publish edilmiş klasöre gidin
2. `install-service.bat` dosyasına **sağ tıklayın**
3. **"Yönetici olarak çalıştır"** seçeneğini seçin
4. Script servisi otomatik olarak yükleyecektir

#### Yöntem 2: Manuel Komut ile
Yönetici olarak açılmış **Command Prompt** veya **PowerShell**'de:

```cmd
sc create "AidatTakipApp" binPath= "C:\Path\To\StudentApp.exe" start= auto DisplayName= "Aidat Takip Uygulamasi"
```

**Not:** `C:\Path\To\StudentApp.exe` kısmını publish edilmiş EXE dosyasının gerçek yoluna göre değiştirin.

### 3. Servisi Başlatma

```cmd
net start AidatTakipApp
```

veya **Services** (Hizmetler) penceresinden:
1. `Win + R` tuşlarına basın
2. `services.msc` yazın ve Enter'a basın
3. **Aidat Takip Uygulamasi** servisini bulun
4. Sağ tıklayıp **Start** seçeneğini seçin

### 4. Servis Durumunu Kontrol Etme

```cmd
sc query AidatTakipApp
```

### 5. Servisi Durdurma

```cmd
net stop AidatTakipApp
```

### 6. Servisi Kaldırma

#### Otomatik Script ile:
1. `uninstall-service.bat` dosyasına **sağ tıklayın**
2. **"Yönetici olarak çalıştır"** seçeneğini seçin

#### Manuel Komut ile:
```cmd
net stop AidatTakipApp
sc delete AidatTakipApp
```

## Servis Yapılandırması

### Port Ayarları
Servis varsayılan olarak **2961** portunda çalışır. Portu değiştirmek için:

1. Publish edilmiş klasördeki `appsettings.json` dosyasını açın
2. `"Port": "2961"` değerini istediğiniz port numarası ile değiştirin
3. Servisi yeniden başlatın

### Log Dosyaları
Windows Service olarak çalışırken loglar **Windows Event Log**'a yazılır:
1. `Win + R` tuşlarına basın
2. `eventvwr.msc` yazın ve Enter'a basın
3. **Windows Logs > Application** bölümünden logları görüntüleyebilirsiniz

### Firewall Ayarları
2961 portunu açmak için:
1. **Windows Defender Firewall** açın
2. **Advanced settings** seçin
3. **Inbound Rules > New Rule** seçin
4. **Port** seçeneğini seçin
5. **TCP** ve **2961** portunu belirtin
6. **Allow the connection** seçin
7. Kuralı kaydedin

## Sorun Giderme

### Servis Başlamıyor
1. Event Viewer'da hata loglarını kontrol edin
2. EXE dosyasının yolunun doğru olduğundan emin olun
3. .NET 8.0 Runtime'ın yüklü olduğunu kontrol edin
4. Lisans dosyasının (`App_Data/license.lic`) mevcut olduğundan emin olun

### Port Kullanımda Hatası
Başka bir uygulama 2961 portunu kullanıyorsa:
```cmd
netstat -ano | findstr :2961
```
Komutu ile hangi process'in portu kullandığını bulabilirsiniz.

### Servis Loglarını Görüntüleme
```cmd
wevtutil qe Application /c:50 /f:text /q:"*[System[Provider[@Name='AidatTakipApp']]]"
```

## Servis Özellikleri
- **Servis Adı:** AidatTakipApp
- **Görünen Ad:** Aidat Takip Uygulamasi
- **Başlangıç Tipi:** Otomatik (Windows başladığında otomatik başlar)
- **Varsayılan Port:** 2961
- **Protokol:** HTTP

## Erişim
Servis çalıştıktan sonra uygulamaya şu adresten erişebilirsiniz:
- `http://localhost:2961`
- `http://[SERVER_IP]:2961`
- `http://[SERVER_NAME]:2961`


