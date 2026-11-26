# KURULUM — İlk oynanabilir prototipi Unity'de açma (PHASE 01.5)

Unity proje kökü: `C:\Users\Clast\OneDrive\Desktop\DEADZONE-LAST-EMPIRE\DEADZONE-LAST-EMPIRE`
Doğrulanan sürümler: Unity 6000.6.10f1 · URP 17.6.0 · Input System 1.20.1 · uGUI 2.6.0 (TextMeshPro dahil)

> Bu kodun hiçbiri Unity'de derlenmedi/çalıştırılmadı. Aşağıdaki adımlar bunu ilk kez yapacağın adımlardır. Herhangi bir adımda kırmızı hata çıkarsa Console metnini bana gönder; sonraki adıma geçme.

## A. Hazırlık (bir kez)
1. Projeyi Unity Hub'dan aç. Derleme bitene kadar bekle (sağ alttaki dönen simge durana kadar).
2. **Console'u kontrol et** (Window > General > Console). Kırmızı hata varsa dur ve bana gönder.
3. **Test Framework'ü kur:** Window > Package Manager > sol üst "Unity Registry" > "Test Framework" > Install.
   Sürüm elle seçilmez; Unity 6.6 ile gelen sürüm kurulur. (Kurmazsan oyun yine derlenir, yalnızca testler çalışmaz.)
4. **TextMeshPro:** Window > TextMeshPro > Import TMP Essential Resources (sorarsa Import).
5. **Girdi:** Edit > Project Settings > Player > Other Settings > Active Input Handling = *Input System Package (New)* veya *Both*. Unity yeniden başlatmak isterse onayla.
6. **Önerilir:** proje OneDrive içinde. `Library` klasörünün senkronlanması yavaşlık/kilit yapabilir. OneDrive'da bu klasörü "Bu cihazda her zaman tut"tan çıkar veya projeyi OneDrive dışına taşı.

## B. Tek tıkla kurulum
7. Menü: **Tools > DEADZONE > Setup Phase 01 (Run This First)**.
   Bu araç: GameConfig verisini, materyalleri, 6 bina prefabını, 19 UI prefabını, 6 sahneyi (Bootstrap, MainMenu, Base, WorldMap, Battle, UIGallery) üretir; Build Settings'e ilk beşini ekler; en sonda doğrulama raporunu verir.
   - Tekrar çalıştırmak güvenlidir: var olan sahne/prefab silinmez, kopya üretilmez.
   - Rapor penceresinde "0 error" görmelisin. Uyarılar (WARN) kabul edilebilir; hatalar (ERROR) için Console'u bana gönder.
8. Ayrıca dene: **Tools > DEADZONE > Validate References** → "No problems found".

## C. Testleri çalıştır
9. Window > General > Test Runner > **EditMode** sekmesi > **Run All**.
   Beklenen: tüm testler yeşil. Başarısız olan olursa test adını + mesajı gönder. Testi silme/devre dışı bırakma.
   - `UiReferenceValidationTests` 7. adım yapılmadan başarısız olur; bu beklenen davranıştır.
   - "EditMode" sekmesi görünmüyorsa 3. adımdaki Test Framework kurulmamıştır.

## D. Oynanabilir prototipi dene
10. Project penceresinde `Assets/_Game/Scenes/Bootstrap` sahnesini aç, **Play**'e bas. (Base sahnesinden doğrudan Play de çalışır.)

Kontrol listesi (Play Mode, Game görünümü: 1080x1920 veya Simulator'da bir telefon seç):

| # | Ne yap | Beklenen |
|---|---|---|
| 1 | Oyunu başlat | Ana menü açılır: başlık, Oyna / Yeni Oyun / Dil butonları |
| 2 | **Oyna** | Base sahnesi açılır, askerî üs görünür (zemin, yollar, duvar, kuleler, 6 bina) |
| 3 | Fareyle sürükle (sol tuş basılı) | Kamera kayar; üs sınırlarının dışına çıkmaz |
| 4 | Fare tekerleği | Yakınlaş/uzaklaş (7–34 arası sınırlı) |
| 5 | Bir binaya tıkla | Alt kısımda bina adı + seviye paneli, binada seçim halkası |
| 6 | Boş zemine tıkla | Seçim kalkar |
| 7 | Üst çubuğa bak | Avatar, oyuncu adı, komutan seviyesi, savaş gücü, VIP, 6 kaynak sayacı |
| 8 | Avatara tıkla | "Komutan Profili" penceresi açılır (animasyonlu); Kapat veya dışa tıkla ile kapanır |
| 9 | Bir kaynak sayacının üstüne gel (mouse) | Tooltip: "Gıda: 500 / ..." |
| 10 | Sol: İnşaat, Araştırma, Görevler; sağ: Etkinlikler, Takvim, VIP, Günlük Ödül, Battle Pass, Özel Teklif | Hepsi **kilitli** görünür; dokununca "yakında" mesajı çıkar |
| 11 | Alt çubuk: Üs, Kahramanlar, Birlikler, Envanter, Dünya, İttifak, Mağaza | Yalnız **Üs** açık (kamerayı sıfırlar); diğerleri kilitli |
| 12 | Üst çubukta **Kaydet** | "Kaydedildi" bildirimi çıkar |
| 13 | Play'i durdur, tekrar başlat | Oyuncu adı/kaynaklar korunur |
| 14 | Ana menüde **Yeni Oyun** | Onay penceresi (Evet/Hayır); Evet → kayıt sıfırlanır, Base açılır |
| 15 | Ana menüde **Dil** | TR ↔ EN, tüm metinler değişir |
| 16 | Butonlara bas | Basma animasyonu (hafif küçülme) |

Mobil dokunma için: Game görünümünde Simulator'a geç (sol üstte "Game" yanındaki açılır menü) ve dokunmatik/pinch'i dene; asıl doğrulama gerçek cihazda yapılmalıdır.

## E. UI bileşen galerisi (geliştirici sahnesi)
11. **Tools > DEADZONE > Open UI Gallery**, sonra Play. Butonlarla Popup, Onay, Ödül, Yükleme ekranı ve Toast denenir; aşağıda Tab, ProgressBar, Hero/Item/Shop/Event kartları ve rozet örnekleri görünür. (Bu sahne Build Settings'te değildir.)

## F. Debug araçları (yalnız Play Mode)
- Tools > DEADZONE > Debug > **Add Test Resources**: her kaynağa +1000 (üst çubuk anında güncellenir).
- Tools > DEADZONE > Debug > **Spend Test Resources**: her kaynaktan 500 harcamayı dener (yetersizse dokunmaz).

## G. Git
12. Proje kökünde: `git init` · `git add .` · `git commit -m "PHASE 01.5: foundation + UI components"`

## Sorun giderme
- **Türkçe karakterler (İ, ş, ğ) kare/boş görünüyor:** TMP varsayılan fontunda eksik olabilir. Bana ekran görüntüsünü gönder; font fallback eklenecek.
- **Setup "TextMeshPro resources missing" diyor:** 4. adımı yap, aracı yeniden çalıştır.
- **Pembe/magenta objeler:** URP materyali sorunu; Edit > Rendering > Materials > Convert... değil, bana ekran görüntüsünü gönder.
- **UI tıklamaları çalışmıyor:** Sahnedeki EventSystem'de "Input System UI Input Module" olmalı; Active Input Handling (5. adım) kontrol et.
- **Sahneyi sıfırdan üretmek istersen:** Tools > DEADZONE > Recreate Base Scene (Overwrite) (onay ister; elle yaptığın değişiklikler gider).
