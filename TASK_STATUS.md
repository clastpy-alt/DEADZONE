# TASK_STATUS  (PENDING · IN_PROGRESS · TEST_REQUIRED · COMPLETED · BLOCKED)
Aktif faz: PHASE 02.5 (Profesyonel Grafik Yenileme, v0.2.5) — grup 1 (02.50), grup 2 (02.51–02.53 dokümanlar) ve grup 3 (02.54/02.55/02.57 modeller + katmanlar + materyaller) tamam; 02.56 ve sonrası sırada · PHASE 03'e kullanıcı komutu olmadan geçilmez
Son güncelleme: 2026-11-26

Ortam: Unity 6000.6.10f1, URP 17.6.0, Input System 1.20.1, uGUI 2.6.0. Derleme: 0 hata. **EditMode: 108/108, PlayMode: 9/9 geçti** (ölçüldü, Test Runner, 2026-11-26; hiçbir test silinmedi/devre dışı bırakılmadı). Console: 0 hata/0 uyarı.

## PHASE 02 görevleri
- COMPLETED      02.00 Cila: bilgi sayfası yerleşimi, toast konumu, kamera yakınlığı, TMP taşma
- COMPLETED      02.01 Veri tabanlı bina sistemi (BuildingDefinition/Database/Instance, azami seviye 35); Validate Buildings temiz; BuildingDataTests 10 test
- COMPLETED      02.02 22 bina türü veri olarak; HQ, Kışla, Hastane, Araştırma Lab., Gıda Çiftliği, Çelik Fabrikası ve (02.5) Depo işlevsel (diğerleri Planned, UI'da kilitli)
- COMPLETED      02.03 Askerî üs ortamı: yer tutucu props (pist, varil, çit, jeneratör, radar, çalı) + bina detayları; ASSET_REGISTER.md notu. TEST_REQUIRED: gerçek cihaz performansı
- COMPLETED      02.04 Bina bilgi sayfası (mevcut/sonraki seviye, maliyet, süre, gereksinim, neden, Topla/Bilgi/Geliştir)
- COMPLETED      02.05 Ekonomi: üretim, Topla, kapasite (bina deposu + Depo bonusu), çevrimdışı 8 saat sınırı, negatif bakiye yok
- COMPLETED      02.06 Gerçek zamanlı yükseltme (UTC, tek yuva, iptal %80, anında bitirme elmasla, çevrimdışı tamamlanma, çift işlem engeli). Yerel saat sınırları ECONOMY_BALANCE.md §7'de; `IGameClock` sunucu zamanına hazır
- COMPLETED      02.07 İnşaat kuyruğu UI'ı (ilerleme, kalan süre, Hızlandır, İptal; onaylı)
- COMPLETED      02.08 ECONOMY_BALANCE.md; test ekonomisi yalnız yeni kayıtlarda (5000/5000/5000/2500/1000/200/100); süre tablosu 30sn/2dk/5dk/15dk; HQ diğer binaları sınırlar
- COMPLETED      02.09 Save V2 + v1→v2 kayıpsız göç (testli); kayıt/yedek asla silinmez
- COMPLETED      02.10 PlayerPower (bina + IPowerSource kancaları); HUD güç göstergesi
- COMPLETED      02.11 Editor otomasyonu (Tools > DEADZONE: Setup Phase 02, Generate Building Database/Level Data/Military Base/Building UI, Validate Buildings/Economy/Save Migration); yeniden çalıştırma kopya üretmez
- COMPLETED      02.12 Testler: 105/105 EditMode. PlayMode testi 02.5'te eklendi
- COMPLETED      02.13 Dokümanlar güncellendi; commit/push (commit a195753 "PHASE 02: askeri us ve ekonomi sistemi (v0.2.0)", GitHub clastpy-alt/DEADZONE main)

## PHASE 02.5 görevleri
- COMPLETED      02.50 Hata düzeltmeleri: çevrimdışı inşaat toast'ı (B-005), Depo işlevsel + `StorageTooSmall` (B-009), Collect/Cancel toast süresi, TMP uyarısı yok, 9 PlayMode testi, `Game.ResetForTests`, seçim durumu hatası (B-013), tek zaman kaynağı (B-014). Cihaz saati manipülasyonu ECONOMY_BALANCE.md §7'de belgeli; sunucu zamanı multiplayer fazına bırakıldı.
- COMPLETED      02.51 PERFORMANCE_BUDGET.md + kalıcı kurallar (CLAUDE.md/MASTER_PROMPT.md). Hedefler HEDEFTİR; gerçek cihaz/Android ölçümü yapılmadı
- COMPLETED      02.52 ART_DIRECTION.md
- COMPLETED      02.53 Blender kontrolü (TESPİT EDİLEMEDİ; kurulum kullanıcı iznine bağlı) + BLENDER_PIPELINE.md
- COMPLETED      02.54 7 bina için ayrıntılı askeri model (Unity içinde primitif birleşimi, malzeme başına birleştirilmiş mesh); HQ + Kışla + Hastane + Lab + Çiftlik + Çelik Fabrikası + Depo; kimlik/seviye/collider/seçim korunur (Unity'de görsel doğrulandı)
- COMPLETED      02.55 7 kademeli görsel sistem (`BuildingVisualTiers`, L1–5…L31–35, eklemeli). Kademe 1 Unity'de görüldü; kademe 2–7 yalnızca EditMode testle doğrulandı, Play'de görsel olarak HENÜZ görülmedi
- COMPLETED      02.57 25 `Mil_*` URP/Lit materyal kütüphanesi (GPU instancing)
- COMPLETED      02.56 Ortam (Upgrade Base Environment, jeneratör işaretli parçalar)
- COMPLETED      02.58 Işık donanımı + LOW/MEDIUM/HIGH profilleri (bake yapılmadı)
- COMPLETED      02.59 Kamera (zoom 26/40, atalet, seçimde odak)
- COMPLETED      02.60 Premium HUD + ikon seti + inşaat sayaç hapı
- COMPLETED      02.61 Bina UI v3: canlı önizleme, karşılaştırma, küçük rozet, Hızlandır
- COMPLETED      02.62 Grafik ayarları paneli (kayıtlı, FPS hedefleri "ölçülmedi" notlu)
- COMPLETED      02.63 Menü araçları (Setup 02.5, Premium HUD, LOD, Validate Graphics, Performance Report — statik)
- COMPLETED      02.64 QA: EditMode 140/140, PlayMode 14/14, 8 ekran görüntüsü 1080x1920
- COMPLETED      02.65 Dokümanlar güncellendi
- TEST_REQUIRED  Gerçek cihaz/Android build FPS ve bellek ölçümleri — ölçülmedi; hiçbir performans hedefi karşılandı olarak raporlanmaz

## Unity'de elle doğrulananlar (Play, Portrait 1080x1920 Game görünümü)
Bina seçimi + bilgi sayfası, yükseltme (maliyet düşer/ilerleme/tamamlanma, güç artar), HQ gereksinimi uyarısı, Bilgi penceresi, İnşaat penceresi, İptal (onay+iade), Hızlandır (onay, 1 Elmas, seviye arttı), Topla (cüzdan arttı), kayıt kalıcılığı.

## Doğrulanmayanlar / açık (bkz. BUG_TRACKER.md)
- TEST_REQUIRED  Topla/İptal toast'ları ekran görüntüsünde elle doğrulanmadı (B-008)
- TEST_REQUIRED  Safe Area gerçek çentikli cihazda; dokunma hissi/performans
- TEST_REQUIRED  EN dil, Yeni Oyun onayı (kullanıcının kaydı silinmesin diye denenmedi), UIGallery

## Önemli tasarım kararları (tekrar sorgulamadan devam et)
- Statik oyun verisi = ScriptableObject (GameConfig, BuildingDefinition/Database), oyuncu verisi = PlayerData (JSON, v2). Kaynaklar yalnız `ResourceWallet` ile değişir.
- Zaman tek kaynaktan: `IGameClock` (UTC saniye); `BuildingManager.Now()` geri gitmez.
- Servisler `Game` statik tutucusunda: Config, Saves, Player, Wallet, BuildingDb, Buildings, Upgrades, Clock. Tek GameManager yok.
- Hangi HUD butonunun çalıştığı tek yerde: `FeatureRouter.IsAvailable` (HudFeature.cs).
- Sahneler/prefablar/SO'lar Editor araçlarıyla üretilir; var olan üzerine yazılmaz. Eski sahneler `Setup Phase 02` ile yamalanır (info paneli layoutVersion).
- Placeholder geometri/ikon adlarında `_PH` / `[PLACEHOLDER]`.
- Kod İngilizce, dokümanlar Türkçe. Yalnız yeni Input System.
