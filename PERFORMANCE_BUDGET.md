# PERFORMANCE_BUDGET — DEADZONE: LAST EMPIRE (PHASE 02.5, v0.2.5)

Bu belge KALICI bir kuraldır. Her yeni varlık, özellik ve faz bu bütçeye göre değerlendirilir.
**Dürüstlük kuralı:** Aşağıdaki hedefler HEDEFTİR. Gerçek cihaz / Android build ölçümü yapılmadan hiçbir hedef "karşılandı" diye raporlanmaz. Bu makinede (Windows editör) alınan ölçümler cihaz ölçümü sayılmaz.

## 1. Platform hedefi
- Android öncelikli (iOS sonra). Minimum cihaz: **4 GB RAM**.
- Kare hızı: **düşük kalite 30 FPS**, **yüksek kalite 60 FPS** (hedef).
- Portrait 1080x1920, izometrik 3D kamera, URP.
- İlk indirme: **300–500 MB**. Geri kalan içerik sonradan (Addressables) iner.

## 2. Toplam depolama bütçesi: 3 GB
| Kalem | Bütçe (MB) |
|---|---|
| Doku / materyal | 900 |
| 3D modeller | 650 |
| Animasyonlar | 300 |
| Dünya / ortam | 300 |
| Ses | 250 |
| Motor + kod | 200 |
| UI + efektler | 100 |
| Gelecek güncellemeler (rezerv) | 300 |
| **Toplam** | **3000** |

Bütçeyi aşan her varlık eklenmeden önce kullanıcıya bildirilir. Faz sonunda gerçek boyutlar `Generate Performance Report` ile raporlanır ve bu tabloyla karşılaştırılır (ölçülen değer / bütçe).

## 3. Çalışma zamanı bütçeleri (hedef; ölçülecek)
| Metrik | Düşük | Orta | Yüksek |
|---|---|---|---|
| Hedef FPS | 30 | 45–60 | 60 |
| Gölge | kapalı / çok yakın | tek ışık, 1 cascade | tek ışık, 2 cascade |
| Render ölçeği | 0.75 | 0.9 | 1.0 |
| MSAA | kapalı | 2x | 4x |
| Post-process | kapalı | hafif (renk) | renk + hafif bloom |
| Ek ışık | 0 | 0 | ≤1 (yalnız vurgu) |
| Üs sahnesi üçgen sayısı (hedef) | ≤150 bin | ≤250 bin | ≤400 bin |
| Üs sahnesi draw call (hedef) | ≤150 | ≤200 | ≤300 |

Sayılar tasarım hedefidir; gerçek değerler Frame Debugger / Profiler / Android cihaz ölçümüyle doğrulanana kadar "ölçülmedi" kalır.

## 4. Kalıcı teknik kurallar
1. **Sıkıştırma:** Android dokular ASTC (6x6 varsayılan; UI ve normal haritada 4x4), mipmap açık (UI hariç). Gereksiz doku yok; 2'nin kuvveti boyutlar.
2. **Doku boyutu:** bina/prop başına en fazla 1024²; zemin/ortak atlaslar 2048². 4096 yasak (istisna için kullanıcı onayı).
3. **Atlas ve paylaşım:** ortak materyal kütüphanesi (askerî metal, beton, çürümüş, cam, kumaş, boya). Binalar aynı materyalleri paylaşır; materyal başına yeni doku değil, **renk/mask çeşitlemesi**.
4. **LOD:** her büyük model LOD0 (yakın) / LOD1 (≈%50) / LOD2 (≈%20) ya da cull. Üs izometrik uzaktan baktığı için LOD1 çoğu zaman yeterli.
5. **Instancing / batching:** GPU instancing açık materyaller, statik prop'lar `BatchingStatic`, SRP Batcher uyumlu shader (URP/Lit). Kendi shader'ımız yok; Shader Graph'ı yalnız ölçüm gerektirirse.
6. **Havuzlama:** tekrarlayan nesneler (VFX, projectile, toast, dünya rozetleri) havuzdan; çalışma anında `Instantiate/Destroy` döngüsü yok.
7. **Addressables'a hazırlık:** büyük içerik (ileride: biyomlar, kahraman modelleri, ses) Resources yerine kataloglanabilir yapıda; `Resources` yalnız `GameConfig` ve `BuildingDatabase` için kalır. Addressables paketi, ilk indirme bütçesi gerçekten gerektirdiğinde (PHASE 03+) eklenir.
8. **Gölge:** tek yönlü ışık; Mobile pipeline'da yumuşak gölge yalnız Yüksek'te.
9. **Update döngüsü:** `Update` içinde `Find*`, LINQ, string birleştirme (GC) yok; UI metinleri yalnız değişince yazılır.
10. **Kod/UI:** tek Canvas'ta çok animasyonlu alt öğe yok; sık değişen öğeler ayrı Canvas'ta (HUD sayaçları).

## 5. Ölçüm planı (yapılacak; şu an YAPILMADI)
| Ölçüm | Nasıl | Durum |
|---|---|---|
| Android development build boyutu | `Tools > DEADZONE > Build Development APK`, APK/AAB boyutu | ÖLÇÜLMEDİ |
| Gerçek cihaz FPS (düşük/orta/yüksek) | Cihazda Profiler / Unity Stats / `adb shell dumpsys gfxinfo` | ÖLÇÜLMEDİ |
| RAM kullanımı (4 GB cihaz) | Memory Profiler, `adb shell dumpsys meminfo` | ÖLÇÜLMEDİ |
| Draw call / üçgen sayısı (editör) | Game > Stats, Frame Debugger — editör değeri, cihaz değeri DEĞİL | Editörde ölçülebilir |
| Isınma / pil | 10 dk oyun sonrası cihaz sıcaklığı | ÖLÇÜLMEDİ |
| Doku/model gerçek boyutları | `Generate Performance Report` (Assets klasörü boyut dökümü) | Rapor aracı 02.63'te |

Her ölçüm yapıldığında bu tabloya tarih, cihaz modeli, Android sürümü, kalite profili ve sayı eklenir. Ölçüm yoksa satır "ÖLÇÜLMEDİ" kalır.

## 6. Bütçe ihlali süreci
1. Varlık bütçeyi aşıyorsa: önce sıkıştır / LOD ekle / paylaş.
2. Hâlâ aşıyorsa: kullanıcıya bildir, alternatif sun (düşük poli, atlas, Addressables).
3. Kullanıcı onayı olmadan bütçe değiştirilmez.


## 7. PHASE 02.5 statik analiz (PERFORMANCE_REPORT.md, editör; cihaz ölçümü DEĞİL)
Assets/_Game kaynak boyutu 15,2 MB (bütçe 3000 MB). Base sahnesi: 593 aktif MeshRenderer, 191.676 üçgen (LOD0, tüm katmanlar), 51 materyal. Draw call/FPS/RAM: ÖLÇÜLMEDİ. Low/Medium/High ayarları (hedef 30/45/60 FPS, LOD bias 0,7/1,0/1,4) uygulanır ama hedefler cihazda doğrulanmadı.
