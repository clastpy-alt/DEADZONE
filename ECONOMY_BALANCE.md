# ECONOMY_BALANCE — Faz 02 ekonomi ve denge (v0.2.0)

Tüm değerler `Assets/_Game/Resources/BuildingDatabase` + `ScriptableObjects/Buildings/Building_*.asset` içindedir; çalışma anında kod sabiti yoktur. Tohum değerleri yalnızca `Editor/BuildingDatabaseGenerator.cs` içindedir. Satır bazında düzeltme için asset'te `levels` listesi elle düzenlenebilir; formülden yeniden üretmek için Tools > DEADZONE > Generate Building Level Data.

## 1. Başlangıç ekonomisi (yalnızca YENİ kayıtlar)
| Kaynak | Miktar |
|---|---|
| Gıda / Odun / Çelik | 5000 / 5000 / 5000 |
| Yakıt | 2500 |
| Altın | 1000 |
| Elmas | 200 |
| Enerji | 100 |

`GameConfig.DefaultStartingResources()`; `PlayerData.CreateNew` kullanır. **Var olan kayıtlara dokunulmaz** (PHASE 01 kaydı Çelik 3K/Yakıt 2K/Altın 10K/Elmas 100 ile devam eder). Test: `EconomyTests.StartingEconomy_OfNewSaves_MatchesTheTestEconomy`.
Kapasite: kaynak başına 100.000 (Gıda/Odun/Çelik/Yakıt, Depo ile artar); Enerji 120; Altın/Elmas/İttifak Kredisi/VIP Puanı 999.999.999.

## 2. Seviye formülleri (`BuildingBalanceCurve.BuildLevels`)
- **Maliyet** (seviye N'e ulaşmak için, N ≥ 2): `baseCost × 1.4^(N−2)`, 10'a yuvarlanır.
- **Süre** (saniye): N=2..5 için 30 / 120 / 300 / 900 (yaklaşık 30 sn, 2 dk, 5 dk, 15 dk); N ≥ 6 için `900 × 1.25^(N−5)`.
- **Güç**: `powerBase × N^1.4`.
- **Üretim / saat**: `productionBase × 1.18^(N−1)`. **Depolama**: `storageBase × 1.2^(N−1)`.
- **HQ sınırı**: HQ dışındaki her bina için N. seviyeye çıkmak HQ ≥ `max(1, N−1)` ister (hqLevelOffset = 1). HQ'nun kendisi için sınır yoktur.
- Azami seviye 35 (`BuildingDefinition.AbsoluteMaxLevel`).

## 3. İşlevsel 6 bina — tohum değerleri
| Bina | Güç tabanı | Üretim/sa (L1) | Depolama (L1) | L2 maliyeti |
|---|---|---|---|---|
| Karargâh (HQ) | 120 | – | – | Çelik 400, Odun 300, Altın 80 |
| Kışla | 60 | – | – | Gıda 250, Odun 250, Çelik 150 |
| Hastane | 50 | – | – | Gıda 200, Odun 150, Çelik 150 |
| Araştırma Lab. | 60 | – | – | Çelik 250, Odun 150, Altın 100 |
| Gıda Çiftliği | 20 | 1200 Gıda | 4000 | Odun 250, Çelik 100 |
| Çelik Fabrikası | 25 | 900 Çelik | 3000 | Gıda 200, Odun 250, Altın 40 |

Diğer 16 bina veri olarak vardır (22 tür), `availability = Planned`: seviye tablosu üretilmiştir ama yükseltilemez (UI'da kilitli). Depo (`warehouse`) tamamlanınca `storageIsGlobal` ile cüzdan kapasitesini artırır (kod ve test hazır: `EconomyTests.Warehouse_*`).

## 4. Örnek değerler (HQ ve Çiftlik, formülden hesaplandı)
| Seviye | Süre | HQ Çelik | HQ Odun | Çiftlik Odun | Güç (taban 50) | Çiftlik üretim/sa |
|---|---|---|---|---|---|---|
| 2 | 30 sn | 400 | 300 | 250 | 132 | 1.416 |
| 3 | 2 dk | 560 | 420 | 350 | 233 | – |
| 4 | 5 dk | 780 | 590 | 490 | 348 | – |
| 5 | 15 dk | 1.100 | 820 | 690 | 476 | 2.327 |
| 10 | 45 dk 47 sn | 5.900 | 4.430 | 3.690 | 1.256 | 5.323 |
| 20 | 7 sa 6 dk | 170.750 | 128.060 | 106.720 | 3.314 | 27.857 |
| 35 | 8 g 9 sa 56 dk | 26.563.490 | 19.922.620 | 16.602.180 | 7.255 | 333.557 |

Tüm seviyelerin (L1→L35) toplam süresi ≈ 42 gün (tek yuva, tek bina).

## 5. Üretim, toplama, çevrimdışı
- Üretim saniye hassasiyetli; yarım birim biriktirilir, aynı saniye iki kez sayılmaz (çıpa ilerler).
- Bina kendi çıktı deposunda biriktirir; **Topla** cüzdana taşır. Cüzdan doluysa kalan bina içinde kalır (kayıp yok). Bina deposu doluysa üretim boşa gider (biriktirilmez).
- Çevrimdışı üretim üst sınırı: `GameConfig.maxOfflineHours` = **8 saat**.
- Yükseltme sırasında üretim eski seviyeyle bitiş anına kadar, sonrası yeni seviyeyle hesaplanır (`EconomyTests.Production_UsesOldLevelUntilUpgradeFinishes_ThenNewLevel`).

## 6. Yükseltme kuralları
- Kuyruk: **1 yuva** (`constructionQueueSlots`); VIP kancası `BuildingUpgradeService.ExtraQueueSlots`.
- İptal: ödenen maliyetin **%80'i** iade (`cancelRefundPercent`); iade cüzdan kapasitesiyle sınırlıdır. Aynı iş iki kez iptal/iade edilemez.
- Anında bitirme: kalan sürenin her **başlanmış dakikası = 1 Elmas** (`instantFinishDiamondsPerMinute`); elmas yoksa hiçbir şey olmaz.
- Hızlandırma öğeleri için `ApplySpeedup(buildingId, saniye)` hazır (öğe sistemi henüz yok; elmas harcama yolu `TryInstantFinish`).
- Çift yükseltme/tamamlama/iade engelleri testlidir (bkz. TEST_PLAN.md).

## 7. Zaman güvenliği — yerel saat sınırları (dürüst not)
- Tüm zamanlar UTC unix saniyesidir ve tek kaynak `IGameClock`'tur (`SystemGameClock` = cihaz saati).
- `BuildingManager.Now()` tarihte gördüğü en yüksek değerin (`lastSeenUtc`) altına inmez: **saati geri almak** inşaatı uzatmaz/kısaltmaz, üretim kazandırmaz (testli: `ClockRollback_*`, `Accrue_ClockRollback_*`).
- **Saati ileri almak** çevrimdışı oyunda inşaatı hızlandırır ve üretimi (en fazla 8 saat/oturum) artırır. Cihaz saatine güvenen tek oyunculu prototipte bu engellenemez; bu bir hile koruması DEĞİLDİR. Kayıt checksum'ı da yalnız kazara bozulmayı yakalar.
- Çözüm yolu (Faz 08): `IGameClock`'a sunucu zamanı uygulaması takmak ve inşaat/üretimi sunucuda doğrulamak; oyun kodu değişmeden kalır.

## 8. Bilinen denge sorunları (Faz 02 sınırı)
- Maliyetler 1.4× üstel büyür; HQ Çelik maliyeti ≈ L19'da 100.000 kapasiteyi aşar. Depo bina henüz işlevsel olmadığından (Planned) yaklaşık L18 üstü yükseltmeler **karşılanamaz**. Depo/kapasite veya eğri düzeltmesi sonraki fazda yapılmalı.
- Süre eğrisi (×1.25) çok uzundur (L35 ≈ 8,4 gün); hızlandırma öğeleri ve VIP yuvaları eklenince yeniden dengelenmeli.
- Elmas hızlandırma oranı (1 elmas/dk) yer tutucudur; gerçek ekonomi ve satın alma Faz 06+.
