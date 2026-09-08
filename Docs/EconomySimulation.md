# Ekonomi simülasyonu

Gerçek config assetlerinden üretilmiştir. 1.000 sabit rastgele tohum; 52 oyun günü. Oynanış testi değildir.

Normal senaryo: tüm planlı müşteriler, doğru sipariş/para üstü, 90/100 memnuniyet, %17.5 bahşiş. Merge ve reklam geliri **0**.
Düşük tempo: günlük müşterilerin %60’ı (yukarı yuvarlanır), bahşiş **0**, Merge ve reklam geliri **0**. Servis edilmeyen müşteri için malzeme hazırlanmadığı varsayılır.
Gün sayacı takvim günü değil, tamamlanan oyun günüdür. Her açılan ürün alınır; kira configteki değerdir.

| Gün | Müşteri | Ciro ort. | Maliyet ort. | Bahşiş ort. | Net kazanç ort. | Yeni ürün bedeli | Gün sonu bakiye ort. | Düşük tempo net ort. |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 1 | 4 | 109.0 | 67.0 | 20.0 | 62.0 | 0 | 72.0 | 30.0 |
| 2 | 5 | 189.0 | 119.0 | 33.0 | 103.0 | 10 | 165.0 | 42.0 |
| 3 | 6 | 269.4 | 171.3 | 47.4 | 145.4 | 10 | 300.4 | 64.0 |
| 4 | 7 | 302.8 | 193.1 | 52.1 | 161.8 | 20 | 442.2 | 80.3 |
| 5 | 6 | 238.2 | 151.1 | 41.7 | 128.7 | 10 | 561.0 | 62.1 |
| 6 | 6 | 251.4 | 159.8 | 45.0 | 136.6 | 15 | 682.6 | 60.7 |
| 7 | 5 | 214.0 | 136.0 | 38.0 | 116.0 | 25 | 773.6 | 48.0 |
| 8 | 5 | 214.8 | 137.1 | 38.3 | 116.0 | 15 | 874.6 | 46.5 |
| 14 | 5 | 230.8 | 147.9 | 40.7 | 123.6 | 15 | 1476.1 | 49.8 |
| 21 | 5 | 254.5 | 162.8 | 45.1 | 136.8 | 30 | 2224.4 | 55.1 |
| 30 | 5 | 247.2 | 158.1 | 43.6 | 132.7 | 20 | 3196.1 | 53.6 |
| 40 | 5 | 274.9 | 178.9 | 47.7 | 143.6 | 50 | 4179.0 | 57.6 |
| 41 | 5 | 274.3 | 178.5 | 47.6 | 143.4 | 40 | 4282.4 | 57.6 |
| 51 | 5 | 274.4 | 178.6 | 47.6 | 143.3 | 90 | 5045.6 | 57.8 |
| 52 | 5 | 274.4 | 178.6 | 47.6 | 143.4 | 150 | 5039.1 | 57.7 |

Her iki senaryoda da toplam geciken günlük satın alma: **0**.
52. gün ortalama bakiye: normal **5039**, düşük tempo **944**.
Bu rahat denge, zorunlu para satın alımı veya zorunlu Merge ihtiyacı oluşturmaz. Hatalı hazırlanıp atılan buketler, oyuncunun kasada fazla para üstü vermesi ve gün bitiminde yarım kalan siparişler modele dahil değildir.

## Ürün fiyatları ve açılış takvimi

Shop bedeli bir kere ödenir. Birim maliyet/satış fiyatı her buketteki kullanım başınadır. İlk gün seçili dekorlar ve başlangıç malzemeleri ücretsizdir.

| Oyuncu günü | ID | Ürün | Shop bedeli | Birim maliyet | Birim satış |
|---:|---|---|---:|---:|---:|
| 1 | decor_chandelier_standard | Standart Avize | 0 | — | — |
| 1 | decor_curtain_standard | Standart Perde | 0 | — | — |
| 1 | decor_painting_board | Not Panosu | 0 | — | — |
| 1 | decor_shelf_standard | Standart Raf | 0 | — | — |
| 1 | decor_trim_wooden | Ahşap Şerit | 0 | — | — |
| 1 | decor_wall_yellow | Sarı Duvar | 0 | — | — |
| 1 | f_1 | gypsum | 0 | 5 | 8 |
| 1 | f_2 | eucalyptus | 0 | 6 | 9 |
| 1 | r_3 | ribbon3 | 0 | 1 | 2 |
| Yakında | u_1 | Kurye | 0 | — | — |
| 1 | wp_7 | wrapper7 | 0 | 2 | 4 |
| 2 | f_3 | rose_red | 10 | 10 | 15 |
| 3 | wp_11 | wrapper11 | 10 | 2 | 4 |
| 4 | f_4 | rose_white | 20 | 10 | 15 |
| 5 | r_5 | ribbon5 | 10 | 1 | 2 |
| 6 | wp_9 | wrapper9 | 15 | 2 | 4 |
| 7 | f_5 | rose_pink | 25 | 10 | 15 |
| 8 | r_4 | ribbon4 | 15 | 1 | 2 |
| 9 | f_23 | daisy | 20 | 7 | 11 |
| 10 | wp_12 | wrapper12 | 10 | 2 | 4 |
| 11 | f_6 | rose_yellow | 20 | 10 | 15 |
| 12 | r_1 | ribbon1 | 15 | 1 | 2 |
| 13 | f_9 | anemone_pink | 25 | 10 | 15 |
| 14 | wp_1 | wrapper1 | 15 | 2 | 4 |
| 15 | f_7 | anemone_red | 25 | 10 | 15 |
| 16 | r_2 | ribbon2 | 15 | 2 | 3 |
| 17 | f_8 | anemone_purple | 30 | 10 | 15 |
| 18 | wp_2 | wrapper2 | 15 | 3 | 5 |
| 19 | f_19 | tulip_white | 30 | 11 | 17 |
| 20 | r_6 | ribbon6 | 15 | 2 | 3 |
| 21 | f_20 | tulip_red | 30 | 11 | 17 |
| 22 | wp_3 | wrapper3 | 20 | 3 | 5 |
| 23 | f_21 | tulip_pink | 35 | 11 | 17 |
| 24 | r_7 | ribbon7 | 20 | 2 | 3 |
| 25 | f_22 | tulip_yellow | 35 | 11 | 17 |
| 26 | wp_4 | wrapper4 | 20 | 3 | 5 |
| 27 | f_15 | carnation_white | 40 | 9 | 14 |
| 28 | r_8 | ribbon8 | 20 | 2 | 3 |
| 29 | f_16 | carnation_red | 40 | 9 | 14 |
| 30 | wp_5 | wrapper5 | 20 | 3 | 5 |
| 31 | f_17 | carnation_pink | 40 | 9 | 14 |
| 32 | wp_6 | wrapper6 | 25 | 4 | 6 |
| 33 | f_18 | carnation_yellow | 45 | 9 | 14 |
| 34 | wp_8 | wrapper8 | 25 | 4 | 6 |
| 35 | f_10 | gladiolus_white | 45 | 12 | 18 |
| 36 | wp_10 | wrapper10 | 25 | 4 | 6 |
| 37 | f_11 | gladiolus_red | 50 | 12 | 18 |
| 38 | f_12 | gladiolus_purple | 50 | 12 | 18 |
| 39 | f_13 | gladiolus_pink | 50 | 12 | 18 |
| 40 | f_14 | gladiolus_yellow | 50 | 12 | 18 |
| 41 | decor_chandelier_flower | Çiçek Avize | 40 | — | — |
| 42 | decor_chandelier_paper | Kağıt Avize | 45 | — | — |
| 43 | decor_chandelier_retro | Retro Avize | 50 | — | — |
| 44 | decor_wall_green | Yeşil Duvar | 55 | — | — |
| 45 | decor_wall_pink | Pembe Duvar | 60 | — | — |
| 46 | decor_curtain_lace | Dantelli Perde | 65 | — | — |
| 47 | decor_curtain_dark | Koyu Perde | 70 | — | — |
| 48 | decor_shelf_double | Çift Raf | 75 | — | — |
| 49 | decor_painting_flower | Çiçek Tablosu | 80 | — | — |
| 50 | decor_trim_green | Yeşil Şerit | 85 | — | — |
| 51 | decor_trim_plain | Sade Şerit | 90 | — | — |
| 52 | u_2 | Yaz saati | 150 | — | — |
