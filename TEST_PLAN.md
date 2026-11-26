# TEST_PLAN

## EditMode (Tests/EditMode) — son çalıştırma: 108/108 geçti, 0 başarısız, 0 atlanan (Unity 6000.6.10f1, 2026-11-26)
Hiçbir test silinmedi, devre dışı bırakılmadı veya gevşetilmedi.

### Faz 01 (39 test, hepsi geçiyor)
ResourceWallet, SaveService (yedek/checksum/bozuk dosya/gelecek sürüm/negatif temizleme), Yardımcılar (sayı, süre, yerelleştirme), PlayerData JSON gidiş-dönüş, Kamera yapılandırması, UI prefab referansları.

### Faz 02 (66 yeni test) + Faz 02.5 (+3: Depo/depolama, başlangıç binası sayısı)
Ortak düzen: `TestWorld` (bellekte HQ/Çiftlik/Kışla/Depo veritabanı, cüzdan, yönetici, yükseltme servisi; `FakeClock` ile zaman test tarafından ilerletilir; `Reload()` kapat-aç simülasyonu). Dosya sistemi yalnız SaveV2Tests'te (geçici klasör).

| Dosya | Kapsanan alanlar |
|---|---|
| BuildingDataTests (10) | Building Data Validation: gerçek veritabanı hatasız, 22 tür, 7 işlevsel (Depo dahil), 35 seviye, HQ gereksinimi, 30sn/2dk/5dk/15dk tablosu, doğrulayıcının bozuk veriyi yakalaması, denge eğrisi tekdüzeliği |
| BuildingUpgradeTests (24) | Building Upgrade, Insufficient Resources, HQ Level Requirement, Construction Queue Limit, Duplicate Upgrade Prevention, Construction Completion, Offline Construction Completion (reload), saat geri alma, Upgrade Cancellation (iade, çift iptal, tamamlandıktan sonra iptal), anında bitirme/elmas, hızlandırma |
| EconomyTests (17) | Resource Production, Storage Capacity (bina deposu, cüzdan kapasitesi, Depo bonusu), Offline Production (8 saat sınırı), Resource Duplication Prevention (reload, çift toplama, tamamlanmış işi iptal), başlangıç ekonomisi |
| SaveV2Tests (10) | Save Version Migration (v1→v2 kayıpsız, kayıt silinmez/karantinaya alınmaz, idempotent, mevcut bina korunur), Save and Reload (bina/kuyruk/zaman damgaları), Sanitize |
| PowerTests (5) | Power Calculation: bina gücü HQ/diğer, seviye artışı, bilinmeyen bina, IPowerSource kayıt/negatif, RecalculatePower |

## Editor doğrulayıcıları (Tools > DEADZONE)
Validate Buildings, Validate Economy, Validate Save Migration, Validate References, Validate Project — Unity'de çalıştırıldı, "No problems found".

## PlayMode (Tests/PlayMode) — son çalıştırma: 9/9 geçti (Unity 6000.6.10f1, 2026-11-26)
`BasePlayModeTests`: gerçek Base sahnesi, yalıtılmış kayıt klasörü ve kontrol edilebilir saat (`Game.ResetForTests`); oyuncunun kendi kaydına dokunulmaz.
- Base sahnesi yüklenir (HUD, kamera, toast, kuyruk paneli, 7 bina)
- Yeni kayıt test ekonomisini kullanır
- Bina seç → bilgi sayfası görünür; Kapat → gizlenir, seçim temizlenir
- Yükseltme: maliyet düşer, saat ilerleyince seviye artar, kuyruk boşalır, güç artar
- Üretim toplanır (1 saat = 1200)
- Kaydet → kapat → bir saat sonra aç: seviye korunur, çevrimdışı tamamlanma bildirilir (toast metni) ve yalnız bir kez tüketilir
- Çevrimdışı üretim: min(8 saat sınırı, bina tamponu); 20 saatlik üretim asla yazılmaz
- Dil değişimi metinleri değiştirir ve kaydedilir
- Kamera geniş başlar, odak hareket eder
Test Runner kullanımı: ayrı pencere açıkken yeni kod derlenmez; pencereyi kapat, Unity ana penceresinde Ctrl+R, sonra aç.

Elle Play'de ayrıca: seçim + bilgi sayfası + Kapat (02.5'te yeniden doğrulandı), yükseltme, iptal, hızlandırma, Topla, kayıt kalıcılığı.

## Elle test listesi (hâlâ elle)
1. Bootstrap → MainMenu → Oyna → Base.  2. Sürükle/zoom/sınır.  3. Bina tıkla → bilgi sayfası, halka.  4. Boşluğa tıkla → kapanır.
5. Çiftlik: Topla → kaynak artar.  6. Geliştir → İnşaat penceresi → Hızlandır / İptal.  7. Kaydet → Play'i durdur → aç: seviye/kuyruk/kaynak korunur.
8. TR↔EN geçişi.  9. Yeni Oyun (YALNIZ test kaydında): 5000/5000/5000/2500/1000/200/100.  10. Gerçek cihazda çentik/Safe Area.

## Eksikler / sonraki
Topla/İptal toast'ının elle görsel doğrulaması (B-008); gerçek cihaz/Android FPS ve bellek ölçümü (yapılmadı); tekrar ödül/satın alma/etkinlik testleri sonraki fazlarda.


## PHASE 02.5 güncellemesi (ölçüldü, 2026-11-26)
- EditMode: 140/140 geçti (kademe, kamera yapılandırması, grafik profilleri, ikon/Referans doğrulayıcıları dahil).
- PlayMode: 14/14 geçti — `Phase025PlayModeTests`: grafik kalitesi kaydı (3 seviye), Ayarlar paneli dişliden açılır/kapanır, HUD dişlisi, bina önizleme render + ortalama regresyonu, kamera zoom aralığı.
- `Phase025ScreenshotTests` ([Explicit], Run All'a girmez): Game görünümü Portrait 1080x1920 iken 8 ekran görüntüsünü `Docs/Screenshots/Phase025/` altına yazar.
- Hâlâ elle/cihazda: Android FPS/RAM, Safe Area, kademe 2–7 görsel kontrolü, ışık/probe bake.
