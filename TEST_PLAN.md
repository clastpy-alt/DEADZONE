# TEST_PLAN
## Mevcut EditMode testleri (Tests/EditMode) — henüz çalıştırılmadı
ResourceWallet: ekleme, negatif/sıfır, kapasite, yetersiz bakiye, tam bakiye, olay.
SaveService: yeni oyun, gidiş-dönüş, bozuk dosya → yedek, checksum, gelecek sürüm, negatif değer temizleme.
Yardımcılar: sayı biçimi, süre biçimi (Duration), yerelleştirme tamlığı/kaynak adları, dil değişimi.
PlayerData: JSON gidiş-dönüş (long değerler dahil), kaynak listesi, benzersiz kimlik, ResourceType sayısal sabitliği.
Kamera: açılar, yakınlaştırma aralığı, sınırlar, ayar değerleri (SerializedObject ile; Play Mode gerekmez).
UI referansları: 19 prefabın varlığı + ReferenceValidator (boş referans yok). Önce Setup Phase 01 çalıştırılmalı.

## Elle test (Play Mode)
1. Bootstrap → MainMenu → Oyna → Base açılır.  2. Sürükle/zoom/sınır.  3. Bina tıkla → panel + halka.
4. Boşluğa tıkla → seçim kalkar.  5. Kilitli butonlar toast verir, Üs butonu kamerayı sıfırlar.
6. Kaydet → Play'i durdur → tekrar aç: isim/kaynak korunur.  7. TR↔EN geçişi.  8. Yeni Oyun onay penceresi.  9. Avatar → profil penceresi, sayaç tooltip, UIGallery sahnesi.
Ayrıntılı adım adım liste: KURULUM.md bölüm D.

## Sonraki fazlarda
Tekrar ödül alma, tekrar satın alma, etkinlik süresi dolması, bağlantı kopması, performans.
