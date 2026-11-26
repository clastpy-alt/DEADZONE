# BLENDER_PIPELINE — DEADZONE: LAST EMPIRE (PHASE 02.5)

## 1. Durum (dürüst rapor)
- **Blender bu bilgisayarda TESPİT EDİLEMEDİ.** Açık uygulama listesinde yok; `C:\Program Files` altındaki kurulumu klasör erişim isteği reddedildiği için kontrol EDİLEMEDİ (erişim kısıtını aşmaya çalışılmadı). Yani "yüklü değil" kesin değil: "görülemedi".
- Hiçbir yazılım izinsiz kurulmaz, ücretli hizmet satın alınmaz. Blender ücretsiz ve açık kaynaktır (GPL); yine de **kurulum kullanıcı iznine bağlıdır**.
- Bu nedenle PHASE 02.5 modelleri **Unity içinde, Editor kodu ile prosedürel/primitif-birleşimli** üretilir (idempotent menü araçları). Bu yöntem gerçek bir sanatçı modeli kalitesine ULAŞMAZ; PBR materyaller, katmanlı gövdeler, detay kümeleri ve LOD ile "düz kutu"dan belirgin biçimde iyi, ancak PHASE 09'da elle modellenmiş varlıklarla değiştirilmesi gereken **ara çözümdür**.

## 2. Blender kullanılabilirse (kullanıcı onaylarsa)
Kurulum (kullanıcı yapar veya izin verir): https://www.blender.org/download/ — LTS sürüm, Windows x64. Kurulumdan sonra:
1. Blender'ı bir kez aç (ilk yapılandırma).
2. Yolu belirt (örn. `C:\Program Files\Blender Foundation\Blender 4.x\blender.exe`) ve klasör erişimi ver.
3. Komut satırı doğrulama: `blender --background --version`.

## 3. Hedef boru hattı (kurulumdan sonra)
```
Blender (.blend, kaynak)  ->  FBX/GLB dışa aktarım  ->  Assets/_Game/Art/Models/<Bina>/  ->  Unity import ayarları
```
- Birim: metre, ölçek 1; Z yukarı → Unity +Y (FBX "Apply Transform").
- Pivot: bina tabanının ortası. İsimlendirme: `Bld_<Id>_LOD0/1/2`, `Bld_<Id>_Tier<N>_<Parça>`.
- UV: 1 kanal, malzeme başına paylaşımlı atlas; lightmap UV gerekmez (dinamik ışık).
- Poligon hedefleri (üçgen): HQ LOD0 ≤ 12k, diğer binalar ≤ 8k, prop ≤ 500; LOD1 ≈ %50, LOD2 ≈ %20.
- Materyaller Unity'de ortak kütüphaneden atanır (`ART_DIRECTION.md` §3); Blender materyalleri yalnız ad eşlemesi içindir.
- Her model `ASSET_REGISTER.md`'ye kaynak/lisans ile işlenir. Harici (indirilen) modeller yalnız açık lisanslıysa (CC0/CC-BY) ve ticari kullanıma uygunsa; lisans kanıtı kaydedilir.

## 4. Doğrulanmamış betik taslağı
`Tools/Blender/export_fbx.py` (Blender yok → ÇALIŞTIRILMADI, DOĞRULANMADI): sahnedeki `Bld_*` koleksiyonlarını FBX'e dışa aktarır.
```python
import bpy, os
out = os.path.join(bpy.path.abspath("//"), "export")
os.makedirs(out, exist_ok=True)
for col in bpy.data.collections:
    if not col.name.startswith("Bld_"): continue
    bpy.ops.object.select_all(action='DESELECT')
    for o in col.all_objects: o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(out, col.name + ".fbx"),
        use_selection=True, apply_scale_options='FBX_SCALE_ALL',
        bake_space_transform=True, mesh_smooth_type='FACE')
```
Blender kurulunca ilk iş bu betik çalıştırılıp doğrulanır; çalışmayan betik "başarılı" raporlanmaz.

## 5. Unity import ayarları (modeller geldiğinde)
Read/Write kapalı, Generate Colliders kapalı (collider'ı kodla/prefabda ekleriz), Optimize Mesh açık, Normals Import, Material: ortak kütüphane (Use External / Remap), Mesh Compression Medium, LOD Group otomatik atama.

## 6. Mevcut modellerin korunması
Bina kimlikleri, seviyeler, kayıtlar, collider ve `BuildingSelectable` seçimi DEĞİŞMEZ. Yeni görsel model, mevcut bina prefab kökü altında yeni bir `Visual` alt nesnesi olarak yerleştirilir; eski primitif görsel yalnız yeni model doğrulandıktan sonra devre dışı bırakılır (silinmez).


## Durum (PHASE 02.5 sonu)
Blender hâlâ kullanılmadı; tüm modeller Unity içinde prosedürel üretildi. Kurulum yalnız kullanıcı onayıyla.
