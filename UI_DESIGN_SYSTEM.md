# UI_DESIGN_SYSTEM
Referans çözünürlük 1080x1920 (Canvas Scaler, match 0.5) + SafeAreaFitter.
Renkler (UIColors): Arka plan #101419 · Panel #1C232B · Askerî yeşil #465744 · Altın #D4AF63 · Gümüş #ADB8C4 · Tehlike #D94B4B · Başarı #4CAF72 · Bilgi #4C9FE8 · Metin #F2F4F6 / #8C97A3.
Nadirlik renkleri ve kodları: RarityStyle (Common gri, Uncommon yeşil, Rare mavi=R, Epic mor=SR, Legendary altın=SSR, Mythic kırmızı=UR).

## Kurallar
- Tüm metin `Loc.T(key)` / `LocalizedText`; sabit metin yok (marka adı ve [DEV]/[PLACEHOLDER] etiketleri hariç).
- Kilitli özellik = soluk + "Kilitli" kaplaması + dokununca toast (`FeatureRouter.IsAvailable`).
- Her buton `ButtonPressAnimator` ile basılınca %94 ölçeğe iner (unscaled time).
- Pencereler `UIPanel` ile 0.18 sn fade + ölçek; kapanınca nesne devre dışı.
- Pencere çerçevesi: koyu panel + altın dış çizgi + üstte ince gümüş "bevel" şeridi (`UIFactory.Window`). Gerçek metalik doku PHASE 09.
- İkon kutusu 44 px (sayaç), 110–180 px (kartlar); ikonlar şimdilik 2 harfli kod placeholder'ıdır.

## Prefablar (Assets/_Game/Prefabs/UI)
PrimaryButton, SecondaryButton, IconButton, ResourceCounter, PopupPanel, ConfirmationDialog, RewardPopup, Tooltip, LoadingScreen, ProgressBar, NotificationBadge, TabNavigation, ScrollablePanel, SafeAreaController, HeroCard, ItemCard, EventCard, ShopCard, UIOverlay.
Önizleme: Tools > DEADZONE > Open UI Gallery.
