# Florist + Merge — rahat ilerleme ekonomisi

7 Eylül 2026. Uygulanan ilk denge sürümü. Ayrıntılı 62 ürün tablosu ve configlerden hesaplanan gelirler: [Ekonomi simülasyonu](EconomySimulation.md).

## Hedef ve başlangıç verileri

Para günlük ilerlemenin engeli olmamalı. Florist kendi günlük açılışlarını karşılar; Merge ekstra alışveriş rahatlığı sağlar. Can ve isteğe bağlı ödüllü reklam modeli için temel oyun yavaşlatılmadı. Başlangıç parası 10, can kapasitesi 5, gün bedeli 1 can, can yenilenmesi 30 dakika, kira 0 olarak korundu.

Projede gerçek birim maliyet/satış dayanakları vardı: cipso **5/8**, okaliptüs **6/9**, kırmızı gül ve mevcut pembe anemon **10/15**. Shop satın alma fiyatlarının tamamı ise 0 idi; sonraki mağaza fiyatlarının tarihsel bir başlangıç fiyatından geldiği iddia edilmiyor. Yeni Shop bedelleri bu birim marjlar ve gerçek ilk hafta müşteri siparişleriyle hesaplanan kazanç üzerinden belirlendi.

İlk incelemedeki `f_4` açıklamasına düzeltme: mağazada beyaz güldü, Workshop'taki aynı ID gerçekte pembe anemon görseli/türü/prefabına bağlıydı. Pembe anemon mevcut prefabıyla doğru `f_9` ID'sine taşındı; beyaz gül için gerçek beyaz gül assetleri bağlandı. 19 eksik çiçek varyantı mevcut görsellerden tamamlandı. Tezgâhın altı yuvalık sınırı da 23 açık prefab referansı ve genişleyen yatay alanla kaldırıldı.

## Takvim

Gün, **oyun günü** anlamındadır; cihazın takvim günü veya günlük giriş ödülü değildir. Oyuncu bir oturumda birden fazla gün tamamlayabilir.

| Oyuncu günü | Yeni malzeme | Tek seferlik Shop bedeli |
|---:|---|---:|
| 1 | Başlangıç: cipso, okaliptüs, kahverengi kâğıt, pembe kurdele | 0 |
| 2 | Kırmızı gül | 10 |
| 3 | Sarı kâğıt | 10 |
| 4 | Beyaz gül | 20 |
| 5 | Kırmızı kurdele | 10 |
| 6 | Pembe kâğıt | 15 |
| 7 | Pembe gül | 25 |
| 8 | Mor kurdele | 15 |
| 9 | Papatya | 20 |

2–40. günlerin her birinde tam **bir yeni çiçek/kâğıt/kurdele** açılır: başlangıç dışındaki 39 malzeme. Eski ilk hafta tanıtımları korundu; 7. günde aynı anda açılan pembe kâğıt ve pembe gül ayrıldı, kâğıt boş olan 6. güne çekildi. Başka boş gün olan 3. güne sarı kâğıt eklendi. 7. günün hikâye siparişleri her iki malzemeye de erişebilir.

41–51. günlerde 11 dekor alternatifi, 52. günde Yaz saati açılır. Başlangıçtaki altı seçili dekor ücretsiz kalır. Dekorlar kendi dekor ekranında, güçlendirme Shop'ta gün etiketiyle görünür. Otomatik gün sonu odak/tanıtım akışı malzemeler içindir; sonraki dekorlar için yeni bir tanıtım animasyonu eklenmedi.

`ShopConfig.UnlockDay` sıfır tabanlı kalır: oyuncu günü = UnlockDay + 1. Shop/dekor etiketleri bunu doğru gösterir. Ürünler yeni güne geçişte erişilebilir olur; yalnızca oyuncunun satın alma eylemi para düşürür. Gün sonu eskisi gibi kendiliğinden ürün satın almaz. Bir ürün alınmazsa sonraki günlerde alınabilir; ilerleme durmaz. Rastgele siparişler yalnızca sahip olunan malzemelerden oluşur. Hikâye siparişi elde olmayan bir ürün istiyorsa veya boş içerik taşıyorsa, o ziyaret için mevcut genel konuşmayla üretilebilir bir sipariş oluşturulur; config müşteri asseti değiştirilmez.

## Fiyatlar

Shop bedeli ürün çeşidini bir kere açar. Workshop maliyeti her bukette kullanıma göre gün sonu muhasebesine yazılır. Satış fiyatı müşterinin hesabına eklenir. Başlangıç malzemeleri sınırsız kullanılır; stok satın alma zorunluluğu eklenmedi.

| Çiçek ailesi | Birim maliyet | Birim satış |
|---|---:|---:|
| Cipso | 5 | 8 |
| Okaliptüs | 6 | 9 |
| Papatya | 7 | 11 |
| Gül | 10 | 15 |
| Anemon | 10 | 15 |
| Karanfil | 9 | 14 |
| Lale | 11 | 17 |
| Glayöl | 12 | 18 |

Aynı ailenin renkleri aynı birim değerdedir; çalışma kodu artık tür **ve renk** ile sorgular, ileride farklı renk fiyatları tanımlanabilir.

Kâğıt maliyeti 2–4, satış fiyatı maliyet +2. Kurdele maliyeti mevcut takvimde 1–2, satış fiyatı maliyet +1. Böylece ilk gün buket başına ambalaj 3 maliyet / 6 satış yaratır. Çiçeklerin önceki marjına küçük ve tutarlı bir katkı eklenir.

Yeni malzeme Shop bedelleri **10–50** aralığındadır. Çiçekler için taban `15 + 5 × floor((oyuncuGünü - 2) / 5)`, kâğıt/kurdele için `10 + 5 × floor((oyuncuGünü - 2) / 10)` kullanıldı. Erken hikâye ürünleri yukarıdaki küçük bedellere sabitlendi; papatya 20. Bunlar assetlere yazılmış somut fiyatlardır; çalışma anında oyuncunun bakiyesine göre fiyat değişmez. Dekorlar 40–90, Yaz saati 150.

## Memnuniyet ve bahşiş

`CustomerConfig.DefaultOrderHappiness`:

- Başlangıç değerlendirme puanı 50; doğru sipariş +40; farklı sipariş −30; eksik çiçek −20; fazladan çiçek +0.
- Birden fazla bukette içerik puanı ortalanır; buket sayısını artırarak memnuniyet katlanmaz.
- Servis için tolerans **90 saniye**. Atölyeye geçişten teslimata kadar ölçülür; kasada geçen süre ve uygulama kapalıyken geçen süre buna eklenmez. Aşım −10 puandır. Süre kayıt dönüşünde korunur.
- Son puan 0–100'e sıkıştırılır. Doğru sipariş, eksiksiz içerik ve doğru para üstüyle puan ≥60 ise bahşiş verilir.
- Config aralığı %10–20; puan 60'tan 100'e yükseldikçe doğrusal artar. Normal doğru servis 90 puan / **%17,5**, yavaş doğru servis 80 puan / **%15** getirir. Mevcut varsayılan içerik puanlarıyla 100 puan doğal olarak oluşmaz; üst uç gelecek özel müşteri ayarları için tanımlıdır.
- Bahşiş yalnızca o müşterinin sipariş fiyatından hesaplanır, tam paraya yuvarlanır ve gün sonu Tip hanesine bir kez eklenir. Günün birikmiş cirosuna veya uzatılan banknota uygulanmaz. Yanlış para üstü ya da eksik/yanlış sipariş bahşiş getirmez; ayrıca para cezası eklenmedi.

Bekleme karşılaştırmasının yönü, kullanılmayan servis zamanlayıcısı, gün cirosundan bahşiş hesaplama riski ve memnuniyet göstergesinin teslimat puanından kopukluğu giderildi.

## Kasadaki fazla nakit

`EconomyConfig.CustomerExtraCashFraction = (0.05, 0.25)` müşteri banknotunu çeşitlendirir. Önce hesap üstüne %5–25 eklenir, sonra mevcut banknotlarla ödenebilir tutara yukarı yuvarlanır. Dolayısıyla son banknot, yüzde aralığının biraz üstüne çıkabilir.

Örnek: sipariş 40, rastgele oran %10 ise hedef 44, uzatılan para 45, doğru para üstü 5 olur. Ciro yine **40**'tır. %17,5 bahşiş varsa ayrıca 7 eklenir. Fazla nakit oranı ekstra gelir veya bahşiş ayarı değildir. Kasa ekranına kayıt dönüşünde de aynı banknot korunur.

## Merge ve gelir beklentisi

Merge artık teslimat sayısına bağlı fide → saksı → koleksiyon ilerleyişi kullanır. Birim ödüller fide 20/25, saksı 125, koleksiyon 220/240; çok kalemli siparişlerde toplam ürün değeri ödenir. Eski saksı tarifleri ve 10 saniyelik üretim süreleri korunmuştur. Enerji kapasitesi 100, yenilenme 120 saniye, üretici bedeli 1; isteğe bağlı reklam 20 enerji verir (60 saniye ara, UTC gününde 5).

Ayrıntılı müşteri tercihleri, ilerleme eşikleri, configler ve bahçe incelemesi: [Merge ilerleyişi](MergeProgressionAndGarden.md). Bu ek gelir Florist'in temel gün/para dengesini karşılamak için zorunlu değildir; aşağıdaki simülasyon hâlâ reklamsız ve Merge gelirsiz baz senaryodur.

Simülasyonda Merge/reklam geliri olmadan:

- Normal doğru servis: ilk gün **62**, 2–7. günlerde yaklaşık **103–162**, ilerleyen günlerde yaklaşık **116–144** net para.
- Her günlük ürün alınmasına rağmen 52. günde yaklaşık **5.039** para birikir. Bilerek rahat bir para dengesi kuruldu.
- Planlı müşterilerin %60'ı servis edilir, bahşiş verilmezse bile 1.000 tohumun hiçbirinde günlük alım gecikmez; 52. gün yaklaşık **944** para kalır.

Bu sonuçlar doğru para üstü ve hazırlanıp atılmayan buket varsayımına dayanır. Oyun içi beceri, yarım kalan siparişler, görsel etkileşim süresi veya gerçek kullanıcı davranışı ölçülmedi.

## Güçlendirme, can ve reklam kapsamı

Yaz saati 52. günde 150 para, gün süresi ×1,2 olarak bağlandı: 5 dakika → 6 dakika. Daha fazla müşteri eklemez; mevcut müşteri akışını tamamlamak için ek zaman verir. Kurye davranışı hazır olmadığı için **Yakında / PurchaseDisabled**; etkisiz bir ürüne para alınmaz.

Makine seviyesi ve mevcut yaklaşık 2,1 saniyelik işlem süresi korundu. Ücretli kalıcı hız bonusu atanmadı. Ödüllü makine hızlandırma reklamı ve gerçek para ile can satışı **henüz entegre değildir**. Bu çalışma yeni IAP ürünleri, mağaza fiyatları veya reklam düğmeleri eklemez. Mevcut tek makine işlemi zaten kısa olduğu için ileride reklam ödülünü tek işlem yerine bir süre/oyun günü boyunca hız bonusu olarak tasarlamak daha anlamlı bir seçenek olabilir; bu henüz uygulanmış bir davranış değildir.

Can yenileme zamanlayıcısı ilk harcamada başlar, sonraki harcamalar mevcut süreyi sıfırlamaz; kapalı geçen zamanda dolan canlar geri dönüşte hesaplanır. Gün sonu aynı kayıtlı gün için iki kere kâr/gün artışı yapmaz. Genel para/Shop/oyun durumu hâlâ ayrı dosyalardadır; süreç tam dosya yazımları arasında zorla kapatılırsa dosyalar arası atomiklik garantisi yoktur.

Eski oyuncunun Purchased/Selected durumları korunur. Yeni fiyat ve başlangıç kilitleri eski sahipliği geri almaz; bu nedenle 52 günlük tam açılış takvimi yeni kayıt için geçerlidir. Eski sürümde tam kasa ekranında alınmış kayıtta müşterinin uzattığı banknot tutulmadığı için o kaydın özgün banknotunu geri çıkarmak mümkün değildir; yeni kayıtlarda makbuz alanları saklanır.

## Yönetim ve doğrulama

- `Assets/_Florist/_Configs/ShopConfig.asset`: tüm Shop bedelleri, günler, başlangıç sahipliği ve güçlendirmeler.
- `Assets/_Florist/_Configs/WorkshopConfig.asset`: 23 çiçek, 12 kâğıt, 8 kurdele; birim maliyet/satış ve mevcut prefab bağlantıları.
- `Assets/_Florist/_Configs/CustomerConfig.asset`: memnuniyet/bahşiş/tolerans.
- `Assets/_Florist/_Configs/EconomyConfig.asset`: başlangıç, kira, can bedelleri, fazla nakit; rastgele siparişte 3 çiçek ve sahip olunanlar içinde en ileri açılış gününe sahip çiçeğe %50 öncelik.
- `Assets/_Merge/ScriptableObjects/MergeEconomyConfig.asset`: Merge enerji/sipariş ritmi. İlerleme `MergeProgressionConfig`, ürün bedelleri Productions, müşteri tercihleri Customers assetlerinden gelir.

Simülasyonu güncel configlerle tekrar çalıştırmak için proje kökünde:

```powershell
./Tools/Economy/Export-Configs.ps1
python Tools/Economy/simulate.py
```

145 C# dosyası / 366 Unity assembly referansıyla Roslyn semantik analizi: **0 hata**. 52 Unity YAML dosyası ayrıştırıldı. 23 çiçeğin tür, renk, sprite, kutu görseli ve prefab component ID'leri; 23 tezgâh konumu doğrulandı. Config simülasyonunda 2.000 × 52 günlük senaryo başarılı. Unity Editor, batch mode, Test Framework veya player build çalıştırılmadı; yeni geniş tezgâhın görsel yerleşimi ve gerçek satın alma/kayıt dönüşü Play Mode'da ayrıca doğrulanmalı.
