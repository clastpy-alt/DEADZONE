# TASK_STATUS  (PENDING · IN_PROGRESS · TEST_REQUIRED · COMPLETED · BLOCKED)
Aktif faz: PHASE 01.5 (Foundation Completion & Integration) · Sıradaki görev: NEXT_TASK.md (Görev 0: Unity'de derleme/test doğrulaması)
Son güncelleme: 2026-11-26

**Hiçbir görev COMPLETED değildir:** kod Unity'de henüz derlenmedi/test edilmedi.
Ortam: Unity 6000.6.10f1, URP 17.6.0, Input System 1.20.1, uGUI 2.6.0. Test Framework manifest'te YOK -> KURULUM.md 3. adım.

## PHASE 01 / 01.5
- TEST_REQUIRED  Unity sürümü / URP / paket doğrulaması (Tools > DEADZONE > Validate Project)
- TEST_REQUIRED  Mimari, asmdef'ler, CLAUDE.md ve dokümanlar
- TEST_REQUIRED  Git altyapısı (.gitignore/.gitattributes yazıldı; `git init` kullanıcıda)
- TEST_REQUIRED  Save System + EditMode testleri (+ PlayerData serileştirme testleri)
- TEST_REQUIRED  ResourceWallet + EditMode testleri
- TEST_REQUIRED  Yerelleştirme (TR/EN, 59 anahtar)
- TEST_REQUIRED  İzometrik kamera (UI üstünde sürükleme engeli RaycastAll ile yenilendi) + yapılandırma testleri
- TEST_REQUIRED  Sahneler: Bootstrap, MainMenu, Base, WorldMap, Battle + UIGallery (dev)
- TEST_REQUIRED  Placeholder 3D askerî üs: 6 bina, beton zemin, yollar, duvar, kuleler
- TEST_REQUIRED  HUD: üst bar (avatar→profil penceresi), sol 3 / sağ 6 yan buton, alt 7 buton, kilitli göstergeler, bina bilgi paneli
- TEST_REQUIRED  19 UI prefabı: PrimaryButton, SecondaryButton, IconButton, ResourceCounter, PopupPanel, ConfirmationDialog, RewardPopup, Tooltip, LoadingScreen, ProgressBar, NotificationBadge, TabNavigation, ScrollablePanel, SafeAreaController, HeroCard, ItemCard, EventCard, ShopCard, UIOverlay
- TEST_REQUIRED  Editor araçları: Setup Phase 01, Validate Project, Validate References, Generate Scenes/UI Prefabs/Building Prefabs/Game Data, Open UI Gallery, Debug kaynak araçları
- TEST_REQUIRED  EditMode testleri: Save/Load, bozuk kayıt, kaynak ekleme/harcama/yetersiz/negatif, PlayerData, kamera, UI referansları, yardımcılar
- PENDING        PlayMode testi (Base yükleme, kayıt döngüsü)
- PENDING        Gerçek cihazda performans ve sürükleme hissi
- PENDING        Gerçek ikon/avatar/font (şimdilik metin placeholder)
- PENDING        Git deposu (`git init` kullanıcıda)

## Önemli tasarım kararları (tekrar sorgulamadan devam et)
- Statik oyun verisi = ScriptableObject (GameConfig), oyuncu verisi = PlayerData (JSON). Kaynaklar yalnız `ResourceWallet` ile değişir.
- `Game.EnsureInitialized()` her sahnede güvenle çağrılır.
- Hangi HUD butonunun çalıştığı tek yerde: `FeatureRouter.IsAvailable` (HudFeature.cs). Yeni sistem gelince buraya ekle.
- Sahneler/prefablar Editor araçlarıyla üretilir; var olanın üzerine yazılmaz (yalnız "Recreate Base Scene (Overwrite)" onayla yazar).
- Popup/Onay/Ödül/Tooltip/Yükleme **UIOverlay** prefabı ile (MainMenu, Base, UIGallery sahnelerinde) gelir; statik yardımcılar `UIOverlay.ShowPopup/Confirm/ShowReward/ShowTooltip/SetLoading` null-güvenlidir.
- Nadirlik: `Rarity` enum (Common..Mythic); kahraman kodları R/SR/SSR/UR (RarityStyle).
- Placeholder geometri/ikon adlarında `_PH` / `[PLACEHOLDER]`.
- Test asmdef `UNITY_INCLUDE_TESTS` kısıtlı: Test Framework yoksa ana oyunun derlenmesini engellemez.
- Kod İngilizce, dokümanlar Türkçe. Yalnız yeni Input System.
