# ATASAY müşteri frontend’i

ASP.NET Core 9 MVC uygulaması. Müşteri girişi, ana sayfa, katalog, ürün detayı,
sepet/PDF ve müşterinin kendi sipariş ekranları içerir. Yönetim ekranları,
kategori/ürün düzenleme endpoint’leri bu uygulamada bulunmaz.

## Çalıştırma

Repo kökünden:

```sh
dotnet run --project frontends/atasay_katalog/atasay_katalog.csproj --launch-profile Atasay
```

Adres: http://localhost:5208. Gerçek backend hesabıyla giriş yapılır.
Varsayılan API adresi mevcut ortak API’dir. Backend ve veritabanı değişikliği yoktur.
`Atasay.Customer` ve `Atasay.ApiToken` çerezleri ile sepet anahtarları bu uygulamaya özeldir.

## Ayrı yayın

```sh
dotnet publish frontends/atasay_katalog/atasay_katalog.csproj -c Release -o /tmp/atasay-publish
```

Çıktıyı `b2bgifty.com` için ayrı IIS sitesi/uygulama havuzu ve ayrı fiziksel dizine
koyun. Domain binding ve TLS sertifikasını bu siteye bağlayın. ASP.NET Core 9
Hosting Bundle gerekir. SDK publish işlemi uygulamaya özel web.config üretir.
Mevcut sitenin yayın klasörünün üzerine yüklemeyin.

`ApiAdress` ortak backend adresidir. `CatalogImageBaseUrl` ortak katalog fotoğraf
kaynağıdır; tarayıcı bu resimleri aynı origin’deki doğrulamalı `CatalogMedia/Image`
üzerinden alır. Bu iki teknik adresteki mevcut marka alan adı arayüzde gösterilmez.
Ortam değişkenleriyle ayarlar değiştirilebilir. `AllowedHosts` yeni domain ve
localhost ile sınırlıdır. Gerçek backend’in geçerli TLS sertifikası olmalıdır.

Mevcut frontend’in müşteri sipariş saklama davranışı korunmuştur; üretim verileri
kopyalanmamıştır. Yeni uygulamanın kendi `App_Data` dizini çalışma sırasında oluşur.
Bu dizin yayın paketine dahil edilmez ve sonraki yayınlarda korunmalıdır.
Sipariş bildirim e-postası için Atasay’a ait `Email__Host`, `Email__Port`,
`Email__EnableSsl`, `Email__User`, `Email__Password`, `Email__From`,
`Email__OrderNotifyTo` sunucuda ayarlanmalıdır. Mevcut sitenin SMTP kimlik bilgileri
bu projeye taşınmamıştır. Ayarsız durumda sipariş saklanır; bildirim gönderilemez.

## Tasarım

- Görsel tema: `wwwroot/css/atasay.css`
- Giriş: `Views/Account/Login.cshtml`
- Ana sayfa: `Views/Home/Welcome.cshtml`
- Ortak navigasyon, footer ve sepet: `Views/Shared/_Layout.cshtml`
- Yerel resmi Atasay marka görselleri: `wwwroot/images/brand`; kaynaklar `ASSETS.md`.
- Ürünler/fiyatlar backend’den gelir; demo ürün üretim uygulamasına eklenmez.

## Yerel testler

```sh
python3 frontends/atasay_katalog/tests/check_boundary.py
dotnet run --project frontends/atasay_katalog/tests/Ownership/Ownership.csproj
dotnet build frontends/atasay_katalog/atasay_katalog.csproj
```

Üretim API’sine bağlanmadan tarayıcı testi için ayrı terminallerde:

```sh
python3 frontends/atasay_katalog/tests/fixture_api.py
ApiAdress=http://127.0.0.1:5210/ CatalogImageBaseUrl=http://127.0.0.1:5210/images/katalog/ dotnet run --project frontends/atasay_katalog/atasay_katalog.csproj --launch-profile Atasay
npm install --prefix /tmp/atasay-browser playwright
PLAYWRIGHT_MODULE=/tmp/atasay-browser/node_modules/playwright node frontends/atasay_katalog/tests/browser.cjs
```

Gerekirse `CHROMIUM_PATH` ile tarayıcı executable adresini belirtin. Test yalnızca
localhost fixture hesabını kullanır; gerçek sipariş ya da e-posta göndermez.
Ekran görüntüleri `/tmp/atasay-*.png` dosyalarına kaydedilir. Test ve fixture
kaynakları publish paketine alınmaz. Fixture API’yi internete açmayın.
