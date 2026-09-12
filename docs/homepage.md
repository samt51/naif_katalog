# Ana sayfa

- Müşteri girişinden sonra `/Home/Welcome` açılır. Ürün listesi ve filtreleri `/Home/Index` adresinde devam eder; logo ana sayfaya döner.
- Yeni eklenenler API'den `pageSize=12&columnIndex=0&orderBy=desc` ile alınır (en yüksek ürün ID'si önce). Fiyatlar mevcut müşteriye özel hesaplama ve B2B/B2C gösterimiyle çalışır.
- Ana kategoriler `ParentId == 0` ile seçilir. Masaüstünde dört sütun, mobilde iki sütun; kartların oranı 5:8'dir.
- Yönetim paneli → **Ana Sayfa İçerikleri** (`/HomeContent/Index`) üzerinden her ana kategori için JPG/PNG/WebP yüklenir. Öneri: 1000 × 1600 piksel, en fazla 8 MB. Fotoğraf gelene kadar Naif alanı gösterilir.
- Bülten formu abonelikleri kaydeder, e-posta göndermez. Aboneler aynı yönetim ekranında görünür. Tekrarlanan e-posta adresleri ikinci kayıt oluşturmaz.

## Kalıcı içerik

Kategori görselleri `wwwroot/images/categories/`, abonelikler web kökü dışındaki `App_Data/newsletter.json` altında saklanır. Mevcut ürün görsellerinin yerel dosya yaklaşımı kullanılır; KatalogApp şemasında değişiklik yapılmadı. Dağıtım sırasında bu iki dizin korunmalı ve uygulama kullanıcısı tarafından yazılabilir olmalıdır. Birden fazla uygulama instance'ı kullanılacaksa dosya deposu ortak veritabanı/nesne deposuna taşınmalıdır.

Yerel testte yeni 12 ürünün eksik ana fotoğrafları mevcut Naif katalog sitesinden `wwwroot/images/katalog/` dizinine alındı. Bunlar mevcut ürün görseli kurallarına göre kaynak kontrolü ve yayın paketi dışındadır.

## Ana görsel

Dosya: `wwwroot/images/home/naif-hero.png` (1774 × 887, 2:1). Yerleşim, ekranın kalan yüksekliğini kaplar ve fotoğrafı `object-fit: cover` ile kırpar. Görsel yayın paketine dahildir.

Üretim: yerleşik `image_gen` aracı; CLI kullanılmadı. Kullanılan istem:

> Use case: ads-marketing. Asset type: wide full-bleed hero photograph for Naif fine jewellery B2B homepage, 2:1 landscape composition. Primary request: original premium jewellery campaign photograph, elegant adult brunette woman in ivory silk wearing refined diamond drop earrings and a slender diamond ring, hand gently near collarbone, cropped editorial portrait from lips to upper torso with jewelry clearly visible. Warm ivory and taupe studio backdrop, soft daylight, realistic skin and diamonds, understated timeless luxury, magazine photography. Composition with subject centered, generous background on both sides, enough quiet space across lower-middle area for a white headline that will be added in HTML. No typography, no logos, no watermarks. Not a copy of any existing advertisement. Save output image and return local path for use in website.

## Doğrulama

`dotnet build --no-restore` ve `node --check wwwroot/js/home.js` başarılı. Yerel Chromium ile 1600 px / 390 px görünüm, yatay taşma, slider, yedi ana kategori, bülten kaydı/tekrar kaydı, geçersiz dosya reddi, görsel yükleme/gösterme ve CSRF kontrolleri doğrulandı. Test için geçici yerel kimlik kullanıldı; gerçek müşteri parolasıyla giriş denenmedi. Test aboneliği ve test kategori görseli temizlendi.
