# Merge etkileşim animasyonları

## Uygulanan davranışlar

- Üretici basışı: item görseli 60 ms'de %94 ölçeğe iner, bırakınca 100 ms'de geri döner. Hücrenin tıklama alanı sabit kalır.
- Üretici çıkışı: yeni item grid üzerinde ayrı bir prefab görseliyle hedefe gider. Cell prefab Inspector'ındaki Producer Item Flight bölümünden süre, Ease, yay yüksekliği ve başlangıç ölçeği ayarlanır. Başlangıç değerleri 350 ms, OutSine, 24 UI birimi ve %80 ölçektir. Yay yüksekliği 0 yapılırsa düz hareket eder.
- Merge: sonuç hedefe otururken %90 → %108 → %100 ölçek vurgusu yapar. Sabit hedef açıldığında buna kısa, yumuşak renk geçişi eklenir.
- Taşıma, yer değiştirme ve geçersiz bırakma: görseller hücrelerine kısa hareketle oturur. Geçersiz hedefte küçük yatay tepki oynar.
- Sürükleme: taşınan görsel %4 büyür. Pointer altındaki birleşebilir hedef rengi MergeEconomyConfig.MergeTargetCellColor ile ayarlanır; varsayılan alpha 0 olduğundan kapalıdır. Maksimum seviyedeki veya uyumsuz hedef vurgulanmaz.
- Kalıcı hücre arka planı yalnızca sabit olmayan, son seviyeye ulaşmış item'larda görünür. MergeEconomyConfig.MaxLevelCellColor ile ayarlanır. Üretici, sabit item ve ara seviyeler transparandır. Bu görsel koşul toplama/üretim kurallarını değiştirmez.
- Enerji yetersizliği enerji metnini hafif büyütür ve kısa mesaj gösterir. Dolu tahta da kısa mesajla açıklanır; bu işlemler enerji harcamaz.
- Envantere toplama: aynı item'a 300 ms içinde çift tıklama/dokunma veya item'ı envanter butonuna sürükleyip bırakma ile çalışır. Tek tıklama toplamaz; üreticiler tek tıklamayla üretmeye devam eder. Çift tıklama aralığı Cell Inspector'ından ayarlanabilir. Başarılı toplamada prefab görseli 220 ms'de envanter düğmesine uçar; başarısız bırakmada item hücresine döner. Üreticiler envantere alınmaz.
- Kiosk hazır olma: toplama düğmesi yalnızca hazır durumuna geçişte bir kez vurgulanır; saniyelik zaman güncellemeleri animasyonu tekrarlamaz.
- Envanter ve tarif pencereleri: 140 ms saydamlık ve %97–100 ölçek geçişleri. Kapanırken panel etkileşimi kapatılır.
- Sipariş kartları: mevcut kartlar korunur; kaldırılan kartlar çıkış, yeni kartlar giriş animasyonu oynatır. Teslimat ve ödül animasyon tamamlanmasını beklemez.

Envanterden tahtaya dönüşe animasyon eklenmedi.

Envanterden dönüşte ortak InventoryCanvas panelinin eksik GameManager bağlantısı tamamlandı. Dolu envanter uyarısı da butonların açtığı bu paneli kullanır. "Tahtada boş yer gerekli" yalnızca gerçek dolulukta gösterilir; bağlantı, hazır olmayan tahta ve artık bulunmayan item durumları ayrı mesaj verir. Başarılı dönüş stoktan bir adet alır; başarısız dönüş stok tüketmez.

## Yapı ve yaşam döngüsü

DOTween kullanılır. Hücre yerleşimi hareket ettirilmez; item görseli hareket eder. Item ve enerji görselleri raycast almaz; giriş hücre üzerindedir. Sahne/prefab referansları controller'larda serialized tutulur. Uçuş için `MergeFlightImage.prefab`, pencere/kart geçişleri için prefab ve sahneye eklenen CanvasGroup bileşenleri kullanılır.

Oyun verisi hemen değiştirilip mevcut akışta kaydedilir; tween callback'leri item üretmez, tüketmez veya ödül vermez. `SetItem` kendi başına animasyon başlatmaz; kayıt yükleme ve envanterden dönüş sessiz kalır. Yeni etkileşim mevcut hareketi sonlandırıp temel görsel değerlerini geri getirir. Ekran kapanışı uçuşları, vurguları ve aktif tween'leri temizler. Sürükleme sürerken ikinci pointer ile toplama/üretim engellenir.

## Doğrulama

- Unity'nin mevcut referans önbelleğiyle Roslyn semantik tanılaması: 148 C# dosyası, 366 assembly referansı, 0 hata. Assembly üretilmedi.
- Mevcut bağımsız envanter/sipariş/ilerleme kontrolü: 18 kontrol geçti.
- Değişen sahne ve prefabların yerel fileID bağlantıları, CanvasGroup sahipliği ve parent/child bağlantıları kontrol edildi.
- Unity Editor, Play Mode, Unity Test Framework veya player build çalıştırılmadı. Görsel zamanlama ve cihaz hissi çalışma zamanında doğrulanmadı.

### Oynanışta kontrol edilecekler

1. Üreticiye seri basma, basılı tutup dışarı çıkma ve üreticiyi sürükleme.
2. Normal/sabit hedef merge, maksimum seviye, uyumsuz sabit hedef, boş hücreye taşıma ve iki item'ın yer değiştirmesi.
3. Item hareket ederken yeniden sürükleme/toplama; geçerli hedefe girip çıkma; ikinci pointer ile tıklama.
4. Enerji yokken ve tahta doluyken üretim; envanter doluyken toplama.
5. Birden fazla item'ı hızlı toplama ve uçuş sürerken bahçeye/Florist'e geçiş.
6. Pencereyi kapanırken yeniden açma; tarif üretimi ardından kapanış; hazır kiosk vurgusunun tekrarlamaması.
7. Teslimatta yalnız ilgili kartın çıkması, diğer kartların korunması ve tekrar ödül verilmemesi.
