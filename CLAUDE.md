# DEADZONE: LAST EMPIRE — Proje Kuralları (kısa)

Unity 6 LTS · URP · C# · uGUI/TMP · yeni Input System · Portrait 1080x1920 · İzometrik 3D kamera.
Bu proje eski DEADZONE projesinden BAĞIMSIZDIR; eski dosyalar kullanılmaz/değiştirilmez.

## Her oturumda
1. Bu dosyayı, NEXT_TASK.md'yi (sıradaki görev), TASK_STATUS.md'yi ve DEVELOPMENT_LOG.md'nin son oturumunu oku. Projeyi baştan analiz etme.
2. Tam kapsam için MASTER_PROMPT.md (V3.0).

## Kurallar
- Kod/sınıf/değişken adları İngilizce; raporlar ve dokümanlar Türkçe.
- Tek GameManager yok. Küçük, bağımsız modüller; gereksiz soyutlama yok.
- Sabit oyun verisi (ScriptableObject) ile oyuncu verisi (PlayerData/JSON) ayrı tutulur.
- Kaynaklar yalnızca ResourceWallet üzerinden değişir (negatif bakiye ve geçersiz miktar engelli).
- Girdi yalnızca yeni Input System (UnityEngine.InputSystem). Eski `Input.*` kullanılmaz.
- Tüm UI metinleri Loc.T(key) ile (TR/EN). Sabit metin yazma.
- Çalışmayan özellik butonu KİLİTLİ görünür (FeatureRouter.IsAvailable).
- Placeholder geometri/ikonlar isimde "_PH" veya "[PLACEHOLDER]" ile işaretlenir.
- Editor araçları (Tools > DEADZONE) tekrar çalıştırılınca kopya üretmemeli; mevcut sahne/prefab korunur.
- Testler silinerek/devre dışı bırakılarak "geçirilmez". Test edilmemiş kod "başarılı" raporlanmaz.
- Gerçek ödeme, canlı sunucu, mağaza yayını: yalnızca kullanıcı onayıyla.
- Gizli anahtar istemci koduna yazılmaz. Proje dışı dosyalara erişilmez.

## Klasörler
Assets/_Game/{Scripts,Scenes,Prefabs,Art,ScriptableObjects,Resources,Tests}
Namespace: Deadzone.Core, .Data, .SaveSystem, .Economy, .Buildings, .UI, .EditorTools
