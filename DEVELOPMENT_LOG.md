# DEVELOPMENT_LOG — DEADZONE: LAST EMPIRE

Son güncelleme: 2026-11-26 · Aktif faz: PHASE 01.5 · Genel durum: **KOD YAZILDI, UNITY'DE HENÜZ DERLENMEDİ/TEST EDİLMEDİ**

> Bir sonraki oturum: önce CLAUDE.md → NEXT_TASK.md → bu dosyanın "Oturum 1" ve "Oturum 2" bölümlerini oku. Projeyi baştan analiz etmek gerekmez; dosya envanteri ve mimari kararlar aşağıda.

---

## Oturum 1 — 2026-11-26 (PHASE 01 temel paketi)

### Ortam (doğrulandı: Packages/manifest.json + ProjectSettings/ProjectVersion.txt okundu)
- Unity **6000.6.10f1**, URP **17.6.0**, Input System **1.20.1**, uGUI **2.6.0** (TextMeshPro bu pakette), şablon: Universal 3D (SampleScene, Mobile_RPAsset/PC_RPAsset var).
- **com.unity.test-framework manifest'te YOK** → kurulmadan testler derlenmez (test asmdef `UNITY_INCLUDE_TESTS` kısıtıyla korunuyor, projeyi bozmaz).
- Unity proje kökü (iç içe): `C:\Users\Clast\OneDrive\Desktop\DEADZONE-LAST-EMPIRE\DEADZONE-LAST-EMPIRE`. Tüm dosyalar buraya yazıldı. Git burada başlatılmalı.
- Yeni proje eski DEADZONE'dan bağımsız (eski: Downloads\DEADZONE-LAST-EMPIRE-Tek-Klasor) — eski dosyalara dokunulmadı.
- Kod/dosyalar bilgisayara `device_commit_files` ile yazıldı; Unity Editor'e, C# derleyicisine ve bilgisayarda komut satırına erişim YOKTU.

### Tamamlanan (YAZILDI — doğrulanmadı)
Hiçbiri "COMPLETED" değildir; hepsi TEST_REQUIRED.
- Mimari: 3 asmdef (Runtime, Editor, Tests.EditMode), klasör yapısı `Assets/_Game/...`
- Core: Game (servis tutucu), GameRuntime (30 sn otomatik kayıt, pause/quit kaydı), GameEvents, GameState, SceneLoader/SceneNames, GameBootstrap, AudioService (klip yok), Loc (TR/EN, 40 anahtar)
- Save: PlayerData, SaveService (versiyonlu JSON, SHA256, yedek, Migrate kancası, bozuk kayıt karantinası, tmp→main taşıma)
- Economy: ResourceWallet (negatif/geçersiz miktar engeli, kapasite, Changed olayı)
- Kamera: IsometricCameraController (fare sürükle, dokunma sürükle, pinch, tekerlek zoom, yumuşatma, XZ sınır, tıklayarak bina seçimi; yeni Input System)
- UI runtime: HudController, ResourceCounterView, NavButton + FeatureRouter (yalnız "Base" açık, diğerleri kilitli), BuildingInfoPanel, ToastView, SafeAreaFitter, LocalizedText, MainMenuController, UIColors, NumberFormatter
- Editor araçları (Tools > DEADZONE): Setup Phase 01, Validate Project, Generate UI, Generate Building Prefabs, Create Base Scene, Build Development APK
- Üretilecek içerik (Setup sonrası oluşur): 5 sahne, 6 placeholder bina prefabı, 4 UI prefabı, URP materyalleri, GameConfig.asset, placeholder üs ortamı (duvar, yol, kule, ağaç, araç, konteyner, ev, enkaz)
- EditMode testleri yazıldı (çalıştırılmadı): ResourceWalletTests (7), SaveServiceTests (6), UtilityTests (≈10 durum)
- Dokümanlar + Git dosyaları: CLAUDE.md, MASTER_PROMPT.md (V3.0 tam metin), GAME_DESIGN, ARCHITECTURE, ROADMAP, TASK_STATUS, BUG_TRACKER, ECONOMY_BALANCE, UI_DESIGN_SYSTEM, ASSET_REGISTER, TEST_PLAN, KURULUM, NEXT_TASK, .gitignore, .gitattributes

### Oluşturulan dosyalar (proje köküne göre)
- Kök: `.gitignore`, `.gitattributes`, `CLAUDE.md`, `MASTER_PROMPT.md`, `GAME_DESIGN.md`, `ARCHITECTURE.md`, `ROADMAP.md`, `DEVELOPMENT_LOG.md`, `TASK_STATUS.md`, `NEXT_TASK.md`, `BUG_TRACKER.md`, `ECONOMY_BALANCE.md`, `UI_DESIGN_SYSTEM.md`, `ASSET_REGISTER.md`, `TEST_PLAN.md`, `KURULUM.md`
- `Assets/_Game/Scripts/Deadzone.Runtime.asmdef`
- `Scripts/Core/`: AudioService, Game, GameBootstrap, GameEvents, GameRuntime, GameState, IsometricCameraController, Loc, SceneLoader, SceneNames
- `Scripts/Data/`: GameConfig, Language, ResourceType · `Scripts/Save/`: PlayerData, SaveService · `Scripts/Economy/`: ResourceWallet · `Scripts/Buildings/`: BuildingSelectable
- `Scripts/UI/`: BuildingInfoPanel, HudController, HudFeature (içinde FeatureRouter), LocalizedText, MainMenuController, NavButton, NumberFormatter, ResourceCounterView, SafeAreaFitter, ToastView, UIColors
- `Scripts/Editor/`: Deadzone.Editor.asmdef, BuildTools, BuildingPrefabGenerator, DeadzoneMenu, EditorPaths, MaterialLibrary, ModelBuilder, PlaceholderModels, ProjectSetup, ProjectValidator, SceneGenerator, UIFactory, UIPrefabGenerator
- `Tests/EditMode/`: Deadzone.Tests.EditMode.asmdef, ResourceWalletTests, SaveServiceTests, UtilityTests
- Toplam 61 dosya. Mevcut proje dosyaları DEĞİŞTİRİLMEDİ. (Setup Phase 01 çalışınca ProjectSettings'te yön=Portrait ve ürün adı değişir, EditorBuildSettings'e sahneler eklenir.)

### Test sonuçları
- Unity derlemesi: **ÇALIŞTIRILMADI**
- EditMode testleri: **ÇALIŞTIRILMADI**
- Play Mode: **ÇALIŞTIRILMADI**
- Yapılan tek kontrol (Unity dışı): tüm .cs dosyalarında {} () [] dengesi tamam; asmdef dosyaları geçerli JSON.

### Mevcut hatalar / riskler (hiçbiri doğrulanmadı — Console çıktısı yok)
1. Derleme hatası olasılığı: Unity 6.6 + uGUI 2.6 API farkları (özellikle `TMP_Settings.instance`, `InputSystemUIInputModule.AssignDefaultActions()`, `FindFirstObjectByType`, `Outline`/TMP türleri). İlk iş Console'a bakmak.
2. Test Framework paketi yok → testler derlenmez/çalışmaz (beklenen).
3. Türkçe karakterler (İ ş ğ ı) TMP varsayılan fontunda eksik görünebilir → font atlası/fallback gerekebilir.
4. Proje OneDrive içinde: OneDrive `Library/` ve `Temp/` klasörlerini senkronlayıp kilit/yavaşlık sorunu çıkarabilir. Öneri: projeyi OneDrive dışına taşı veya Library'yi senkrondan hariç tut.
5. Sahne/prefab üretimi hiç çalışmadı; HUD yerleşimi (1080x1920) görsel olarak doğrulanmadı, taşma/çakışma olabilir.
6. Kamera hissi (sürükleme hızı, sınırlar, tıklama-sürükleme eşiği) ayarsız; mobil dokunma gerçek cihazda test edilmedi.
7. Save: dosya yolu `Application.persistentDataPath/saves`; Windows'ta `File.Move` hedef varsa hata verir diye main önce siliniyor (çok kısa pencerede kayıt kaybı riski var, yedek bunu karşılıyor).

### Yarım kalan / yapılmayan görevler (PHASE 01)
- UI prefabları: Popup, ProgressBar, NotificationBadge, ItemCard, HeroCard, EventCard, ShopCard, TabNavigation, ConfirmationDialog, LoadingScreen (yalnız PrimaryButton, SecondaryButton, IconButton, ResourceCounter yapıldı)
- Editor araçları: Import Game Assets, Generate Test Data, Run Validation
- Kayıt: oyuncu adı/avatar düzenleme yok; kaynak değişimi için henüz oynanış yok (kaynak sabit)
- İkonlar ve avatar metin placeholder; ses/VFX yok
- Gerçek cihaz performans ölçümü, PlayMode testleri yok
- Git deposu başlatılmadı (`git init` kullanıcıda)

### Önemli tasarım kararları (tekrar sorgulamadan devam et)
- Statik oyun verisi = ScriptableObject (GameConfig, `Resources/GameConfig`), oyuncu verisi = PlayerData (JSON). Kaynaklar yalnız `ResourceWallet` ile değişir; `Game.Wallet.Changed` → `GameEvents.ResourceChanged`.
- `Game.EnsureInitialized()` her sahnede güvenle çağrılır (Base'de doğrudan Play çalışır). Domain reload kapalıysa statikler `ResetStatics` ile sıfırlanır.
- Hangi HUD butonunun çalıştığı tek yerde: `FeatureRouter.IsAvailable` (HudFeature.cs). Yeni sistem gelince buraya ekle; buton otomatik açılır.
- Sahneler Editor araçlarıyla üretilir; mevcut sahne/prefab üzerine yazılmaz (yalnız "Create Base Scene" onayla yazar). Sahneyi elle düzenlersen araçları yeniden çalıştırmak güvenlidir.
- Placeholder geometri adlarında `_PH` / `[PLACEHOLDER]` var; gerçek modeller PHASE 09.
- Resources klasörü yalnız GameConfig için kullanılır. Checksum hile koruması değil (PHASE 08'de sunucu otoriter).
- Kod İngilizce, dokümanlar/rapor Türkçe. Eski `Input.*` yok, yalnız Input System.

### Bu oturumdaki süreç notu
Çalışma alanı bir kez sıfırlandı ve ilk yazılan kod kayboldu; aynı kod yeniden yazıldı (kamera tekerlek zoom'u normalize edildi). Kaynak gerçeği artık kullanıcının proje klasörüdür.

---

## Oturum 2 — 2026-11-26 (PHASE 01.5: tamamlama ve entegrasyon)

Yine Unity/derleyici erişimi YOK. Aşağıdakilerin hiçbiri derlenmedi/çalıştırılmadı; yapılan tek otomatik kontrol: tüm .cs dosyalarında {} () [] denge kontrolü.

### Yeni dosyalar (kod)
- `Data/Rarity.cs` · `UI/`: RarityStyle, ButtonPressAnimator, UIPanel, ProgressBarView, NotificationBadgeView, TabNavigationView, PopupPanelView, ConfirmationDialogView, RewardPopupView, TooltipView, TooltipTrigger, LoadingScreenView, ItemCardView, HeroCardView, EventCardView, ShopCardView, UIOverlay, UIGalleryController
- `Editor/UIPrefabGenerator.Components.cs` (15 yeni prefab), `Editor/ReferenceValidator.cs`
- Testler: PlayerDataSerializationTests, CameraConfigurationTests, UiReferenceValidationTests

### Değiştirilen dosyalar
Loc (+~35 anahtar), NumberFormatter (+Duration), HudFeature (+BattlePass), HudController (avatar→profil), MainMenuController (onay penceresi, null koruması), ResourceCounterView (tooltip), IsometricCameraController, SceneLoader, SceneNames (+UIGallery), UIFactory (basma animasyonu, Bevel, Window), UIPrefabGenerator (partial), SceneGenerator (HUD düzeni, UIOverlay, UIGallery), PlaceholderModels (beton zemin), DeadzoneMenu (yeniden yazıldı), ProjectValidator (19 prefab + referans kontrolü), test asmdef (+Deadzone.Editor), UtilityTests (+Duration, kaynak adı anahtarları), KURULUM/TASK_STATUS/NEXT_TASK/ARCHITECTURE/UI_DESIGN_SYSTEM/TEST_PLAN.

### Düzeltilen hatalar / riskler (kod incelemesiyle, Unity'de doğrulanmadı)
1. Kamera UI engeli: `IsPointerOverGameObject(-1)` yerine `EventSystem.RaycastAll` (InputSystemUIInputModule ile pointer id sorunu riski). Tekerlek zoom'unda da UI üstü kontrolü eklendi.
2. `SceneLoader`: Build Settings'te olmayan sahne yüklenmeye çalışılınca artık hata mesajı verip duruyor (Setup yönlendirmesiyle), sahne geçişi bozulmuyor.
3. `MainMenuController`: buton referansı eksikse NullReference yerine açıklayıcı hata ve devre dışı kalma.
4. "Yeni Oyun": çift dokunma yerine ConfirmationDialog (overlay yoksa log uyarısı verir; bkz. bilinen eksikler).
5. HUD: sol/sağ buton setleri istenen listeye getirildi (sol: İnşaat, Araştırma, Görevler; sağ: Etkinlik, Takvim, VIP, Günlük Ödül, Battle Pass, Özel Teklif). Birlik Eğitimi sol butonlardan çıktı (alt barda Birlikler var).
6. Test Framework yokken test asmdef'in derlemeyi bozmaması `defineConstraints` ile güvenceye alındı (Oturum 1'den; Test Framework sürümü sabitlenmedi, Unity 6.6 çekirdek paketi).

### Tamamlanan UI bileşenleri
PrimaryButton, SecondaryButton, IconButton, PopupPanel, ConfirmationDialog, ProgressBar, ResourceCounter (tooltip'li), HeroCard, ItemCard, EventCard, ShopCard, NotificationBadge, TabNavigation, Tooltip, LoadingScreen, RewardPopup, ScrollablePanel, SafeAreaController + UIOverlay (birleşik). Tümü prefab olarak üretilir (Setup Phase 01); görsel doğrulama yapılmadı.

### Test durumu
- Derleme: ÇALIŞTIRILMADI · EditMode: ÇALIŞTIRILMADI · Play Mode: ÇALIŞTIRILMADI.
- Yeni testler (yazıldı): PlayerData gidiş-dönüş (4), Kamera yapılandırması (4), UI referans doğrulama (2), Duration (6+), kaynak adı yerelleştirme (1).

### Bilinen eksikler
- Yeni Oyun onayı yalnız UIOverlay ile çalışır; sahnede overlay yoksa buton bir uyarı loglar ve işlem yapmaz (eski çift dokunma yedeği kaldırıldı; MainMenu/Base/UIGallery sahnelerinde overlay Setup ile gelir).
- Derleme hataları olabilir (Unity 6.6 / uGUI 2.6 API farkları); ikonlar/avatarlar metin placeholder; Türkçe karakter glyph'leri doğrulanmadı.
- Hero/Item/Event/Shop kartları yalnız UIGallery'de örnek verili; oyun içinde henüz kullanılmıyor (PHASE 03+).
- PlayMode testi yok; gerçek cihaz performansı ölçülmedi; `git init` kullanıcıda.
- Prefab/sahne düzeni görsel olarak doğrulanmadı (taşma/çakışma olabilir).


---

## Oturum 3 — 2026-11-26 (PHASE 01.6: Unity entegrasyonu ve hata ayıklama)

Bu oturumda Unity 6000.6.10f1 Editor'ü kullanıcının bilgisayarında bizzat çalıştırıldı.

### Yapılanlar / sonuçlar
1. GitHub: clastpy-alt/DEADZONE main, 2b8c291 push edildi (GitHub Desktop, kullanıcı telefon onayıyla).
2. Derleme: tek hata CS0122 (`EditorPaths` erişilemez, UiReferenceValidationTests.cs) -> test dosyasında yerel sabit yol kullanıldı. Sonrasında 0 hata.
3. Test Framework: Test Runner mevcut; 39 test listeleniyor.
4. TextMeshPro: Window > TextMeshPro > Import TMP Essential Resources ile içe aktarıldı, Setup Phase 01 yeniden çalıştırıldı.
5. Setup Phase 01: Bootstrap, MainMenu, Base, WorldMap, Battle, UIGallery sahneleri oluştu. Validate Project: 0 hata, 1 uyarı (içeriği okunmadı).
6. Validate References: 'No problems found'.
7. EditMode: 39/39 geçti. Test silinmedi, devre dışı bırakılmadı.
8. Play: Bootstrap -> MainMenu ('Oyna', 'Yeni Oyun', 'Dil: Türkçe') -> Base. Üst bar, 6 kaynak sayacı, kilitli yan/alt butonlar, Türkçe karakterler (İnşaat, Araştırma, İttifak, Mağaza...) düzgün. Bina seçimi (Hastane, Seviye 1/35) ve 'Geliştir' bilgi toast'ı çalıştı; kamera sürükleme + zoom çalıştı; avatar -> 'Komutan Profili' penceresi açılıp kapandı; Kaydet düğmesi hata vermedi.
9. Kayıt kalıcılığı: ilk Play 'Save load outcome: CreatedNew' logladı; durdurup yeniden başlatınca bu log çıkmadı (Game.cs yalnız Loaded dışı sonuçlarda loglar) -> kayıt yüklendi.
10. Play sırasında Console'da hata yok; yalnız TMP Ellipsis uyarıları.

### Doğrulanmayan
Yeni Oyun onay penceresi, dil değiştirme (EN), tooltip, UIGallery, kilitli butona dokunma geri bildirimi, 1080x1920 portre yerleşim, gerçek cihaz/dokunma, kaynak değişimi sonrası kayıt (kaynak oynanışı yok), Validate Project uyarısının içeriği.

### Bilinen sorunlar
- TMP 'Ellipsis' uyarıları (bkz. TASK_STATUS.md); kozmetik.
- İlk TMP içe aktarımında tek seferlik 'Essential Resources are missing' hatası; tekrarlamadı.
- LiberationSans.ttf.meta importer sürüm uyarısı (kozmetik).
- Proje OneDrive içinde; Library/Temp senkron sorunu riski.

### PHASE 02 hazırlığı
Derleme hatası yok, ilk oynanabilir prototip (menü -> üs -> bina seçimi -> kayıt) doğrulandı. Yukarıdaki 'doğrulanmayan' maddeler PHASE 02 öncesi kısa manuel kontrol olarak önerilir; engelleyici değil.


---

## Oturum 4 — 2026-11-26 (PHASE 02: Askerî Üs ve Ekonomi, v0.2.0)

### Yapılanlar
1. **Veri modeli** (Scripts/Buildings): BuildingDefinition (SO), BuildingDatabase (SO, `Validate()`), BuildingLevelData, BuildingProductionData, BuildingRequirement, BuildingBalanceCurve (maliyet/süre/güç/üretim/depolama formülleri), BuildingInstanceData, ConstructionJobData. 22 tür; azami seviye 35.
2. **Servisler**: `BuildingManager` (örnekler, üretim, Topla, kapasite bonusu, güç yeniden hesabı, geri gitmeyen `Now()`), `BuildingUpgradeService` (CheckUpgrade/TryStartUpgrade/Cancel/ApplySpeedup/TryInstantFinish/CompleteFinished), `PlayerPower`, `IGameClock`/`SystemGameClock`. `Game` ve `GameRuntime` bunları başlatır, tick eder ve çevrimdışı tamamlanmayı yükleme sonrası çalıştırır.
3. **Kayıt V2**: `SaveService.CurrentVersion = 2`; `SaveMigration.Migrate` (v1→v2: başlangıç binaları L1, zaman damgaları; kaynaklar değişmez) ve `Sanitize`. Kayıt/yedek silinmez, v1 karantinaya alınmaz.
4. **UI**: yeniden yazılan `BuildingInfoPanel` (mevcut/sonraki seviye farkları, maliyet kırmızı/yeşil, gereksinimler, neden, Topla/Bilgi/Geliştir), yeni `ConstructionQueuePanel` (ilerleme, Hızlandır, İptal; onaylı), HUD güç göstergesi ve tamamlanma toast'ı, 6 yeni Loc anahtarı, kamera startZoom 14→22, toast konumu.
5. **Ortam**: `MilitaryBaseGenerator` — bina detayları (`Details_PH`) ve "Military Base Props [PLACEHOLDER]" grubu; yalnız Unity primitifleri.
6. **Editor**: `BuildingDatabaseGenerator`, `Phase02Setup`, `Phase02Validators`, `UIPrefabGenerator.Buildings`, `BuildingUiTools.PatchBaseScene`, DeadzoneMenu girdileri (Setup Phase 02, Generate Building Database/Level Data/UI/Military Base, Validate Buildings/Economy/Save Migration). Hepsi idempotent; mevcut sahne/prefab/asset korunur.
7. **Testler**: 66 yeni EditMode testi (TestWorld ortak düzeni + FakeClock). Test Runner: **105/105 geçti**.
8. **Dokümanlar**: ECONOMY_BALANCE, TEST_PLAN, ASSET_REGISTER, BUG_TRACKER, TASK_STATUS, NEXT_TASK, ARCHITECTURE, KURULUM güncellendi.

### Unity'de doğrulananlar
Derleme 0 hata; Validate Buildings/Economy/Save Migration/References temiz; EditMode 105/105; Play (1080x1920): seçim, bilgi sayfası, yükseltme akışı (güç 335→433→466), HQ gereksinimi, Bilgi penceresi, İnşaat penceresi, İptal (%80 iade), Hızlandır (1 Elmas), Topla, kayıt kalıcılığı. Console'da 0 hata.

### Karşılaşılan sorunlar ve çözümler
- CS0618/CS0619 (BuildingWorldLabel/HudFeature eski API) → düzeltildi.
- Bilgi sayfası yerleşim çakışması ve toast gizlenmesi → yeni ofsetler + `layoutVersion` ile eski sahneyi otomatik yenileme.
- Cihaza yazılan dosyanın eski kopya olması (BuildingInfoPanel boyut farkı) → düzenlemeden sonra birkaç sn bekle, boyutu `device_list_dir` ile doğrula.
- Menü otomasyonunda modal "No problems found" pencereleri tıklamayı engelledi → OK ile kapatıldı.

### Bilinen eksikler
Bkz. BUG_TRACKER.md (B-005…B-012) ve ECONOMY_BALANCE.md §7–8: çevrimdışı tamamlanma toast'ı, otomatik PlayMode testi, Safe Area cihaz testi, L18 üstü maliyet/kapasite dengesi, yerel saat ileri alma sınırı.

### Sonraki
PHASE 03'e yalnızca kullanıcı komutuyla geçilir (NEXT_TASK.md).


## Oturum 5 — PHASE 02.5, Grup 1: Hata düzeltmeleri (02.50)
### Yapılanlar
- Çevrimdışı biten inşaat bildirimi (`Game.OfflineCompletions`, HUD toast), Topla/İptal toast süresi 2.6 sn.
- Depo işlevsel (veri + GameConfig başlangıç binası + sahne yerleşimi); `StorageTooSmall` yükseltme nedeni ve UI mesajı; Validate Economy depolama ilerlemesi.
- Test altyapısı: `Game.ResetForTests`, `Game.SaveDirectoryOverride`; 9 PlayMode testi.
- PlayMode testinin bulduğu gerçek hatalar: (1) `BuildingSelectionController` hiçbir sahnede yoktu → statik izleme `GameEvents.RaiseBuildingSelected` içinde; (2) yeni kayıt gerçek sistem saatini kullanıyordu → `SaveService.Clock`, `PlayerData.CreateNew(config, nowUtc)`.
- Test düzeltmesi (gevşetme değil): çevrimdışı üretim testi artık min(8 saat sınırı, bina tamponu) bekler — tampon (4000) 8 saatlik üretimden (9600) önce devreye giriyor; bu davranış tasarıma uygun.
- Sabit "6 bina" beklentileri dinamik/7 yapıldı (daha sıkı).
### Doğrulama (ölçüldü)
Derleme 0 hata/0 uyarı; Setup Phase 02 → Depo sahneye yerleşti, Validate Buildings/Economy/Save Migration OK; EditMode 108/108; PlayMode 9/9; Play'de seçim + Kapat doğrulandı.
### Açık
B-008 (toast'ın elle görsel doğrulaması), Safe Area cihaz testi. Blender bu makinede tespit edilemedi (02.53). Gerçek cihaz FPS ölçümü yapılmadı.


## Oturum 5 — PHASE 02.5, Grup 3: Modeller, kademeler, materyaller (02.54 / 02.55 / 02.57)
### Yapılanlar
- Blender yok → `MilitaryModelGenerator` (+ `MilitaryModelBuildings.cs`) her bina prefab'ına `Mil_Visual` (Core + Tier1..Tier7) üretir; parçalar `MeshMerger` ile malzeme başına tek mesh'e birleştirilir (draw call/renderer azalır); eski primitifler SİLİNMEZ, devre dışı bırakılır; Foundation, SelectionRing, collider, `BuildingSelectable` değişmez.
- Silüetler: HQ (kule + çanak + beacon), Kışla (uzun/alçak kademeli çatı), Hastane (merkez + kanatlar, kırmızı haç, helipad, ambulans), Araştırma Lab. (kubbe + reaktör), Çiftlik (ahır + 2 silo + yel değirmeni + tarla), Çelik Fabrikası (testere dişi çatı + 2 baca + fırın ışıltısı), Depo (beşik çatılı hangar + rampa + konteynerler).
- `BuildingVisualTiers`: 7 eklemeli kademe (L1–5→1 … L31–35→7), seviye değişince/inşaat bitince güncellenir. Ortak kademe eklentileri: bariyer+ışık, kum torbası, çevre duvarı+kapı, güneş paneli+jeneratör, depo+haberleşme, zırh+taret, altın çerçeve+plaket.
- 25 `Mil_*` materyal (`MilitaryMaterials`); menü: Generate Military Materials / Generate Military Models / Rebuild Military Models (Generated Parts Only) — idempotent.
### Doğrulama (ölçüldü)
Derleme 0 hata; EditMode 124/124 (108 + 16 yeni kademe testi); PlayMode 9/9; Scene'de 7 binanın tamamı görsel olarak incelendi (kademe 1 görünür); collider ve BuildingSelectable bilgileri aynı.
### Açık / dürüst notlar
- Kademe 2–7'nin Play'de görsel doğrulaması ve oyun içi (Game view, 1080x1920) ekran görüntüleri sonraki adımda.
- Dikey kare/üçgen sayısı ve FPS ölçülmedi; LOD henüz yok (02.63 Generate LOD Models).
- Modeller primitif birleşimidir; gerçek sanatçı modeli değildir (PHASE 09).


## Oturum 6 — PHASE 02.5, Grup 4–6: HUD, bina UI, kamera, grafik ayarları, LOD, QA (02.56 / 02.58–02.65)
### Yapılanlar
- Premium HUD (`PremiumHudGenerator`, SerializedObject ile kalıcı yama): 34 prosedürel SDF ikon (`Resources/Icons`), kaynak sayaçları ikonlu, avatar+çerçeve+seviye rozeti, VIP, sağ üstte dişli (eski Kaydet düğmesi → Ayarlar), alt gezinme ve yan menü ikonları, kilit ikonu, inşaat zamanlayıcı hapı + sayaç balonu (`HudStatusView`).
- Ayarlar paneli (`SettingsPanel`): DÜŞÜK/ORTA/YÜKSEK (PlayerPrefs `deadzone.graphics.quality`), dil, "Şimdi Kaydet"; FPS hedefleri "cihazda ölçülmedi" notuyla.
- Grafik: URP_Low/Medium/High, `GraphicsSettingsService`, `GraphicsProfileSet`, ışık donanımı (Setup Lighting Profiles). Işık/probe bake YAPILMADI.
- Bina UI v3: büyük canlı önizleme (`BuildingPreview`, RenderTexture 384², 15 FPS, yalnız seçiliyken), mevcut/sonraki karşılaştırma, küçük seviye rozeti (`BuildingWorldLabel`: kademe rengi, inşaatta ince ilerleme + süre), inşaat sırasında Hızlandır onayı.
- Kamera: başlangıç zoom 26, maks 40, atalet, seçimde odaklanma (`focusOnSelect`).
- LOD: `Generate LOD Models` (LOD1 kutu vekili %2 ekran yüksekliğinde; pratikte nadiren devreye girer). `Generate Performance Report` statik analiz üretir (cihaz ölçümü değil).
### Bulunan/çözülen hatalar
- Önizleme boş kare: kamera, modelin pivotunun kardeşi değil AYNI nesnedeydi → model kamerayla birlikte kayıyordu. Kamera ayrı alt nesneye alındı (PlayMode regresyon testi var). Ayrıca sahne örneği statik batch'li olduğundan klonlama prefab varlığından yapılır.
- HUD sayaç ikonları play sonrası düz renge dönüyordu (prefab-instance değişikliği kaydedilmiyordu) → `SerializedObject` ile yazım.
### Doğrulama (ölçüldü)
Derleme 0 hata; EditMode 140/140; PlayMode 14/14 (+1 isteğe bağlı ekran görüntüsü testi); Validate Graphics OK; Play'de ayarlar paneli (3 kalite, dil, kaydet), önizleme ve inşaat rozeti görüldü; 8 dikey ekran görüntüsü 1080x1920 → `Docs/Screenshots/Phase025/` (01_base_overview … 08_high_mode).
### Açık / dürüst notlar
- Gerçek cihaz FPS/RAM/ısınma ÖLÇÜLMEDİ; hedefler (30/45/60) yalnız hedef. Statik rapor: 191.676 üçgen (LOD0, tüm katmanlar; Yüksek bütçesi ≤400 bin), 593 aktif MeshRenderer, 51 materyal; Low profili için üçgen bütçesi (≤150 bin) kamera kırpması olmadan aşılmış görünür — cihaz ölçümü şart.
- "Önce/sonra" karşılaştırması: Faz 02 görüntüsü kayıtlı olmadığından üretilmedi.
- Bina bilgi panelinde karşılaştırma ile maliyet arasında boş bir şerit var (kozmetik).
