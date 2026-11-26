# ASSET_REGISTER
Tüm 3D modeller ve ikonlar PLACEHOLDER'dır: Unity primitiflerinden (Cube, Cylinder, Sphere, Capsule) Editor kodu ile üretilir; harici model, doku, ses, font veya ücretli varlık YOKTUR. Hepsi proje sahibine aittir (üretim kodu bu repodadır). Ticari yayın öncesi PHASE 09'da değiştirilecek.

| Varlık | Durum | Kaynak / Lisans |
|---|---|---|
| 7 işlevsel bina (HQ, Kışla, Hastane, Lab, Çiftlik, Çelik Fab., Depo) + `Details_PH` (pencere, kapı çerçevesi, saçak, havalandırma, anten, HQ çanağı/direği) | Placeholder | Unity primitifleri, kendi içeriğimiz |
| "Military Base Props [PLACEHOLDER]": helikopter pisti, varil kümeleri, çit, jeneratör, radar çanağı, çalılar | Placeholder | Unity primitifleri, kendi içeriğimiz |
| Duvar, yol, kule, lamba, ağaç, kamyon, tank, konteyner, ev | Placeholder | Unity primitifleri, kendi içeriğimiz |
| PHASE 02.5: 7 bina için `Mil_Visual` (Core + Tier1..7), birleştirilmiş mesh asset'leri `Art/Models/Generated/` (bina başına yaklaşık 30–35; toplam boyut henüz ölçülmedi) | Geçici (Blender yok) | Unity primitifleri + Editor kodu, kendi içeriğimiz |
| PHASE 02.5: 25 `Mil_*` materyal (renk + metallic/smoothness, doku yok) | Geçici | Unity'nin yerleşik URP/Lit shader'ı, kendi içeriğimiz |
| Malzemeler (URP/Lit, düz renk) | Placeholder | Unity'nin yerleşik URP shader'ı, kendi içeriğimiz |
| UI ikonları (binalarda 2 harflik etiket, ör. "FD") | Placeholder (metin) | Kendi içeriğimiz |
| Yazı tipi | Unity'nin TMP varsayılanı (LiberationSans SDF) | SIL OFL (Unity paketiyle gelir; üçüncü taraf ekleme yok) |
| Ses / VFX | Yok | — |

Mobil uyumluluk notları: URP, çoğu prop `BatchingStatic`, gölge ve ışık sayısı düşük; gerçek cihaz performansı ölçülmedi.

PHASE 02.5: Blender tespit edilemedi → modeller Unity içinde prosedürel üretilir (ara çözüm); Blender kurulumu kullanıcı izniyle (BLENDER_PIPELINE.md). Tüm harici varlıklar buraya lisansıyla işlenir; şu an harici varlık YOK.
