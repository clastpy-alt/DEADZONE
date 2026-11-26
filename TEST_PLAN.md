# TEST_PLAN

## EditMode (Tests/EditMode) — son çalıştırma: 105/105 geçti, 0 başarısız, 0 atlanan (Unity 6000.6.10f1, 2026-11-26)
Hiçbir test silinmedi, devre dışı bırakılmadı veya gevşetilmedi.

### Faz 01 (39 test, hepsi geçiyor)
ResourceWallet, SaveService (yedek/checksum/bozuk dosya/gelecek sürüm/negatif temizleme), Yardımcılar (sayı, süre, yerelleştirme), PlayerData JSON gidiş-dönüş, Kamera yapılandırması, UI prefab referansları.

### Faz 02 (66 yeni test)
Ortak düzen: `TestWorld` (bellekte HQ/Çiftlik/Kışla/Depo veritabanı, cüzdan, yönetici, yükseltme servisi; `FakeClock` ile zaman test tarafından ilerletilir; `Reload()` kapat-aç simülasyonu). Dosya sistemi yalnız SaveV2Tests'te (geçici klasör).

| Dosya | Kapsanan alanlar |
|---|---|
| BuildingDataTests (10) | Building Data Validation: gerçek veritabanı hatasız, 22 tür, yalnız 6 işlevsel, 35 seviye, HQ gereksinimi, 30sn/2dk/5dk/15dk tablosu, doğrulayıcının bozuk veriyi yakalaması, denge eğrisi tekdüzeliği |
| BuildingUpgradeTests (24) | Building Upgrade, Insufficient Resources, HQ Level Requirement, Construction Queue Limit, Duplicate Upgrade Prevention, Construction Completion, Offline Construction Completion (reload), saat geri alma, Upgrade Cancellation (iade, çift iptal, tamamlandıktan sonra iptal), anında bitirme/elmas, hızlandırma |
| EconomyTests (17) | Resource Production, Storage Capacity (bina deposu, cüzdan kapasitesi, Depo bonusu), Offline Production (8 saat sınırı), Resource Duplication Prevention (reload, çift toplama, tamamlanmış işi iptal), başlangıç ekonomisi |
| SaveV2Tests (10) | Save Version Migration (v1→v2 kayıpsız, kayıt silinmez/karantinaya alınmaz, idempotent, mevcut bina korunur), Save and Reload (bina/kuyruk/zaman damgaları), Sanitize |
| PowerTests (5) | Power Calculation: bina gücü HQ/diğer, seviye artışı, bilinmeyen bina, IPowerSource kayıt/negatif, RecalculatePower |

## Editor doğrulayıcıları (Tools > DEADZONE)
Validate Buildings, Validate Economy, Validate Save Migration, Validate References, Validate Project — Unity'de çalıştırıldı, "No problems found".

## PlayMode
Otomatik PlayMode testi YAZILMADI (bkz. Eksikler). Elle Play oturumunda doğrulananlar (1080x1920 portre Game görünümü): bina seçimi + bilgi sayfası, yükseltme (maliyet düşer, ilerleme, tamamlanma, güç 335→433 ve 433→466), HQ seviye gereksinimi kırmızı uyarı, Bilgi penceresi, inşaat kuyruğu penceresi, İptal (onay + %80 iade), Hızlandır (onay, 1 Elmas düştü, seviye arttı), Topla (üretici binada cüzdana geçti), kayıt kalıcılığı, kamera, Console'da 0 hata.

## Elle test listesi (hâlâ elle)
1. Bootstrap → MainMenu → Oyna → Base.  2. Sürükle/zoom/sınır.  3. Bina tıkla → bilgi sayfası, halka.  4. Boşluğa tıkla → kapanır.
5. Çiftlik: Topla → kaynak artar.  6. Geliştir → İnşaat penceresi → Hızlandır / İptal.  7. Kaydet → Play'i durdur → aç: seviye/kuyruk/kaynak korunur.
8. TR↔EN geçişi.  9. Yeni Oyun (YALNIZ test kaydında): 5000/5000/5000/2500/1000/200/100.  10. Gerçek cihazda çentik/Safe Area.

## Eksikler / sonraki
Otomatik PlayMode testleri; çevrimdışı tamamlanma bildiriminin gösterimi (BUG_TRACKER); gerçek cihaz performansı; tekrar ödül/satın alma/etkinlik testleri sonraki fazlarda.
