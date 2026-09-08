# Florist + Merge ekonomi incelemesi

> Bu belge önceki incelemenin bulgularını arşivler. Sonraki uygulamayla fiyatlar, açılışlar, eksik çiçekler ve birçok çalışma akışı değişti. Güncel durum için [Ekonomi modeli](EconomyModel.md) ve [güncel katalog/simülasyon](EconomySimulation.md) belgelerini kullanın.

İnceleme tarihi: 7 Eylül 2026. Kapsam: mevcut kod, sahne/prefab bağlantıları, gerçek config assetleri ve Türkçe/İngilizce isimler. Bu rapor bir Play Mode veya cihaz testi sonucu değildir.

## Sonuç

Ekonominin parçaları mevcut, fakat henüz tutarlı bir ilerleme ve harcama dengesi oluşturmuyor. En büyük sorun config yokluğu kadar **configte görünen içeriğin oyunda karşılığının bulunmaması**. Shop'taki 23 çiçeğin 19'unun Workshop tanımı yok; iki güçlendirmenin satın alma sonrası etkisi tanımlı değil. Shop'taki 62 ürünün tamamının satın alma fiyatı sıfır. Merge gerçek Florist parası kazandırıyor, fakat Shop bu parayı harcatacak şekilde dengelenmemiş.

Bu incelemede eksik genel/Merge ekonomi configleri oluşturuldu ve çalışma koduna bağlandı. Mevcut fiyatlar, müşteri ödülleri ve açılış günleri değiştirilmedi. Aşağıdaki açık bulguların tümünün giderildiği iddia edilmiyor.

## Hangi ayar nerede?

| Alan | Config / kaynak | Gerçekte kullanımı |
|---|---|---|
| İlk para, elmas, can, makine seviyesi | **Yeni:** `Assets/_Florist/_Configs/EconomyConfig.asset` | Yalnızca yeni kayıtta SaveSystem tarafından uygulanır; mevcut oyuncunun bakiyesi/seviyesi değiştirilmez. |
| Gün giriş can bedeli, ödüllü reklam canı | **Yeni:** aynı EconomyConfig | MainPage kullanır. Varsayılanlar 1 ve 1. |
| Günlük kira | **Yeni:** aynı EconomyConfig | EarningsInfo.Reset kira tutarını alır, gün sonu kârından düşer. Varsayılan 0; önceden de kira yazılmıyordu. |
| Kasada verilen fazla nakit oranı | **Yeni:** aynı EconomyConfig | Customer ödeme hesabı; varsayılan %5–25. Bu oran bahşiş değildir, para üstü işlemidir. |
| Can kapasitesi/yenilenmesi | `Assets/_Florist/_Configs/ProfileConfig.asset` | 5 can, 30 dakika. GeneralData kullanır; aşağıdaki zamanlayıcı sorunu devam ediyor. |
| Shop ürünleri/fiyatları/açılışları | `Assets/_Florist/_Configs/ShopConfig.asset` | Name, Id, Price, DefaultItemState, UnlockDay, IsSelectable. UpgradeEffect ve UpgradeValue alanları bu çalışmada eklendi. |
| Çiçek/kâğıt/kurdele birim maliyet ve satış fiyatı | `Assets/_Florist/_Configs/WorkshopConfig.asset` | DukkanPage maliyet; Customer satış fiyatı hesaplar. Shop satın alma bedelinden ayrıdır. |
| Makine süresi | Aynı WorkshopConfig → MachineInfo | BaseDuration=2 saniye; DurationGainPerLevel=0.1; alt sınır kodda 0.1 saniye. |
| Gün süresi ve müşteri/tanıtım takvimi | `Assets/_Florist/_Configs/LevelConfig.asset` | 7 tanımlı gün; 07:00–16:00, gerçek süre 5 dakika. DayTimeManager ve DukkanPage kullanır. |
| Memnuniyet ve bahşiş hesabı | `Assets/_Florist/_Configs/CustomerConfig.asset` | Varsayılan bekleme 60 saniye, bahşiş aralığı %10–50, eşik 60. Bahşişi cüzdana aktaracak adım eksik. |
| Merge enerji, kapasite, sipariş ritmi/çarpanları | **Yeni:** `Assets/_Merge/ScriptableObjects/MergeEconomyConfig.asset` | GameManager, InventoryManager ve OrderManager'a serialized referanslarla bağlandı. |
| Merge eşya seviyeleri ve üretim olasılıkları | `Assets/_Merge/ScriptableObjects/MergeItems/*.asset` | Sprite dizileri seviyeleri belirler. SpawnChances kullanımı artık MergeEconomyConfig'teki açık bir seçenekle yönetilir. |
| Merge tarif/malzeme/süre/çıktı | `Assets/_Merge/ScriptableObjects/Recipes/*.asset` | ProductionManager kullanır. İki tarifin süresi de 10 saniye. |
| Merge müşteri talebi/ödül tabanı | `Assets/_Merge/ScriptableObjects/Customers/CustomerData.asset` | Tek müşteri; papatya saksısı ister, taban ödül 125. |
| Merge üretim ürünü | `Assets/_Merge/ScriptableObjects/Productions/*.asset` | İsim/görsel kullanılır; baseValue=100 ve bazı kilit alanları ekonomik karara bağlanmıyor. |
| Reklamlar | `Assets/_Florist/_Configs/AdsConfig.asset` | Test reklamları açık; interstitial/rewarded açık, banner kapalı. Can ödülü artık EconomyConfig'te. |
| Gerçek parayla satın alma | `Assets/_Florist/_Configs/_IAPConfig.asset` | Ürün listesi ve ürün ID'leri boş; Purchaser kodu tamamen yorum satırı. Çalışan IAP ekonomisi yok. |

Yeni configler yalnızca veri içerir. UI, sahne veya hiyerarşi referansları configlere taşınmadı. WorkshopConfig'in önceden var olan prefab/Animator alanları bu incelemede yeniden yapılandırılmadı.

## Öncelikli açık sorunlar

### P1 — Shop'taki çoğu çiçek üretilemiyor

ShopConfig'te `f_1`–`f_23` olmak üzere 23 çiçek var. WorkshopConfig.FlowerInfo yalnızca `f_1`, `f_2`, `f_3`, `f_4` içeriyor: gypsophila, eucalyptus, kırmızı gül, beyaz gül.

WorkshopPage.SetAvailableFlowers satın alınan ürünü **ID ile** WorkshopConfig'te arıyor. Eşleşme yoksa kutu oluşturulmuyor. Dolayısıyla `f_5` pembe gül dahil 19 ürün satın alınsa da kullanılamaz. Pembe gülün 6. gün tanıtımında GetFlowerId da sonuç bulamayacağı için EnableNewItem akışında null erişimi riski var. Eksik ürünlerin gerçek prefabları, renkleri, kutu görselleri ve maliyet/satış verileri tamamlanmalı; yalnızca Shop satırı eklemek yetmez.

Kâğıt ve kurdeleler için durum farklı: 12/12 kâğıt ve 8/8 kurdelenin Workshop karşılığı var. Liste sıraları farklı olsa da mevcut atölye kodu ID üzerinden eşleştirdiği için bu fark tek başına hata değil.

### P1 — Tanıtım olayları birbirini eziyor

DukkanPage.StartNextEvent her tanıtımda tek `_newItemInroduceEvent` alanını değiştiriyor. Araya müşteri girince alanı null yapıyor; art arda tanıtımların yalnızca sonuncusu gün sonuna taşınıyor.

- 3. günde bahçe tanıtımı ve beyaz gül tanıtımı var.
- 6. günde kâğıt ve pembe gül tanıtımı var.

Bu yapı bir günde birden fazla açılışı güvenilir biçimde uygulayamıyor. Olayların kuyruk/liste halinde işlenmesi ve her ürünün tek kez açılması gerekir. IntroduceGarden için de ürün tanıtımlarından ayrı bir uygulama akışı yok; EndDayPage'in genel `else` dalları olayı kurdele gibi ele alabilir.

### P1 — Ücretli açılışa geçmeden tanıtım satın alması düzeltilmeli

EndDayPage.EnableNewItem doğrudan `ChangeMoney(-Price)` ve `SetPurchasedState` çağırıyor. Yeterli bakiye veya zaten sahiplik kontrolü yok. Fiyatlar sıfırken görünmeyen bu sorun, fiyat girildiğinde negatif bakiye veya tekrar tahsilat üretebilir. Animasyonlu tanıtım yolunda da unlock durumu ile yalnızca düğmeyi açma davranışı aynı şey değil; tek bir satın alma/açma işlemi kullanılmalı.

ShopItem'in normal satın alma tıklamasına bu çalışmada bakiye ve negatif fiyat kontrolü eklendi. Ancak tanıtım yolu bundan bağımsızdır ve açık sorun olarak kalıyor.

### P1 — Florist can yenileme akışı eksik

GeneralData.SetLife tam canla başlarken erken dönüyor. Daha sonra ChangeLife ile can harcanınca yenileme event aboneliği kurulmadığından aynı oturumda ilk can harcamasından sonra yenilenme başlamayabilir. Ayrıca her harcama/ödül kalan yenilenme süresini sıfırlıyor. Yenileme birikiminin korunması ve tam/dolu durum geçişlerinde aboneliğin yönetilmesi gerekiyor.

MainPage'de ayrı bir son-can hatası vardı: önce can eksiltiliyor, sonra sıfır kontrol edildiğinden oyuncu son canını kaybedip güne giremiyordu. **Bu kısım düzeltildi:** önce yeterlilik kontrol edilir, yalnızca güne girerken bedel düşülür. Sıfır giriş bedeli de desteklenir.

### P1 — Bahşiş configte var, gelire uygulanmıyor

Customer, TipPercentage hesaplıyor; DukkanPage'de gerçek Tip tutarını ekleyen satırlar yorum halinde. `EarningsInfo.Tip` bu nedenle normal akışta sıfır kalıyor. Ayrıca `AcceptableWaitTime > totalWaitTime` koşulu kısa beklemeyi “WaitedLong” sayıyor; eşitsizlik yönü beklenen davranışın tersinde.

Bahşiş tutarının hangi fiyat tabanına ve memnuniyet değerine göre hesaplanacağı netleştirilip tek tahsilat noktasına bağlanmalı. Bu incelemede yeni bir bahşiş tasarımı uygulanmadı.

### P2 — Gün numarası ve açılış etiketi farklı

ShopData.GetItemState `CurrentDayIndex >= UnlockDay` karşılaştırıyor; CurrentDayIndex sıfır tabanlı. UI ise UnlockDay değerini doğrudan gün etiketi olarak gösteriyor. Örneğin UnlockDay=1 gerçekte oyuncunun **2. günü**, UnlockDay=100 ise **101. günü** anlamına geliyor.

Alanlar ve kayıtlar yeniden numaralandırılmadı. Rapordaki katalog tablosu hem ham değeri hem oyuncu gününü ayrı gösterir. Tanıtım takvimi ve Shop açılışı tek bir kurala bağlanmalı; mevcut oyuncuların Purchased/Selected durumları korunmalı.

### P2 — Shop fiyatları ve para harcama dengesi hazırlanmış değil

23 çiçek, 12 kâğıt, 8 kurdele, 2 güçlendirme ve 17 dekorun tamamının Shop fiyatı **0**. Dekorlar da başlangıçta zaten Purchased/Selected. Konuşma balonu ve balon düğmesi listeleri boş.

Workshop maliyet/satış değerleri:

| Ürün | Kullanım maliyeti | Satış fiyatı | Birim fark |
|---|---:|---:|---:|
| `f_1` | 5 | 8 | 3 |
| `f_2` | 6 | 9 | 3 |
| `f_3` kırmızı gül | 10 | 15 | 5 |
| `f_4` beyaz gül | 10 | 15 | 5 |
| Tüm 12 kâğıt | 0 | 0 | 0 |
| Tüm 8 kurdele | 0 | 0 | 0 |

Çiçek fiyat/maliyet sorguları yalnızca FlowerType alıyor; gelecekte aynı çiçeğin renkleri farklı fiyatlandırılırsa ilk eşleşmenin bedeli kullanılacak. Shop fiyatı, atölyede her kullanım maliyeti ve müşteriye satış fiyatı ayrı kavramlardır.

### P2 — Mağaza arayüzü cache nedeniyle eski durumu gösterebilir

ShopScroll yalnızca ilk Init'te kartları kuruyor. Para veya gün değiştiğinde tüm kartları yenileyen abonelik yok. Dolayısıyla görünür fiyat butonu/lock durumu eski kalabilir. Yeni satın alma kontrolü negatif bakiyeyi engeller, ama arayüz yenileme sorununu çözmez. Boş listeler açılırsa `_items[0]` erişimi de korunmuyor; özellikle boş konuşma balonu kategorileri bağlanacaksa düzeltilmeli.

## Güçlendirmeler: isim ve gerçek etki

| ID | Mevcut isim | Fiyat/açılış | İnceleme öncesi gerçek durum | Bu çalışmadan sonraki durum |
|---|---|---|---|---|
| `u_1` | Kurye | 0; ilk günden alınabilir | Yalnızca sahiplik kaydediliyor; kurye davranışı yok. Name, localization anahtarı sanıldığı için isim boş kalabiliyordu. | `upgrade_courier` TR/EN isim anahtarı bağlı. **Etki None**; kurye sistemi hâlâ yazılmış değil. |
| `u_2` | Yaz saati | 0; ilk günden alınabilir | Sahiplik kaydediliyor; gün süresine etkisi yok. Aynı localization sorunu var. | `upgrade_summer_time` TR/EN isim anahtarı bağlı. **Etki None**; otomatik süre bonusu atanmadı. |

ShopConfig.UpgradeItems için **UpgradeEffect / UpgradeValue** alanları eklendi ve şu etkiler çalışma koduna bağlandı:

- `None`: etki yok; mevcut iki ürün böyle bırakıldı.
- `WrappingMachineLevelBonus`: sahip olunan ürünün değerini makine seviyesine ekler. Hem gerçek makine süresi hem atölyenin bekleme hesabı aynı bonusu kullanır. Tam sayı seviyeler için 1, 2… değerleri verilmeli.
- `DayDurationMultiplier`: gerçek gün süresini çarpar; örneğin 1.2, taban 5 dakikayı 6 dakika yapar. Yeni gün başlangıcında uygulanır. Birden fazla sahip olunan çarpan çarpılarak birleştirilir.

Makine normalde `MachineLevel=0` ile başlıyor. Formül `max(0.1, 2 - 0.1*(level-1))` olduğundan mevcut süre 2.1 saniye. Bu başlangıç seviyesi artık EconomyConfig'te yönetilir. Kurye ürününün neyi otomatikleştireceği ve Yaz saati bonusunun miktarı tasarım kararı gerektirir; isimlerden davranış uydurulmadı.

## Merge ekonomisi

Yeni MergeEconomyConfig'in varsayılanları: 100 başlangıç/maksimum enerji, üretici tıklamasına 1 enerji, 120 saniyede 1 enerji, 24 envanter yuvası, en çok 3 sipariş, 2 başlangıç siparişi, 30 saniyelik sipariş oluşturma aralığı; normal/VIP/nadir müşteri ödül çarpanları 1/2/3.

- Papatya ve starliçe tarifleri aynı kaynak seviyelerini ister: güneş 6, ilgili çiçek 4, toprak 5, su 5; her birinden 1 adet. Süreler 10 saniye, çıktı 1 saksı.
- Tek müşteri yalnızca papatya saksısı ister ve 125 Florist parası verir. **Starliçe saksısının mevcut sipariş havuzunda alıcısı yok.**
- Üretim ürünlerinin baseValue=100 değeri ödül hesabında kullanılmıyor; ödül müşteri tabanından geliyor. ProductionItemData.isUnlocked da tarifin kilit kontrolünün yerine geçmiyor. Tarifler RecipeData.isUnlocked ile kontrol ediliyor.
- ItemData.SpawnChances dizileri 90/10 içeriyordu ama üretici kodu seviyeyi 50/50 seçiyordu. Şimdi `UseItemSpawnChances` ile gerçek veri tabanlı seçim mümkün. **Varsayılan false**, mevcut dengeyi korur. True yapılırsa mevcut 90/10 ağırlıkları kullanılır; eksik kalan seviye ağırlıkları 0 kabul edilir.
- Envanter kapasitesi mevcut panel nedeniyle configte 1–24 aralığında tutuldu. Kapasite/sipariş limiti düşürüldüğünde kayıtlı eşyalar ve mevcut siparişler atılmaz; limitler yeni eklemeleri sınırlar.
- Dört üretim kiosku ve 9×5 tahta mevcut sahne düzenidir. Kiosk açma/satın alma, Merge giriş günü, enerji reklamı/IAP ve süre hızlandırma ekonomisi yok. Merge girişinde şu an yalnızca Florist tutorial tamamlanması aranır.
- Merge stokları Florist atölyesinin çiçek stoğuna dönüşmez. Bağlantı şu an ortak para ve sahne geçişidir. Para kaydı ile Merge PlayerPrefs kaydı farklı depolarda olduğundan iki dosya arasında sert kapanmaya karşı atomiklik garantisi yoktur.

## Diğer eksikler

- Elmas alanı, göstergesi ve ChangeDiamonds metodu var; çalışan bir kazanma/harcama döngüsü veya IAP ürün teslimi yok.
- Refund alanı muhasebe formülünde var; onu dolduran iade işlemi yok. Kira bu çalışmada config üzerinden bağlandı, varsayılan 0.
- EndDayPage.SetData kârı her çağrıda ekler; Open her çağrıda günü artırır. Yeniden yükleme/tekrar açma için gün sonu işleminin tek kez uygulanmasını garanti eden ayrı bir settlement kaydı yok. SaveData.LoadGame'in EndDay dalı veri kurmadan Open çağırdığı için bu akış özel olarak düzeltilmeli/test edilmeli.
- Rastgele müşteri üretimi sahip olunan kâğıt/kurdele/çiçekleri filtrelemiyor. İlerleme açılışları ile müşteri talebi birlikte dengelenmeli.
- Dekorlar gerçek scene görsellerine uygulanıyor; ancak fiyat/açılış dengesi hazır değil ve bazı dekor isimlerinde bozuk karakterler var.
- Yeni kayıtta başlangıç değerleri configten gelir; eski Shop kayıtlarında DefaultItemState değişikliği sahipliği sıfırlamaz. Bir ürünü sonradan pahalı/kilitli yapmak, zaten sahip olan oyuncudan ürünü geri almaz.

## Bu çalışmada yapılanlar

1. EconomyConfig ve MergeEconomyConfig assetleri/scriptleri eklendi; ilgili sahne controller'larına açık referanslarla bağlandı. Configs, yeni kayıt oluşmadan önce hazır olacak şekilde execution order aldı.
2. Genel başlangıç, can bedeli/reklam ödülü, kira ve kasada fazla nakit oranı çalışma koduna bağlandı.
3. Merge enerji/kapasite/sipariş/ödül çarpanları ve opsiyonel SpawnChances kullanımı configlere bağlandı. Limit azaltmada mevcut envanter/siparişleri koruyan yükleme yolu eklendi.
4. Güçlendirme etki alanları ve makine/gün süresi tüketicileri eklendi. Mevcut iki ürünün etki tasarımı None olarak korundu; isimleri TR/EN localization ile düzeltildi.
5. Normal Shop satın almasına yeterli bakiye/negatif fiyat kontrolü eklendi. Son canla güne girememe hatası düzeltildi.

Fiyatlar, tanıtım takvimi, eksik çiçek içerikleri, bahşiş ve kurye tasarımı bu çalışmada yeniden dengelenmedi/üretilmedi. Önce aşağıdaki sırayla işlevsel eksiklerin kapatılması önerilir:

1. Eksik Workshop ürünleri ve çoklu tanıtım akışı.
2. Can yenileme, gün sonu tek seferlik muhasebe ve güvenli tanıtım satın alması.
3. Kurye/Yaz saati davranış kararı; satın alma sonrası UI yenileme.
4. Bahşiş, kâğıt/kurdele maliyetleri, Shop fiyatları ve Merge ödüllerinin birlikte dengelenmesi.
5. Starliçe alıcısı, Merge ilerleme kilitleri ve elmas/IAP ekonomisi.

## Doğrulama

Unity 6'nın mevcut assembly referanslarıyla Roslyn semantik analizi tamamlandı: **144 C# dosyası, 366 assembly referansı, 0 hata**. 23 Unity YAML dosyası ve iki localization JSON dosyası ayrıştırıldı; yeni ekonomi assetlerinin script GUID'leri ve üç sahnedeki config bağlantıları doğrulandı. Shop dökümü 62 ürünle eşleştirildi. `git diff --check` hata vermedi.

Unity Editor, batch mode, player build veya Unity Test Framework çalıştırılmadı. Statik analiz oynanış testi yerine geçmez; gerçek satın alma, kayıt dönüşü ve cihaz oynanışı için Play Mode doğrulaması hâlâ gereklidir.

## Shop katalog dökümü

`UnlockDay` ham sıfır tabanlı değerdir. “Oyuncu günü”, kilitli ürünün yalnızca gün kontrolüyle açılabildiği ilk gündür; LevelConfig tanıtım satın alması ayrı bir yoldur. Başlangıçta Purchased/Selected olanlar için gün kapısı uygulanmaz.


### FlowerItems

| ID | Asset adı | Fiyat | Başlangıç durumu | UnlockDay | Oyuncu günü | Oyun karşılığı |
|---|---|---:|---|---:|---|---|
| f_1 | gypsum | 0 | Satın alınmış | 0 | Başlangıçtan | Var |
| f_2 | eucalyptus | 0 | Satın alınmış | 0 | Başlangıçtan | Var |
| f_3 | rose_red | 0 | Kilitli | 1 | 2 | Var |
| f_4 | rose_white | 0 | Kilitli | 3 | 4 | Var |
| f_5 | rose_pink | 0 | Kilitli | 6 | 7 | **Eksik** |
| f_6 | rose_yellow | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_7 | anemone_red | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_8 | anemone_purple | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_9 | anemone_pink | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_10 | gladiolus_white | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_11 | gladiolus_red | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_12 | gladiolus_purple | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_13 | gladiolus_pink | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_14 | gladiolus_yellow | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_15 | carnation_white | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_16 | carnation_red | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_17 | carnation_pink | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_18 | carnation_yellow | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_19 | tulip_white | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_20 | tulip_red | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_21 | tulip_pink | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_22 | tulip_yellow | 0 | Kilitli | 100 | 101 | **Eksik** |
| f_23 | daisy | 0 | Kilitli | 100 | 101 | **Eksik** |


### WrapperItems

| ID | Asset adı | Fiyat | Başlangıç durumu | UnlockDay | Oyuncu günü | Oyun karşılığı |
|---|---|---:|---|---:|---|---|
| wp_7 | wrapper7 | 0 | Satın alınmış | 0 | Başlangıçtan | Var |
| wp_9 | wrapper9 | 0 | Kilitli | 6 | 7 | Var |
| wp_1 | wrapper1 | 0 | Kilitli | 100 | 101 | Var |
| wp_2 | wrapper2 | 0 | Kilitli | 100 | 101 | Var |
| wp_3 | wrapper3 | 0 | Kilitli | 100 | 101 | Var |
| wp_4 | wrapper4 | 0 | Kilitli | 100 | 101 | Var |
| wp_5 | wrapper5 | 0 | Kilitli | 100 | 101 | Var |
| wp_6 | wrapper6 | 0 | Kilitli | 100 | 101 | Var |
| wp_8 | wrapper8 | 0 | Kilitli | 100 | 101 | Var |
| wp_10 | wrapper10 | 0 | Kilitli | 100 | 101 | Var |
| wp_11 | wrapper11 | 0 | Satın alınabilir | 0 | Başlangıçtan | Var |
| wp_12 | wrapper12 | 0 | Satın alınabilir | 0 | Başlangıçtan | Var |


### RibbonItems

| ID | Asset adı | Fiyat | Başlangıç durumu | UnlockDay | Oyuncu günü | Oyun karşılığı |
|---|---|---:|---|---:|---|---|
| r_3 | ribbon3 | 0 | Satın alınmış | 0 | Başlangıçtan | Var |
| r_5 | ribbon5 | 0 | Kilitli | 4 | 5 | Var |
| r_1 | ribbon1 | 0 | Kilitli | 100 | 101 | Var |
| r_2 | ribbon2 | 0 | Kilitli | 100 | 101 | Var |
| r_4 | ribbon4 | 0 | Kilitli | 7 | 8 | Var |
| r_6 | ribbon6 | 0 | Kilitli | 100 | 101 | Var |
| r_7 | ribbon7 | 0 | Kilitli | 100 | 101 | Var |
| r_8 | ribbon8 | 0 | Kilitli | 100 | 101 | Var |


### UpgradeItems

| ID | Asset adı | Fiyat | Başlangıç durumu | UnlockDay | Oyuncu günü | Oyun karşılığı |
|---|---|---:|---|---:|---|---|
| u_1 | Kurye | 0 | Satın alınabilir | 0 | Başlangıçtan | Etki: None |
| u_2 | Yaz saati | 0 | Satın alınabilir | 0 | Başlangıçtan | Etki: None |

### DecorationItems

| ID | Asset adı | Fiyat | Başlangıç durumu | UnlockDay | Oyuncu günü | Oyun karşılığı |
|---|---|---:|---|---:|---|---|
| decor_chandelier_standard | Standart Avize | 0 | Seçili | 0 | Başlangıçtan | Dekor seçimi |
| decor_chandelier_flower | Çiçek Avize | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_chandelier_paper | Kağıt Avize | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_chandelier_retro | Retro Avize | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_wall_yellow | Sarı Duvar | 0 | Seçili | 0 | Başlangıçtan | Dekor seçimi |
| decor_wall_green | Yeşil Duvar | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_wall_pink | Pembe Duvar | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_curtain_standard | Standart Perde | 0 | Seçili | 0 | Başlangıçtan | Dekor seçimi |
| decor_curtain_lace | Dantelli Perde | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_curtain_dark | Koyu Perde | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_shelf_standard | Standart Raf | 0 | Seçili | 0 | Başlangıçtan | Dekor seçimi |
| decor_shelf_double | Çift Raf | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_painting_board | Not Panosu | 0 | Seçili | 0 | Başlangıçtan | Dekor seçimi |
| decor_painting_flower | Çiçek Tablosu | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_trim_wooden | Ahşap Şerit | 0 | Seçili | 0 | Başlangıçtan | Dekor seçimi |
| decor_trim_green | Yeşil Şerit | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
| decor_trim_plain | Sade Şerit | 0 | Satın alınmış | 0 | Başlangıçtan | Dekor seçimi |
