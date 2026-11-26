# NEXT_TASK — Sıradaki görev

Güncelleme: 2026-11-26 · Faz: PHASE 02 tamamlandı (v0.2.0) · **PHASE 03'e kullanıcı komutu olmadan GEÇİLMEZ.**

Yeni oturumda sırayla oku: CLAUDE.md → bu dosya → TASK_STATUS.md → DEVELOPMENT_LOG.md (Oturum 4). Projeyi baştan analiz etme.

## Durum
- Derleme 0 hata; EditMode 105/105; Play'de bina/ekonomi/yükseltme/kuyruk akışı doğrulandı (bkz. TASK_STATUS.md).
- Git: GitHub Desktop ile commit/push (commit mesajı sonuna Co-Authored-By ve Claude-Session satırları).

## Faz 02 küçük açık işler (istenirse, PHASE 03'ten ÖNCE)
1. B-005: çevrimdışı biten inşaat toast'ı — `HudController` abone olduktan sonra bekleyen tamamlanmaları göster (ör. `Game` tamamlananları listede tutup HUD açılışında tüketsin).
2. Otomatik PlayMode testi: Base sahnesi yüklenir → HUD + kamera var → bina seç → yükselt → kayıt döngüsü.
3. B-009 denge: Depo binasını işlevsel yap veya maliyet eğrisini yumuşat (ECONOMY_BALANCE.md §8).
4. Safe Area / dokunma / performans: gerçek cihaz (veya Device Simulator) denemesi.
5. Yeni Oyun (test kaydında) ile başlangıç ekonomisini UI'da gör; EN dil kontrolü.

## PHASE 03 (yalnız kullanıcı onayıyla): bkz. MASTER_PROMPT.md yol haritası
Planlı 16 bina için sırayla işlev eklerken `BuildingDefinition.availability = Functional` yap, `FeatureRouter`/UI kilidini kaldır, `PlayerPower.Register` ile araştırma/kahraman/birlik güçlerini bağla.

## Kurallar hatırlatması
- Test silerek/gevşeterek geçirme. Doğrulanmayan şeyi "tamamlandı" yazma.
- Mevcut çalışan sistemleri gereksiz değiştirme; kaynak değişimi yalnız ResourceWallet; kayıt/yedek asla silinmez.
- Eski DEADZONE projesine dokunma. Gerçek ödeme/sunucu/mağaza: yalnız kullanıcı onayıyla.
- Her oturum sonunda: DEVELOPMENT_LOG.md, TASK_STATUS.md, NEXT_TASK.md güncelle.
