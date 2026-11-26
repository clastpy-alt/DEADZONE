# ART_DIRECTION — DEADZONE: LAST EMPIRE (PHASE 02.5)

## 1. Vizyon
Modern askerî zombi kıyameti. Yarı gerçekçi (semi-realistic), PBR materyalli, sinematik ama mobil bütçesine uygun. Oyuncu "ayakta kalan, tahkim edilmiş bir askerî üs" hissini ilk bakışta almalı: betonarme, çelik, kumanda kulesi, radar, helikopter pisti — tehlikeli bir dünyada düzenli ve işlevsel bir kale.
- Düz kutu (flat-box) bina YOK: her bina kendi silüetine, katmanlı gövdesine ve fonksiyonel detaylarına sahiptir.
- Özgünlük: referans oyunlardan yalnız mekanik ilham; şekil, isim, renk şeması ve varlık kopyalanmaz.
- Çizgi film / low-poly oyuncak görünümü hedef DEĞİL.

## 2. Renk paleti (UIColors ile aynı)
| Rol | Hex |
|---|---|
| Arka plan / en koyu | `#101419` |
| Panel / koyu çelik | `#1C232B` |
| Askerî yeşil | `#465744` |
| Altın vurgu | `#D4AF63` |
| Gümüş / açık metal | `#ADB8C4` |
| Tehlike kırmızısı | `#D94B4B` |
| Başarı yeşili | `#4CAF72` |

Kullanım oranı (3D sahne): %60 nötr koyu (beton, asfalt, çelik), %25 askerî yeşil/haki, %10 gümüş metal, %4 altın (HQ/yüksek seviye vurguları), %1 kırmızı (uyarı ışıkları, çürüme). Doygun renk yalnız etkileşim ve durum göstergelerinde.

## 3. Materyal dili (PBR)
Ortak URP/Lit kütüphanesi (02.57): `Concrete_*` (açık/koyu/lekeli), `SteelPainted_Olive`, `SteelPainted_Gray`, `MetalWorn`, `Asphalt`, `Rust`, `Glass_Dark`, `Fabric_Canvas`, `Rubber`, `HazardStripe`, `EmissiveWindow`, `EmissiveWarning`.
- Metallic: boyalı çelik 0.4–0.6, çıplak metal 0.8–0.9, beton/asfalt 0.0.
- Smoothness: beton 0.15–0.25, boyalı metal 0.35–0.5, cam 0.8, asfalt 0.2 (ıslak varyantta 0.5).
- Doku yerine mümkün olduğunca renk + küçük tiling detay/normal; bütçe için `PERFORMANCE_BUDGET.md` §4.

## 4. Bina silüetleri (tanınabilirlik)
| Bina | Silüet / ana unsur |
|---|---|
| Karargâh (HQ) | Yüksek merkez blok + kumanda kulesi + anten/radar çanağı, altın şerit, bayrak direği; üstte helikopter pistine bağlantı |
| Kışla | Uzun, alçak, tekrarlayan pencere dizisi; ön avlu, çadır/bölme, sancak |
| Hastane | Beyaz/açık gövde, kırmızı haç, helikopter platformu, ambulans park alanı |
| Araştırma Laboratuvarı | Kubbe/yarım silindir, cam cephe, mavi/yeşil yayan çizgi, bacalar |
| Gıda Çiftliği | Seralar/ekim yatakları, silo, su kulesi, kasa yığınları |
| Çelik Fabrikası | Baca, endüstriyel hangar, vinç, konveyör, turuncu ısı yayan detay |
| Depo | Büyük hangar, sıralı konteynerler, forklift/palet |

Her silüet izometrik 3/4 açıdan 64 px küçüklükte bile ayırt edilebilmelidir.

## 5. Bina görsel kademeleri (02.55)
7 kademe, seviyeye göre (35 seviye eşlemesi):
1. **L1–5 Hayatta kalma:** derme çatma, yarı hasarlı, geçici örtüler
2. **L6–10 Onarılmış**
3. **L11–15 Güçlendirilmiş:** çelik takviye, çit, ek bloklar
4. **L16–20 Askerî standart**
5. **L21–25 Gelişmiş:** ek modüller, antenler, aydınlatma
6. **L26–30 Elit:** zırhlı paneller, kule, altın vurgular
7. **L31–35 Nihai kale:** tam silüet, yayan detaylar, özel efekt noktaları
Kademeler aynı prefab içinde açılıp kapanan alt gruplarla yapılır (yeni draw call'ı en aza indirmek için materyal paylaşımı). Bina kimliği, seviye ve kayıt DEĞİŞMEZ.

## 6. Ortam
Betonlu üs zemini, asfalt yollar (şerit çizgili), çit/duvar, gözetleme kulesi, sandbag, jersey bariyer, jeneratör, yakıt tankı, ışıklı lambalar, sınırda çürümüş/solmuş bitki örtüsü ve uzak zombi tehdidi (kırık araç, tabela). Kompozisyon: merkezde HQ, çevresinde işlevsel kümeler (askerî / üretim / depolama), yollarla bağlı.

## 7. Işık ve atmosfer
Sıcak-soğuk kontrastı: soğuk mavi-gri gölge, sıcak altın/turuncu vurgu. Tek yönlü ana ışık (alçak açı, uzun gölge), yumuşak ortam ışığı, hafif sis (yalnız Orta/Yüksek), vignette hafif. Gece/gündüz sonraki fazlarda; bugün tek "alacakaranlık" profili. Düşük/Orta/Yüksek profilleri `PERFORMANCE_BUDGET.md` §3.

## 8. UI görsel dili
Koyu panel + altın dış çizgi + gümüş bevel; premium HUD (02.60): temiz kaynak şeridi, bildirim rozetleri, minimum görsel gürültü. Detaylar `UI_DESIGN_SYSTEM.md`.

## 9. Yasaklar
- Başka oyunların model/doku/ikon/logo kopyası.
- Lisansı belirsiz varlık (her harici varlık `ASSET_REGISTER.md`'ye lisansıyla işlenir).
- Gereksiz yüksek poligon, 4096 doku, her bina için ayrı materyal.
- Gerçek bir kuruma/markaya ait amblem.
