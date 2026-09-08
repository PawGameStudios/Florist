# Florist Merge entegrasyonu

## Giriş ve dönüş

- Normal başlangıç sahnesi: `Assets/_Florist/Scenes/1_GameScene.unity`.
- Tutorial tamamlandıktan sonra ana ekrandaki **Merge** düğmesi, `Assets/_Florist/Scenes/MergeGame.unity` sahnesini additive olarak açar.
- Florist sahnesi bellekte kalır. Açık/kapalı durumları saklanan Florist arayüzleri, kamera ve giriş nesneleri geçici olarak kapatılır; bağımsız References, kayıt ve zaman servisleri korunur.
- Merge ekranındaki **Florist'e Dön** düğmesi ilerlemeyi kaydeder, Merge sahnesini kaldırır ve Florist'in önceki durumunu geri getirir.
- Merge sahnesini tek başına açmak tahta/üretim incelemesi içindir. Ortak para ödülü ve dönüş için oyunu Florist başlangıç sahnesinden başlatın.

## İçerik ve kayıt

- Kaynak `Merge-main` değiştirilmedi. Oyun scriptleri, prefablar, görseller ve veriler `Assets/_Merge` altında bulunur. TextMesh Pro ve proje ayarlarının ikinci kopyaları taşınmadı.
- Kod `Florist.Merge` namespace'inde; Florist'in `ItemType` tanımıyla çakışmaz. Hedef, Florist'in mevcut Unity **6000.3.11f1** sürümüdür.
- Tahta, enerji, envanter, üretim ve sipariş kayıtları `Florist.Merge.*` PlayerPrefs anahtarlarını kullanır. Kaynak bağımsız oyunun kayıtları otomatik aktarılmaz.
- Müşteri siparişleri gerçek envanteri tüketir, Florist `GeneralData.ChangeMoney` üzerinden para kazandırır ve her iki kayıt tarafını günceller. Tekrar tıklama aynı siparişi ikinci kez ödüllendirmez.
- Enerji ve üretim süreleri UTC zamanını kullanır; sahne kapalıyken geçen süre hesaba katılır.
- İçerik altı tarif, altı müşteri profili ve beş ilerleme aşamasına genişletildi. Güncel enerji reklamı ve bahçe incelemesi: [Merge ilerleyişi](MergeProgressionAndGarden.md).

## Düzeltilen davranışlar

- Envanter tüketimi gerçek listeyi değiştirir ve sıfır adetli kayıtları kaldırır. Tarif malzemeleri, birbirini tekrar eden gereksinimler dahil, topluca doğrulanır.
- Envantere ekleme başarısız olursa hazır üretim kaybolmaz.
- Sürükleme yalnızca onu başlatan pointer tarafından yönetilir; ekran kapanınca sürükleme iptal edilir.
- `GetComponent`/hiyerarşi araması yerine serialized referanslar ve prefab initialization parametreleri kullanılır.
- Yeni Merge kodunda `Update`, `LateUpdate`, `FixedUpdate` yoktur. Arayüz olaylarla, süreler zamanlanmış coroutine callback'leriyle güncellenir.
- Canvas ölçeklendirmesi geniş yatay telefonlarda tahtayı kesmemek için Florist ile aynı Expand modundadır.

## Yapılan doğrulama ve sınırlar

- Unity'nin mevcut önbelleğindeki Unity 6 assembly referansları kullanılarak Roslyn ile 142 C# dosyası üzerinde semantik analiz yapıldı: **0 hata**. Assembly üretilmedi; Unity-generated proje/solution dosyaları çalıştırılmadı veya değiştirilmedi.
- Sahne/prefab/veri YAML sözdizimi, GUID/fileID bağlantıları, yeni serialized alanlar, prefab bileşen tipleri ve UI hiyerarşisi kontrol edildi.
- Unity Editor, Play Mode, Unity Test Framework veya player build çalıştırılmadı. Gerçek cihaz görünümü ve uçtan uca oynanış henüz çalıştırılarak doğrulanmış değildir.
- Florist para kaydı ve Merge PlayerPrefs kaydı ayrı depolardır; işletim sistemi tam iki kayıt arasındayken süreci sonlandırırsa bu iki depoda atomik işlem garantisi yoktur.

## Play Mode'da kontrol edilecek akış

1. Florist ana ekranından Merge'e girin; tek kamera/giriş sistemiyle etkileşimin çalıştığını kontrol edin.
2. Üreticiye dokunun; aynı tür/seviyede iki eşyayı birleştirin; son seviyedeki eşyayı envantere alın.
3. Bahçede malzemelerle saksı üretin, süreyi bekleyip toplayın ve müşteri siparişini teslim edin. Florist para göstergesinin arttığını doğrulayın.
4. Florist'e dönüp yeniden girin; tahta, enerji, envanter, üretimler ve siparişlerin korunduğunu doğrulayın.
5. İki parmakla sürükleme, geniş yatay ekran, üretim sırasında sahneden çıkma ve uygulamayı arka plana alma durumlarını kontrol edin.
