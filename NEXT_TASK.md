# NEXT_TASK — Sıradaki görev

Güncelleme: 2026-11-26 · Faz: PHASE 01.5 (kod yazıldı, Unity doğrulaması bekliyor) · Önkoşul: Unity'de açılış

Yeni oturumda sırayla oku: CLAUDE.md → bu dosya → DEVELOPMENT_LOG.md (Oturum 1 + 2) → TASK_STATUS.md. Projeyi baştan analiz etme.

## GÖREV 0 — Derleme ve kurulum doğrulaması (ZORUNLU, her şeyden önce)
Başarısız sistemin üstüne yeni sistem ekleme. Önce çalıştığını kanıtla.

Kullanıcıdan istenecekler (bilgisayardan Unity açıkken; ben Unity'yi çalıştıramıyorum). Adım adım: **KURULUM.md**.
1. Unity'yi aç, derleme bitince Console çıktısını (kırmızı hatalar) gönder.
2. Test Framework kur, TMP Essentials içe aktar, Active Input Handling = New/Both.
3. Tools > DEADZONE > Setup Phase 01; çıkan doğrulama raporunu ve Tools > DEADZONE > Validate References sonucunu gönder.
4. Test Runner > EditMode > Run All; sonucu gönder (geçen/kalan, hata metni).
5. Bootstrap'tan Play; KURULUM.md bölüm D tablosunu doldur (hangi satır çalışmadı), ekran görüntüsü + Console gönder. Ayrıca UI Gallery'yi aç.

Benim yapacaklarım:
- Gelen derleme hatalarını dosya dosya düzelt (kodu Read ile oku, minimal düzeltme, mevcut çalışan yapıyı bozma). Düzeltmeleri `device_commit_files` ile proje klasörüne yaz (önce klasör erişimi: `C:\Users\Clast\OneDrive\Desktop\DEADZONE-LAST-EMPIRE`).
- Hata sebeplerini BUG_TRACKER.md'ye, düzeltmeleri DEVELOPMENT_LOG.md'ye yaz; TASK_STATUS.md'de yalnız gerçekten doğrulananları COMPLETED yap.
- Görsel sorunlar (HUD taşması, kamera hissi, Türkçe karakter) için ekran görüntüsüne göre ayar: UI yerleşimi `SceneGenerator.BuildHud`, kamera değerleri `IsometricCameraController` alanları, font için TMP fallback.

Çıkış kriteri: derleme hatasız, EditMode testlerin hepsi geçiyor (hiçbiri silinmeden/devre dışı bırakılmadan), Base sahnesi Play'de açılıyor, kamera hareket ediyor, HUD görünüyor, kaydet→durdur→aç ile kayıt korunuyor.

## GÖREV 1 — PHASE 01'i kapat (Görev 0 geçtikten sonra)
a) Görsel düzeltmeler: HUD/popup taşmaları, Türkçe font fallback, kamera hissi (ekran görüntülerine göre).
b) Oyuncu adı değiştirme (basit giriş) ve kaydedilmesi (PlayerData.playerName); avatar tıklama penceresine eklenebilir.
c) PlayMode testi: Base sahnesi yüklenir, HUD ve kamera var, kayıt döngüsü.
d) Editor: Import Game Assets, Generate Test Data (şimdilik Debug menüsü var).
e) Mümkünse batch mode test komutu: `Unity.exe -batchmode -runTests -testPlatform EditMode -projectPath <kök> -testResults results.xml`.
f) PHASE 01 kapanış raporu (MASTER_PROMPT.md sonundaki format).

## GÖREV 2 — PHASE 02 başlangıcı (Görev 1 tamam ve kullanıcı onayladıktan sonra)
Veri tabanlı bina sistemi, ilk 6 binayı aktif et:
1. `BuildingDefinition` ScriptableObject (id, nameKey, maxLevel=35, seviye başına maliyet/süre/güç/üretim, prefab, gereksinimler) → `Assets/_Game/ScriptableObjects/Buildings/`.
2. PlayerData'ya `buildings` listesi (id, level, upgradeEndUtc), SaveService.CurrentVersion=2 + Migrate (v1→v2: boş liste). Süreler UTC bitiş zamanı olarak saklanır.
3. `BuildingService`: seviye yükseltme = ResourceWallet.TrySpend ile gerçek maliyet; sıra (construction queue) 1 slot; offline ilerleme.
4. BuildingSelectable'ı tanımlara bağla; BuildingInfoPanel'de maliyet, süre, Geliştir butonu; HudFeature.ConstructionQueue'yu `FeatureRouter`'da aç.
5. Kaynak üretimi (Food Farm, Steel Factory) + offline üretim; testler (yetersiz kaynak, negatif miktar, çift tamamlama, UTC offline).

## Kurallar hatırlatması
- Test silerek/gevşeterek geçirme. Doğrulanmayan şeyi "tamamlandı" yazma.
- Mevcut çalışan sistemleri gereksiz değiştirme; yeni dosya eklerken asmdef referanslarına dikkat.
- Eski DEADZONE projesine dokunma. Gerçek ödeme/sunucu/mağaza: yalnız kullanıcı onayıyla.
- Her oturum sonunda: DEVELOPMENT_LOG.md, TASK_STATUS.md, NEXT_TASK.md güncelle.
