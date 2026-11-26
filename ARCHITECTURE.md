# ARCHITECTURE

Assembly'ler: `Deadzone.Runtime` (Scripts/), `Deadzone.Editor` (Scripts/Editor), `Deadzone.Tests.EditMode` (Tests/EditMode).

| Modül | Sorumluluk |
|---|---|
| Core/Game | Config, PlayerData, Wallet, SaveService, GameState tutan ince servis sınıfı. `EnsureInitialized()` her sahneden güvenle çağrılır. |
| Core/GameRuntime | DontDestroyOnLoad: otomatik kayıt (30 sn), pause/quit'te kayıt, AudioService. |
| Core/GameEvents | Gevşek bağlı statik olaylar (kaynak, bina seçimi, durum, dil, kayıt). |
| Core/Loc | TR/EN yerelleştirme tablosu. Yeni dil = enum + sütun; `FindIncompleteKeys()` eksikleri bulur. |
| Core/IsometricCameraController | Ortografik kamera: sürükleme, pinch/wheel zoom, yumuşatma, sınır, bina seçimi. |
| Data/GameConfig | Sabit ayarlar (ScriptableObject, Resources/GameConfig). |
| SaveSystem | PlayerData + SaveService: versiyonlu JSON, SHA256 bütünlük, yedek, göç (Migrate), bozuk kayıt karantinası. |
| Economy/ResourceWallet | Kaynaklar için tek giriş noktası. |
| Buildings | Veri: BuildingDefinition/Database (SO), BuildingLevelData, BuildingBalanceCurve. Çalışma: BuildingManager (örnek, üretim, Topla, kapasite), BuildingUpgradeService (yükseltme, kuyruk, iptal, anında bitirme, çevrimdışı tamamlanma), PlayerPower (güç + IPowerSource kancaları), IGameClock (UTC, sunucu zamanına hazır). BuildingSelectable/SelectionController dünya girişi. |
| UI/HUD | HudController, FeatureRouter (hangi buton açık), NavButton, Toast, SafeArea, LocalizedText. |
| UI/Bileşenler | UIPanel (fade+scale), PopupPanelView, ConfirmationDialogView, RewardPopupView, TooltipView/TooltipTrigger, LoadingScreenView, ProgressBarView, TabNavigationView, NotificationBadgeView, Hero/Item/Event/ShopCardView, ButtonPressAnimator. Hepsi Editor tarafından prefab olarak üretilir. |
| UI/UIOverlay | Sahnede tek prefab (Canvas sortingOrder 100); popup, onay, ödül, tooltip, yükleme. `UIOverlay.ShowPopup/Confirm/ShowReward/ShowTooltip/SetLoading` statik ve null-güvenli. |
| UI/Bina | BuildingInfoPanel (bilgi sayfası), ConstructionQueuePanel (kuyruk penceresi). |
| Editor/Faz 02 | BuildingDatabaseGenerator, Phase02Setup, Phase02Validators, MilitaryBaseGenerator, BuildingUiTools (sahne yamalama), UIPrefabGenerator.Buildings. |
| Editor/ReferenceValidator | UI ve bina prefablarındaki boş serileştirilmiş referansları bulur (menü + test tarafından kullanılır). |

## Kararlar
- Kayıt şeması: `{version, checksum, payload}`; checksum yalnızca kazara bozulmayı yakalar, hile koruması DEĞİLDİR (Faz 08'de sunucu otoriter).
- Statik sahne içeriği Editor araçlarıyla üretilir (yeniden çalıştırılabilir); sahne elle düzenlenirse üzerine yazılmaz.
- Kayıt sürümü 2: bina/kuyruk/zaman damgaları PlayerData'da; v1 → v2 `SaveMigration` ile kayıpsız. Zamanlar UTC saniye, tek kaynak IGameClock (cihaz saati sınırları: ECONOMY_BALANCE.md §7).
- Resources klasörü GameConfig ve BuildingDatabase için kullanılır (şartnamedeki klasör listesine küçük sapma).
