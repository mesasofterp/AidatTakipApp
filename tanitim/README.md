# Spor Salonu Öğrenci Takip Sistemi - Tanıtım Web Sitesi

Bu, spor salonu öğrenci takip uygulamanız için modern ve responsive bir tanıtım web sitesidir.

## Özellikler

- ✅ Modern ve şık tasarım
- ✅ Tamamen responsive (mobil, tablet, masaüstü uyumlu)
- ✅ Smooth scroll animasyonları
- ✅ İnteraktif form validasyonu
- ✅ Scroll animasyonları
- ✅ Mobil uyumlu navigasyon menüsü
- ✅ SEO dostu yapı

## Dosya Yapısı

```
tanitim/
├── index.html      # Ana HTML dosyası
├── style.css       # CSS stilleri
├── script.js       # JavaScript fonksiyonları
└── README.md       # Bu dosya
```

## Kullanım

1. **Yerel Olarak Çalıştırma:**
   - `index.html` dosyasını bir web tarayıcısında açın
   - Veya bir local server kullanın:
     ```bash
     # Python ile
     python -m http.server 8000
     
     # Node.js ile (http-server kuruluysa)
     http-server
     ```

2. **Web Sunucusuna Yükleme:**
   - Tüm dosyaları (`index.html`, `style.css`, `script.js`) web sunucunuza yükleyin
   - Dosyalar aynı klasörde olmalıdır

## Özelleştirme

### Renkleri Değiştirme

`style.css` dosyasındaki CSS değişkenlerini düzenleyin:

```css
:root {
    --primary-color: #6366f1;      /* Ana renk */
    --primary-dark: #4f46e5;       /* Koyu ana renk */
    --secondary-color: #ec4899;    /* İkincil renk */
    /* ... */
}
```

### İçerik Düzenleme

`index.html` dosyasını açın ve şu bölümleri düzenleyin:

- **Hero Section:** Ana başlık ve açıklama
- **Features Section:** Özellikler listesi
- **Contact Section:** İletişim bilgileri
- **Footer:** Alt bilgiler

### İletişim Formu

Şu anda form sadece frontend validasyonu yapıyor. Gerçek form gönderimi için:

1. Backend API endpoint'i oluşturun
2. `script.js` dosyasındaki form submit handler'ını güncelleyin
3. AJAX/Fetch ile form verilerini backend'e gönderin

Örnek:

```javascript
contactForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const formData = new FormData(contactForm);
    
    try {
        const response = await fetch('/api/contact', {
            method: 'POST',
            body: formData
        });
        
        if (response.ok) {
            showMessage('Mesajınız başarıyla gönderildi!', 'success');
            contactForm.reset();
        }
    } catch (error) {
        showMessage('Bir hata oluştu. Lütfen tekrar deneyin.', 'error');
    }
});
```

## Bölümler

1. **Hero Section:** Ana başlık, açıklama ve istatistikler
2. **Features Section:** Uygulamanın özellikleri
3. **How It Works:** Nasıl çalıştığına dair adımlar
4. **Contact Section:** İletişim formu ve bilgileri
5. **Footer:** Hızlı linkler ve sosyal medya

## Tarayıcı Desteği

- Chrome (son 2 versiyon)
- Firefox (son 2 versiyon)
- Safari (son 2 versiyon)
- Edge (son 2 versiyon)

## Notlar

- Font Awesome ikonları CDN üzerinden yükleniyor (internet bağlantısı gerekli)
- Tüm animasyonlar CSS ve vanilla JavaScript ile yapılmıştır
- Harici kütüphane bağımlılığı yoktur (Font Awesome hariç)

## Lisans

Bu tanıtım sitesi projeniz için özelleştirilmiştir.

