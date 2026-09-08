# Merge ilerleyişi, enerji reklamı ve bahçe incelemesi

7 Eylül 2026. Bu belge Merge için önceki entegrasyon/ekonomi notlarının güncel devamıdır.

## Uygulanan oyun döngüsü

Tahtada malzeme üret → birleştir → dokunarak envantere al → üretim alanında tarif yap → ürünü topla → müşteri siparişini teslim et. Teslimatlar kalıcı Merge ilerlemesini artırır; yeni tarifler ve müşteri profilleri açılır. Florist parası ortak cüzdana aktarılır. Florist'in günlük malzeme açılışları hâlâ kendi mevcut gün/para kurallarıyla çalışır; bu aşamada bahçeye bağlanmadı.

Envanterde normal malzemeye dokunmak bir adedini boş tahta hücresine geri koyar. Hazır üretim ürünleri müşterilere teslim edilir. Tahtada yer yoksa envanter eksilmez. Envanter doluysa toplama ürünü silmez ve envanter uyarısı açılır; aynı yığına ekleme yapılabilir. Eski kayıtlarda kapasite fazlası varsa yüklemede korunur. Tarifler minimum seviye ister, uygun en düşük seviyeli malzemeleri önce tüketir; yüksek seviyeli malzeme de kullanılabilir. Arayüzde `Sv.3+` bunu belirtir.

## Yönetim noktaları

| Konu | Dosya / alan |
|---|---|
| Enerji, yenilenme, reklam ödülü ve sınırı, müşteri sıklığı | `Assets/_Merge/ScriptableObjects/MergeEconomyConfig.asset` |
| İlerleme eşikleri, siparişte tür/adet sınırları | `Assets/_Merge/ScriptableObjects/MergeProgressionConfig.asset` |
| Müşteri adı, portre, ürün tercihleri, açılma eşiği, seçilme ağırlığı | `Assets/_Merge/ScriptableObjects/Customers/*.asset` |
| Ürün başına para ve açılma eşiği | `Assets/_Merge/ScriptableObjects/Productions/*.asset`: `baseValue`, `minimumCompletedOrders` |
| Malzeme türü/seviyesi/adedi ve üretim süresi | `Assets/_Merge/ScriptableObjects/Recipes/*.asset` |
| Reklam SDK test modu ve reklam birimleri | `Assets/_Florist/_Configs/AdsConfig.asset` |
| Sahne ve UI bağlantıları | `Assets/_Florist/Scenes/MergeGame.unity`, `Assets/_Merge/Prefabs` |

Müşteri assetindeki eski `baseReward` alanı artık yeni sipariş hesabının kaynağı değildir. Yeni ödül = istenen her ürünün `baseValue × adet` toplamı × müşteri tipi çarpanı. Mevcut altı profil Regular, çarpan 1. Eski kayıtlı siparişlerin gösterilmiş ödülü korunur.

## Müşteri ve ürün ilerleyişi

İlerleme Florist günü değil, tamamlanan Merge siparişi sayısıdır.

| Teslimat sayısı | Aşama | Sipariş üst sınırı |
|---|---|---|
| 0–3 | Yeni yetiştirici | 1 tür, 1 adet |
| 4–9 | Saksı ustası | 1 tür, 2 adet |
| 10–19 | Çiçek atölyesi | 2 tür, toplam 2 adet |
| 20–34 | Koleksiyoncu | 2 tür, tür başına 2, toplam 3 adet |
| 35+ | Sipariş ustası | 2 tür, tür başına 3, toplam 4 adet |

Bunlar üst sınırdır; müşteri tercihi ve rastgele adet seçimi daha kolay siparişler de üretir. İlk aşamada ev bitkisi meraklısı, papatya koleksiyoncusu ve starliçe meraklısı gelir. 4 teslimatta çiçekçi, 10'da etkinlik organizatörü, 20'de botanik koleksiyoncusu eklenir. Son üç profil karışık sipariş verebilir. Müşteri başına ürün havuzu farklıdır. Yeni açılan ürün seviyesini seçmeye %60 eğilim vardır; aynı seviyedeki ürünler arasında rastgele seçim yapılır. Kilitli veya tarifi olmayan ürünler yeni siparişte istenmez.

| Ürün | Açılış | Birim ödül | Malzeme gereksinimi |
|---|---|---|---|
| Papatya fidesi | 0 teslimat | 20 | Güneş Sv.3; papatya Sv.2; su Sv.2; toprak Sv.2, birer adet |
| Starliçe fidesi | 0 teslimat | 25 | Güneş Sv.3; starliçe Sv.2; su Sv.2; toprak Sv.2, birer adet |
| Papatya saksısı | 4 teslimat | 125 | Güneş Sv.6; papatya Sv.4; su Sv.5; toprak Sv.5, birer adet |
| Starliçe saksısı | 4 teslimat | 125 | Güneş Sv.6; starliçe Sv.4; su Sv.5; toprak Sv.5, birer adet |
| Papatya koleksiyonu | 10 teslimat | 220 | Papatya saksısı tarifindeki seviyelerin her birinden 2 adet |
| Starliçe koleksiyonu | 10 teslimat | 240 | Starliçe saksısı tarifindeki seviyelerin her birinden 2 adet |

Altı tarif de mevcut 10 saniyelik üretim süresini kullanır. Eski iki saksı tarifinin gereksinimleri ve 125 ödülü korundu; yeni katmanlar bunlardan türetildi. Koleksiyon görselleri mevcut büyüme görsellerini kullanır. Tarif penceresi üçlü sayfalara ayrıldı. İlk iki sipariş başlangıçta gelir, açık sipariş sınırı 3, yeni teklif aralığı 30 saniyedir. Bu süre gelir garantisi değildir; siparişi üretip teslim etmek gerekir.

Eski Merge sipariş kaydı olup ilerleme anahtarı olmayan oyuncu 4 teslimat seviyesinden geçirilir; mevcut saksı istekleri kilitlenmez. Eski ürün/tarif kimlikleri değişmedi. Yeni çok kalemli siparişler bütün kalemleriyle saklanır. Eksik ürün varsa teslimat hiçbir ürünü tüketmez.

## Enerji reklamı

- İsteğe bağlı ödül: **20 enerji**, enerji kapasitesini aşmadan.
- Başarılı reklamlar arasında **60 saniye**, **UTC takvim gününde 5** ödül sınırı. Florist oyun günüyle karıştırılmamalı.
- Enerji dolu, reklam hazır değil, günlük sınır dolu veya bekleme sürerken düğme kullanılamaz.
- Ödül, reklam SDK'sının ödül callback'inden bir kez gelir. Erken kapanma/gösterim hatası ödül ve kullanım hakkı tüketmez. Ortak `AdManager` callback'i düzeltildiği için diğer rewarded kullanım yerleri de bu başarı tanımını kullanır.
- Kullanım sayısı ve bekleme zamanı `Florist.Merge.EnergyAd.*` anahtarlarında, enerji mevcut Merge kaydında saklanır.
- Mevcut **test reklamı ayarı açık** bırakıldı. Bu çalışma gerçek reklam geliri doğrulaması veya yayın ayarlarına geçiş değildir.

Doğal enerji dengesi aynı: kapasite/başlangıç 100, üretici tıklaması 1 enerji, 120 saniyede 1 yenilenme. Reklam enerjiye ek bir kaynak verir; üretim süreleri reklam gerektirmek için uzatılmadı. Merge gelirleri Florist'in reklamsız temel ekonomi simülasyonuna zorunlu gelir olarak eklenmedi.

## Bahçede mevcut olan ve eksik olan

Hem kaynak Merge-main hem entegre projede bahçe Canvas'ı, bahçeye geçiş, dört üretim alanı, tariften süreli üretim ve üç aşamalı büyüme görselleri var. Florist `LevelConfig.SpecialEvents` içinde `IntroduceGarden` enum girdisi de var; bu girdinin çalışan bir bahçe döngüsü uygulaması bulunamadı.

Şunlar yok: kalıcı ekili parsel modeli, ekim bedeli/ürün tüketimi, bakım zamanları ve bakım aksiyonları, bakım sonucuna göre hasat, hasattan Florist çiçek unlock'una bağlantı. Mevcut üretim alanı bitince toplanabilir ürün verir ama periyodik bakım gerektirmez. Dolayısıyla bu temel yeniden kullanılabilir; tamamlanmış bir bahçe sistemi değildir.

Bahçe tasarımı netleşene kadar bu kurallar eklenmedi. Sonraki tasarımda ekim girdisi, bakım sıklığı, kaçırılan bakımın sonucu, hasadın stok mu kalıcı unlock mu verdiği ve hangi Florist çiçek ID'lerini açacağı belirlenmeli. Merge ilerleme, ürün tüketme ve kayıt altyapısı bu bağlantı için kullanılabilir; mevcut para katkısının dışında ana oyun unlock katkısı henüz yok.

## Doğrulama ve kalan cihaz kontrolü

- Unity çalıştırmadan Roslyn semantik analiz: 147 C# dosyası, 366 referans, 0 hata.
- `Tools/Merge/Check-Logic.ps1`: gerçek envanter/sipariş/ilerleme kodu üzerinde Unity dışı stand-in bağımlılıklarla 18 kontrol geçti. Atomik tüketim, tekrar teslimat, seviye/kapasite, taşma, eski fazla stok ve ilerleme eşikleri kapsanır.
- 32 Merge sahne/prefab/config YAML dosyası ayrıştırıldı. Yerel fileID ve asset/package GUID bağlantıları, altı benzersiz müşteri/ürün/tarif bağlantısı doğrulandı.
- Unity Editor, Unity Test Framework ve player build çalıştırılmadı. Bu kontroller görsel yerleşimi, Unity yaşam döngüsünü veya gerçek AdMob akışını kanıtlamaz.
- Cihaz/Editor kontrolünde Florist'ten Merge'e giriş, alt seviye toplama/geri koyma, dolu tahta/envanter, tarif sayfaları, 4/10 teslimat açılışları, çok kalemli teslimatın kayıt sonrası korunması ve başarılı/erken kapanan/hatalı reklam denenmeli. Reklam ödülü ve envanter/sipariş ilerlemesi sahneden çıkıp yeniden girildiğinde korunmalı.
