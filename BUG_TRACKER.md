# BUG_TRACKER
| ID | Durum | Açıklama |
|---|---|---|
| B-001 | ÇÖZÜLDÜ | Faz 02: `BuildingWorldLabel`/`HudFeature` eski API (CS0618/CS0619) → düzeltildi, derleme temiz. |
| B-002 | ÇÖZÜLDÜ | Bina bilgi sayfasında ilerleme/neden metni düğmelerin üstüne biniyordu → yerleşim yeniden ayarlandı, `layoutVersion` ile eski sahneler otomatik yenilenir. |
| B-003 | ÇÖZÜLDÜ | Toast, uzun bilgi sayfasının arkasında kalıyordu → 470 → 1140 ve son kardeş yapıldı (`BuildingUiTools.PatchBaseScene`). |
| B-004 | ÇÖZÜLDÜ | Varsayılan kamera yakınlığı portrede fazla yakındı → startZoom 14 → 22. |
| B-005 | AÇIK | Çevrimdışıyken biten inşaatın bildirimi gösterilmiyor: `ConstructionCompleted` olayı HUD abone olmadan önce (oyun başlangıcında) tetikleniyor. Seviye ve kaynaklar doğru; yalnız toast eksik. |
| B-006 | AÇIK | Onay penceresinin (ConfirmationDialog) uzun metinlerde taşma riski var; mevcut metinler sığıyor. |
| B-007 | AÇIK | Safe Area gerçek çentikli cihazda denenmedi (simülatör/Game görünümü yalnız). |
| B-008 | AÇIK | Topla/İptal toast'ları Play'de ekran görüntüsünde yakalanamadı (kısa süreli); gösterimi doğrulanmadı, işlevleri (kaynak artışı) doğrulandı. |
| B-009 | AÇIK (denge) | ~L18 üstü HQ/bina maliyeti cüzdan kapasitesini (100.000) aşar; Depo işlevsel değil → karşılanamaz. Bkz. ECONOMY_BALANCE.md §8. |
| B-010 | BİLGİ | Konsolda "Error reason is 'NoSubscription'" (generators.ai.unity.com) Unity AI Assistant gürültüsüdür; projeyle ilgisiz. |
| B-011 | BİLGİ | Cihaz saatini ileri almak çevrimdışı ilerlemeyi hızlandırır (yerel saat sınırı, ECONOMY_BALANCE.md §7); geri alma korunur. |
| B-012 | AÇIK | PlayMode otomatik testi yok. |

## Bilinen riskler
- Gerçek cihazda performans/dokunma hissi ölçülmedi.
- Proje OneDrive içinde: Library/Temp senkron sorunu riski.
