# BUG_TRACKER
| ID | Durum | Açıklama |
|---|---|---|
| B-001 | ÇÖZÜLDÜ | Faz 02: `BuildingWorldLabel`/`HudFeature` eski API (CS0618/CS0619) → düzeltildi, derleme temiz. |
| B-002 | ÇÖZÜLDÜ | Bina bilgi sayfasında ilerleme/neden metni düğmelerin üstüne biniyordu → yerleşim yeniden ayarlandı, `layoutVersion` ile eski sahneler otomatik yenilenir. |
| B-003 | ÇÖZÜLDÜ | Toast, uzun bilgi sayfasının arkasında kalıyordu → 470 → 1140 ve son kardeş yapıldı (`BuildingUiTools.PatchBaseScene`). |
| B-004 | ÇÖZÜLDÜ | Varsayılan kamera yakınlığı portrede fazla yakındı → startZoom 14 → 22. |
| B-005 | ÇÖZÜLDÜ (02.5) | Çevrimdışı biten inşaat bildirimi: `Game.OfflineCompletions` listesi tutuluyor, HUD açılışta tüketip toast gösteriyor. PlayMode testi (`SaveReload_KeepsProgress_AndOfflineCompletionIsReported`) doğruladı. |
| B-006 | AÇIK | Onay penceresinin (ConfirmationDialog) uzun metinlerde taşma riski var; mevcut metinler sığıyor. |
| B-007 | AÇIK | Safe Area gerçek çentikli cihazda denenmedi (simülatör/Game görünümü yalnız). |
| B-008 | AÇIK (kısmi) | Topla/İptal toast'ı süresi 2.6 sn'ye çıkarıldı; hâlâ ekran görüntüsünde elle doğrulanmadı (Play'de Topla'ya basılıp anında görüntü alınmalı). İşlevleri (kaynak artışı) testlerle doğrulandı. |
| B-009 | ÇÖZÜLDÜ (02.5) | Depo işlevsel (depolama 20.000 × 1.42^(L-1)); `StorageTooSmall` nedeni, kapasiteyi aşan maliyeti açık mesajla bildirir; Validate Economy depolama ilerlemesini denetler. |
| B-010 | BİLGİ | Konsolda "Error reason is 'NoSubscription'" (generators.ai.unity.com) Unity AI Assistant gürültüsüdür; projeyle ilgisiz. |
| B-011 | BİLGİ | Cihaz saatini ileri almak çevrimdışı ilerlemeyi hızlandırır (yerel saat sınırı, ECONOMY_BALANCE.md §7); geri alma korunur. |
| B-012 | ÇÖZÜLDÜ (02.5) | Otomatik PlayMode testleri eklendi (9 test, 9/9 geçti). |
| B-013 | ÇÖZÜLDÜ (02.5) | Seçim durumu (`BuildingSelectionController`) hiçbir sahnede bileşen olmadığı için tutulmuyordu; bilgi panelinin Kapat tuşu seçim halkasını temizlemiyordu. Durum artık `GameEvents.RaiseBuildingSelected` içinde statik tutuluyor (PlayMode testi buldu). |
| B-014 | ÇÖZÜLDÜ (02.5) | Yeni kayıt başlangıç zamanını gerçek sistem saatinden alıyordu (`PlayerData.CreateNew`); oyun saatiyle 1 sn kayma → üretim 1199 yerine 1200. `SaveService.Clock`/`CreateNew(config, nowUtc)` ile tek zaman kaynağı. |
| B-015 | BİLGİ | Test Runner ayrı pencerede açıkken Unity ana pencere odak almadığı için yeni .cs dosyaları derlenmez; Test Runner'ı kapatıp ana pencerede Ctrl+R gerekir (OneDrive klasörü). |

## Bilinen riskler
- Gerçek cihazda performans/dokunma hissi ölçülmedi.
- Proje OneDrive içinde: Library/Temp senkron sorunu riski.
