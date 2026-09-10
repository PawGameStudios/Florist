# Merge referans arayüzü — 8 Eylül 2026

Kaynak: `Assets/_Merge/Sprites/_Refs` içindeki üç ekran görüntüsü.
Uygulanan sahne: `Assets/_Florist/Scenes/MergeGame.unity`. Kullanıcının ayrı `MergeGame 1.unity` kopyası değiştirilmedi.

- Üst gezinme yeni Shop/Garden/Merge ikonlarını ve enerji panelini kullanır.
- Bahçede yalnızca boş slota basmak çiçek seçimini açar. Ekili çiçek görseline basmak pencere açmaz.
- Ekim mevcut tarif girdilerini tüketir. İki sonraki büyüme adımı `RecipeData.growthStages` üzerinden ayrıca materyal tüketir; üçüncü görsel aşamaya ulaşınca şeritten hasat edilir. Geçici denge: iki adım da mevcut tarifin güneş/su/toprak seviyelerini ve adetlerini kullanır; yeniden tohum istemez. Bunlar altı tarif assetinde ayrı ayrı düzenlenebilir.
- Materyal şeritleri uygun seviyedeki toplam envanter adedini ve gereken adedi gösterir. Yeterli girdinin zemini yeşildir. Eksik girdide işlem tüketim yapmaz, yukarı kayan ve sönen uyarı gösterir.
- Büyüme artık zamana bağlı değildir. `GrowthStage` kayıtla korunur. Eski süreli kayıtlar yeniden ücret istenmeden hasada geçirilir.
- Müşteri portreleri, ürün görseli, sipariş metni ve para ödülü üst sırada düzenlendi. Çok ürünlü sipariş metni korunur.
- Envanter: üstte üç malzeme, altta altı tohum/çiçek yuvası. Her bölüm aynı sayfa düğmeleriyle sayfalanır; 24 yığın kapasitesi ve eski kapasite fazlası içerik korunur. Ürünler tohum/çiçek bölümündedir.
- Tarif seçimi bütün açık tarifleri kaydırılabilir listede gösterir; önceki/sonraki düğmeleri kaldırıldı. Her iki popup da arkadaki tıklamaları engelleyen karartma içerir.

## Doğrulama

- Roslyn, assembly üretmeden: 148 C# dosyası, 366 mevcut metadata referansı, 0 hata.
- `Tools/Merge/Check-Logic.ps1`: gerçek envanter ve üretim koduyla 27 kontrol. Ekim, eksik girdide atomiklik, iki büyüme adımı, zamana göre ilerlememe, dolu envanterde hasadı koruma ve tek seferlik hasat dahil.
- 31 Unity YAML sahne/prefab/asset dosyası ayrıştırıldı. Yerel fileID, asset GUID ve transform ebeveyn/çocuk bağlantıları doğrulandı.
- Unity Editor, Unity testleri ve build çalıştırılmadı. Görsel yerleşim ve dokunma alanları gerçek oyun içinde henüz doğrulanmadı.

## Görsel düzeltmeler

- Envanterde seed yuvaları ve başlıkları arka planın gerisinde kaldığı için görünmüyordu; çizim sırası düzeltildi. 3 malzeme ve 6 tohum/çiçek yuvası aynı anda görünür.
- Kırmızı/beyaz kapatma ikonu ve yeşil aksiyon butonu Florist kaynaklarından `Assets/_Merge/Sprites/buttons/close.png` ve `action.png` olarak yeni GUID ile kopyalandı.
- RecipeCard arka planı `recipe_white_bar` kullanır; ürün/portre görsellerinde en-boy oranı korunur. İhtiyaç metninde yalnızca mevcut/gereken adet gösterilir.
- Müşteri kartındaki sipariş metni arka planın önüne alındı; ürün görseli ortalanmış çerçeve içindedir. Ödül metninde `para-ikon.asset` ve `<sprite=0>` kullanılır.
- Enerji metni koyulaştırıldı. Merge'e ait reklam/ilerleme başlığı bahçedeki müşteri portrelerinin üzerine çizilmez.
- Doğrudan Merge sahnesiyle başlanırsa dönüş düğmesi `1_GameScene.unity` sahnesini açar. Florist üzerinden girildiğinde mevcut additive sahne geri dönüşü korunur.
- `Tools/Merge/Check-ReferenceUI.py` çizim sırası, 3+6 yerleşim, sprite bağlantıları, scroll ve transform bağlantılarını doğrular.

## Lokalizasyon ve enerji gösterimi

- `Merge Progress`, tamamlanan Merge siparişlerini ve bir sonraki ilerleme eşiğini gösterir. Aşamalar tarif ve müşteri açılışlarına bağlıdır; envanter doluluğu değildir.
- Kullanıcının kaydettiği Rewarded Energy yerleşimi korundu. InventoryPage0/1, Merge'e kopyalanmış yeşil aksiyon sprite'ını kullanır.
- Enerji etiketi yalnızca sayı ve `<sprite=0>` gösterir. `Assets/_Merge/Sprites/energy-icon.asset`, mevcut `enerji.png` üzerinden oluşturulmuştur; kullanıcının metin rengi korunur.
- InventoryWindow başlık alanı kaldırıldı. Envanter uyarıları mevcut bildirim metnine yönlendirilir; bildirim ortak Canvas'ta popup üzerinde görünür.
- 44 TR/EN anahtarı ve 11 sabit etiket komponenti eklendi. `MergeLocalizedLabel` hedef TMP referansını açıkça serialize eder; component araması yapmaz. Dinamik metinler kendi controller'larından lokalize edilir.
- Tarif ve ürün adları için ayrı localizationKey alanları kullanılır. Mevcut kayıt kimlikleri değiştirilmez.
- Doğrulama: 150 C# dosyasında Roslyn semantik analiz, 0 hata; 27 mantık kontrolü; UI ve lokalizasyon kontrolleri başarılı. Unity çalıştırılmadı.

## Son HUD düzenlemesi

- Kullanıcı isteğiyle MergeProgress ve MergeFeedbackText sahneden ve ilgili controller bağlantılarından kaldırıldı. Genel bildirim metni ve ona ait tween/call-site kodu silindi; yerine yeni bir gösterim eklenmedi. Bahçe slotuna özel eksik materyal animasyonları ayrı sistemdir ve korunur.
- Sipariş sayacı, tarif/müşteri açılma koşulları ve ilerleme kaydı korunur; yalnızca ekrandaki ilerleme etiketi kaldırıldı.
- FloristMoney, TopCanvas.prefab içindeki para göstergesinin aynı arka planı, boyutu ve TMP yazı stiliyle Inventory Canvas'a eklendi. Aynı para sprite asset'i üzerinden `<sprite=0> bakiye` gösterir ve ortak Florist para değişimini dinler.
- Roslyn semantik analiz: 150 kaynak, 0 hata. Unity çalıştırmadan UI/YAML/lokalizasyon bağlantı kontrolleri başarılı.
