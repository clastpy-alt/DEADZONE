# PERFORMANCE_REPORT

Üretim: 2026-11-26 20:36 (Unity 6000.6.10f1). **Statik analiz**: proje dosyaları ve editörde açılan Base sahnesi. Cihaz FPS / RAM / ısınma ölçümü DEĞİLDİR; bu satırlar PERFORMANCE_BUDGET.md'de "ÖLÇÜLMEDİ" kalır.

## Proje varlık boyutları (Assets/_Game, kaynak dosya; build boyutu değil)
| Kategori | MB | Bütçe (MB) |
|---|---|---|
| Doku | 0,44 | 900 |
| 3D modeller (mesh) | 9,62 | 650 |
| Materyal | 0,29 | - |
| Ses | 0,00 | 250 |
| Prefab / sahne | 3,92 | - |
| Kod | 0,65 | 200 |
| Diğer | 0,28 | - |
| **Toplam** | **15,20** | 3000 |

## Base sahnesi (editör, aktif nesneler)
- Aktif MeshRenderer: 593 (statik: 478)
- Üçgen (LOD0, tüm bina katmanları dahil; kamera kırpması yok): 191.676 (bütçe: Düşük ≤150 bin, Orta ≤250 bin, Yüksek ≤400 bin)
- Benzersiz materyal: 51
- Draw call sayısı burada HESAPLANMAZ; Game > Stats / Frame Debugger ile editörde ölçülür.

## Grafik profilleri (ayar; hedef FPS ölçülmedi)
- Low: hedef 30 FPS, LOD bias 0,7, post-process kapalı, yumuşak gölge kapalı, sis kapalı
- Medium: hedef 45 FPS, LOD bias 1,0, post-process açık, yumuşak gölge kapalı, sis açık
- High: hedef 60 FPS, LOD bias 1,4, post-process açık, yumuşak gölge açık, sis açık
