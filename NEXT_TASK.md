# NEXT_TASK — Sıradaki görev

Güncelleme: 2026-11-26 · Faz: PHASE 02.5 devam ediyor (Grup 1 / 02.50 tamam, v0.2.5 hedefi) · **PHASE 03'e kullanıcı komutu olmadan GEÇİLMEZ.**

Yeni oturumda sırayla oku: CLAUDE.md → bu dosya → TASK_STATUS.md → DEVELOPMENT_LOG.md (Oturum 4). Projeyi baştan analiz etme.

## Durum
- Derleme 0 hata; EditMode 108/108, PlayMode 9/9; Play'de bina/ekonomi/yükseltme/kuyruk akışı doğrulandı (bkz. TASK_STATUS.md).
- Git: GitHub Desktop ile commit/push (commit mesajı sonuna Co-Authored-By ve Claude-Session satırları).

## PHASE 02.5 sıradaki işler (kullanıcının PHASE 02.5 isteminden)
1. 02.51 PERFORMANCE_BUDGET.md + CLAUDE.md/MASTER_PROMPT.md/GAME_DESIGN.md kalıcı kuralları → commit.
2. 02.52 ART_DIRECTION.md; 02.53 BLENDER_PIPELINE.md (Blender yok; kurulum için kullanıcı izni gerekir).
3. 02.54+ HQ modeli önce (Unity'de doğrula), sonra diğer binalar; kademe sistemi; ortam; materyaller; ışık/profiller; kamera; HUD; bina UI (dünya etiketlerini küçük rozet yap); grafik ayarları; menü araçları; QA (8 ekran görüntüsü 1080x1920); dokümanlar.
4. Açık: B-008 toast görsel doğrulaması; Safe Area ve gerçek cihaz/Android FPS ölçümü (ölçülmeden başarı raporlanmaz).

## PHASE 03 (yalnız kullanıcı onayıyla): bkz. MASTER_PROMPT.md yol haritası
Planlı 16 bina için sırayla işlev eklerken `BuildingDefinition.availability = Functional` yap, `FeatureRouter`/UI kilidini kaldır, `PlayerPower.Register` ile araştırma/kahraman/birlik güçlerini bağla.

## Kurallar hatırlatması
- Test silerek/gevşeterek geçirme. Doğrulanmayan şeyi "tamamlandı" yazma.
- Mevcut çalışan sistemleri gereksiz değiştirme; kaynak değişimi yalnız ResourceWallet; kayıt/yedek asla silinmez.
- Eski DEADZONE projesine dokunma. Gerçek ödeme/sunucu/mağaza: yalnız kullanıcı onayıyla.
- Her oturum sonunda: DEVELOPMENT_LOG.md, TASK_STATUS.md, NEXT_TASK.md güncelle.
