# TASK_STATUS  (PENDING · IN_PROGRESS · TEST_REQUIRED · COMPLETED · BLOCKED)
Aktif faz: PHASE 02 (Askerî Üs ve Ekonomi, v0.2.0) — kod, testler ve Unity doğrulaması tamam; kullanıcı onayı bekleniyor · PHASE 03'e kullanıcı komutu olmadan geçilmez
Son güncelleme: 2026-11-26

Ortam: Unity 6000.6.10f1, URP 17.6.0, Input System 1.20.1, uGUI 2.6.0. Derleme: 0 hata. **EditMode: 105/105 geçti** (eski 39 + yeni 66; hiçbir test silinmedi/devre dışı bırakılmadı). Console: 0 hata/0 uyarı (Play).

## PHASE 02 görevleri
- COMPLETED      02.00 Cila: bilgi sayfası yerleşimi, toast konumu, kamera yakınlığı, TMP taşma
- COMPLETED      02.01 Veri tabanlı bina sistemi (BuildingDefinition/Database/Instance, azami seviye 35); Validate Buildings temiz; BuildingDataTests 10 test
- COMPLETED      02.02 22 bina türü veri olarak; yalnız HQ, Kışla, Hastane, Araştırma Lab., Gıda Çiftliği, Çelik Fabrikası işlevsel (diğerleri Planned, UI'da kilitli)
- COMPLETED      02.03 Askerî üs ortamı: yer tutucu props (pist, varil, çit, jeneratör, radar, çalı) + bina detayları; ASSET_REGISTER.md notu. TEST_REQUIRED: gerçek cihaz performansı
- COMPLETED      02.04 Bina bilgi sayfası (mevcut/sonraki seviye, maliyet, süre, gereksinim, neden, Topla/Bilgi/Geliştir)
- COMPLETED      02.05 Ekonomi: üretim, Topla, kapasite (bina deposu + Depo bonusu), çevrimdışı 8 saat sınırı, negatif bakiye yok
- COMPLETED      02.06 Gerçek zamanlı yükseltme (UTC, tek yuva, iptal %80, anında bitirme elmasla, çevrimdışı tamamlanma, çift işlem engeli). Yerel saat sınırları ECONOMY_BALANCE.md §7'de; `IGameClock` sunucu zamanına hazır
- COMPLETED      02.07 İnşaat kuyruğu UI'ı (ilerleme, kalan süre, Hızlandır, İptal; onaylı)
- COMPLETED      02.08 ECONOMY_BALANCE.md; test ekonomisi yalnız yeni kayıtlarda (5000/5000/5000/2500/1000/200/100); süre tablosu 30sn/2dk/5dk/15dk; HQ diğer binaları sınırlar
- COMPLETED      02.09 Save V2 + v1→v2 kayıpsız göç (testli); kayıt/yedek asla silinmez
- COMPLETED      02.10 PlayerPower (bina + IPowerSource kancaları); HUD güç göstergesi
- COMPLETED      02.11 Editor otomasyonu (Tools > DEADZONE: Setup Phase 02, Generate Building Database/Level Data/Military Base/Building UI, Validate Buildings/Economy/Save Migration); yeniden çalıştırma kopya üretmez
- COMPLETED      02.12 Testler: 105/105 EditMode. Otomatik PlayMode testi YAZILMADI (PENDING)
- COMPLETED      02.13 Dokümanlar güncellendi; commit/push (aşağıda hash)

## Unity'de elle doğrulananlar (Play, Portrait 1080x1920 Game görünümü)
Bina seçimi + bilgi sayfası, yükseltme (maliyet düşer/ilerleme/tamamlanma, güç artar), HQ gereksinimi uyarısı, Bilgi penceresi, İnşaat penceresi, İptal (onay+iade), Hızlandır (onay, 1 Elmas, seviye arttı), Topla (cüzdan arttı), kayıt kalıcılığı.

## Doğrulanmayanlar / açık (bkz. BUG_TRACKER.md)
- PENDING        Otomatik PlayMode testi
- TEST_REQUIRED  Çevrimdışı tamamlanma toast'ı (B-005), Topla/İptal toast'ları ekran görüntüsünde yakalanamadı (B-008)
- TEST_REQUIRED  Safe Area gerçek çentikli cihazda; dokunma hissi/performans
- TEST_REQUIRED  EN dil, Yeni Oyun onayı (kullanıcının kaydı silinmesin diye denenmedi), UIGallery
- PENDING        Denge: ~L18 üstü maliyet > kapasite (Depo işlevsel değil) — B-009

## Önemli tasarım kararları (tekrar sorgulamadan devam et)
- Statik oyun verisi = ScriptableObject (GameConfig, BuildingDefinition/Database), oyuncu verisi = PlayerData (JSON, v2). Kaynaklar yalnız `ResourceWallet` ile değişir.
- Zaman tek kaynaktan: `IGameClock` (UTC saniye); `BuildingManager.Now()` geri gitmez.
- Servisler `Game` statik tutucusunda: Config, Saves, Player, Wallet, BuildingDb, Buildings, Upgrades, Clock. Tek GameManager yok.
- Hangi HUD butonunun çalıştığı tek yerde: `FeatureRouter.IsAvailable` (HudFeature.cs).
- Sahneler/prefablar/SO'lar Editor araçlarıyla üretilir; var olan üzerine yazılmaz. Eski sahneler `Setup Phase 02` ile yamalanır (info paneli layoutVersion).
- Placeholder geometri/ikon adlarında `_PH` / `[PLACEHOLDER]`.
- Kod İngilizce, dokümanlar Türkçe. Yalnız yeni Input System.
