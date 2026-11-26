using System.Collections.Generic;

namespace Deadzone.Core
{
    /// <summary>PHASE 02 strings: buildings, upgrade rules and construction UI. Each entry is { turkish, english }.</summary>
    public static partial class Loc
    {
        private static readonly Dictionary<string, string[]> BuildingTable = new Dictionary<string, string[]>
        {
            // ---- building names (the first six keys live in Loc.cs)
            { "building.tank_factory", new[] { "Tank Fabrikası", "Tank Factory" } },
            { "building.air_base", new[] { "Hava Üssü", "Air Base" } },
            { "building.training_ground", new[] { "Eğitim Sahası", "Training Ground" } },
            { "building.lumber_mill", new[] { "Kereste Fabrikası", "Lumber Mill" } },
            { "building.oil_refinery", new[] { "Petrol Rafinerisi", "Oil Refinery" } },
            { "building.gold_mine", new[] { "Altın Madeni", "Gold Mine" } },
            { "building.power_plant", new[] { "Enerji Santrali", "Power Plant" } },
            { "building.warehouse", new[] { "Depo", "Warehouse" } },
            { "building.radar_station", new[] { "Radar İstasyonu", "Radar Station" } },
            { "building.intelligence_center", new[] { "İstihbarat Merkezi", "Intelligence Center" } },
            { "building.alliance_center", new[] { "İttifak Merkezi", "Alliance Center" } },
            { "building.recruitment_center", new[] { "Askere Alma Merkezi", "Recruitment Center" } },
            { "building.trading_center", new[] { "Ticaret Merkezi", "Trading Center" } },
            { "building.watchtower", new[] { "Gözetleme Kulesi", "Watchtower" } },
            { "building.defense_turret", new[] { "Savunma Taretı", "Defense Turret" } },
            { "building.defensive_wall", new[] { "Savunma Duvarı", "Defensive Wall" } },

            // ---- descriptions
            { "building.hq.desc", new[] { "Üssün kalbi. Karargâh seviyesi diğer binaların ulaşabileceği en yüksek seviyeyi belirler.", "The heart of the base. Its level limits how high other buildings can be upgraded." } },
            { "building.barracks.desc", new[] { "Piyade birliklerini barındırır ve eğitir.", "Houses and trains infantry troops." } },
            { "building.tank_factory.desc", new[] { "Zırhlı birlikler üretir.", "Produces armored units." } },
            { "building.air_base.desc", new[] { "Helikopter ve hava birliklerini barındırır.", "Houses helicopters and air units." } },
            { "building.hospital.desc", new[] { "Yaralı askerleri tedavi eder.", "Treats wounded soldiers." } },
            { "building.training_ground.desc", new[] { "Birliklerin eğitim hızını artırır.", "Speeds up troop training." } },
            { "building.farm.desc", new[] { "Saatlik gıda üretir.", "Produces food every hour." } },
            { "building.lumber_mill.desc", new[] { "Saatlik odun üretir.", "Produces wood every hour." } },
            { "building.steel.desc", new[] { "Saatlik çelik üretir.", "Produces steel every hour." } },
            { "building.oil_refinery.desc", new[] { "Saatlik yakıt üretir.", "Produces fuel every hour." } },
            { "building.gold_mine.desc", new[] { "Saatlik altın üretir.", "Produces gold every hour." } },
            { "building.power_plant.desc", new[] { "Saatlik enerji üretir.", "Produces energy every hour." } },
            { "building.warehouse.desc", new[] { "Kaynak depolama kapasitesini artırır.", "Raises the resource storage capacity." } },
            { "building.lab.desc", new[] { "Yeni teknolojilerin araştırılmasını sağlar.", "Allows new technologies to be researched." } },
            { "building.radar_station.desc", new[] { "Düşman hareketlerini erkenden tespit eder.", "Detects enemy movement early." } },
            { "building.intelligence_center.desc", new[] { "Keşif ve istihbarat bilgisi sağlar.", "Provides scouting and intelligence." } },
            { "building.alliance_center.desc", new[] { "İttifak yardımı ve etkinlikleri için kullanılır.", "Used for alliance help and events." } },
            { "building.recruitment_center.desc", new[] { "Yeni kahramanların işe alınmasını sağlar.", "Allows new heroes to be recruited." } },
            { "building.trading_center.desc", new[] { "Kaynak takası ve ticaret sağlar.", "Allows resource trading." } },
            { "building.watchtower.desc", new[] { "Üssün çevresini gözetler.", "Watches the surroundings of the base." } },
            { "building.defense_turret.desc", new[] { "Saldırganlara karşı üssü savunur.", "Defends the base against attackers." } },
            { "building.defensive_wall.desc", new[] { "Üssü saldırılara karşı korur.", "Protects the base from attacks." } },

            // ---- building info panel
            { "bi.current", new[] { "Mevcut", "Current" } },
            { "bi.next", new[] { "Sonraki Seviye", "Next Level" } },
            { "bi.power", new[] { "Savaş Gücü", "Combat Power" } },
            { "bi.production", new[] { "Üretim", "Production" } },
            { "bi.storage", new[] { "Depolama", "Storage" } },
            { "bi.perhour", new[] { "/sa", "/h" } },
            { "bi.cost", new[] { "Geliştirme Maliyeti", "Upgrade Cost" } },
            { "bi.duration", new[] { "Süre", "Duration" } },
            { "bi.requirements", new[] { "Gereksinimler", "Requirements" } },
            { "bi.collect", new[] { "Topla", "Collect" } },
            { "bi.collected", new[] { "Toplandı", "Collected" } },
            { "bi.nothing", new[] { "Toplanacak kaynak yok", "Nothing to collect" } },
            { "bi.info", new[] { "Bilgi", "Info" } },
            { "bi.upgrading", new[] { "Geliştiriliyor", "Upgrading" } },
            { "bi.global", new[] { "Kaynak kapasitesi", "Resource capacity" } },
            { "bi.none", new[] { "-", "-" } },
            { "bi.full", new[] { "Depo dolu", "Storage full" } },
            { "bi.maxlevel", new[] { "Maksimum seviye", "Maximum level" } },
            { "bi.unlockhq", new[] { "Karargâh seviyesi", "Headquarters level" } },
            { "bi.levels", new[] { "Sonraki seviyeler", "Upcoming levels" } },
            { "bi.stored", new[] { "Depoda", "Stored" } },
            { "bi.soon", new[] { "Yakında", "Coming soon" } },

            // ---- upgrade rules
            { "up.started", new[] { "Geliştirme başladı", "Upgrade started" } },
            { "up.err.unknown", new[] { "Bina bulunamadı", "Building not found" } },
            { "up.err.planned", new[] { "Bu bina yakında açılacak", "This building unlocks soon" } },
            { "up.err.notbuilt", new[] { "Bina henüz inşa edilmedi", "Building not built yet" } },
            { "up.err.max", new[] { "Maksimum seviyeye ulaşıldı", "Maximum level reached" } },
            { "up.err.busy", new[] { "Bu bina zaten geliştiriliyor", "This building is already being upgraded" } },
            { "up.err.queue", new[] { "İnşaat kuyruğu dolu", "Construction queue is full" } },
            { "up.err.req", new[] { "Gereksinimler karşılanmıyor", "Requirements not met" } },
            { "up.err.res", new[] { "Yetersiz kaynak", "Not enough resources" } },

            // ---- construction screen
            { "cq.empty", new[] { "Aktif inşaat yok", "No active construction" } },
            { "cq.speedup", new[] { "Hızlandır", "Speed Up" } },
            { "cq.cancel", new[] { "İptal", "Cancel" } },
            { "cq.remaining", new[] { "Kalan", "Remaining" } },
            { "cq.target", new[] { "Hedef", "Target" } },
            { "cq.slots", new[] { "Kuyruk", "Queue" } },
            { "cq.done.many", new[] { "{0} inşaat tamamlandı!", "{0} constructions completed!" } },
            { "up.err.storage", new[] { "Depo kapasitesi yetersiz: Depoyu geliştir", "Storage too small: upgrade the Warehouse" } },
            { "cq.done", new[] { "{0} Seviye {1} tamamlandı!", "{0} reached level {1}!" } },
            { "cq.cancel.confirm", new[] { "İnşaat iptal edilsin mi? Maliyetin %{0} kadarı iade edilir.", "Cancel construction? {0}% of the cost is refunded." } },
            { "cq.speedup.confirm", new[] { "{0} elmas karşılığında hemen tamamlansın mı?", "Finish now for {0} diamonds?" } },
            { "cq.nodiamonds", new[] { "Yeterli elmas yok", "Not enough diamonds" } },
            { "cq.cancelled", new[] { "İnşaat iptal edildi", "Construction cancelled" } }
        };
    }
}
