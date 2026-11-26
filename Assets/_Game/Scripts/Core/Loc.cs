using System.Collections.Generic;
using Deadzone.Data;

namespace Deadzone.Core
{
    /// <summary>
    /// Localization foundation (TR / EN). Each entry is { turkish, english }.
    /// Adding a language = add an enum value and a column; FindIncompleteKeys() reports gaps.
    /// </summary>
    public static class Loc
    {
        private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            { "app.title", new[] { "DEADZONE: SON İMPARATORLUK", "DEADZONE: LAST EMPIRE" } },
            { "menu.play", new[] { "Oyna", "Play" } },
            { "menu.newgame", new[] { "Yeni Oyun", "New Game" } },
            { "menu.newgame.confirm", new[] { "Emin misin? Tekrar dokun", "Are you sure? Tap again" } },
            { "menu.language", new[] { "Dil: Türkçe", "Language: English" } },

            { "hud.level", new[] { "Seviye", "Level" } },
            { "hud.power", new[] { "Güç", "Power" } },
            { "hud.save", new[] { "Kaydet", "Save" } },
            { "hud.saved", new[] { "Oyun kaydedildi", "Game saved" } },

            { "common.locked", new[] { "Kilitli", "Locked" } },
            { "feature.locked", new[] { "Sonraki aşamada açılacak", "Unlocks in a later phase" } },

            { "nav.base", new[] { "Üs", "Base" } },
            { "nav.heroes", new[] { "Kahramanlar", "Heroes" } },
            { "nav.troops", new[] { "Birlikler", "Troops" } },
            { "nav.inventory", new[] { "Envanter", "Inventory" } },
            { "nav.world", new[] { "Dünya", "World" } },
            { "nav.alliance", new[] { "İttifak", "Alliance" } },
            { "nav.shop", new[] { "Mağaza", "Shop" } },

            { "side.construction", new[] { "İnşaat", "Construction" } },
            { "side.research", new[] { "Araştırma", "Research" } },
            { "side.training", new[] { "Eğitim", "Training" } },
            { "side.missions", new[] { "Görevler", "Missions" } },
            { "side.vip", new[] { "VIP", "VIP" } },
            { "side.events", new[] { "Etkinlik", "Events" } },
            { "side.calendar", new[] { "Takvim", "Calendar" } },
            { "side.daily", new[] { "Günlük", "Daily" } },
            { "side.offers", new[] { "Teklifler", "Offers" } },

            { "building.hq", new[] { "Karargâh", "Headquarters" } },
            { "building.barracks", new[] { "Kışla", "Barracks" } },
            { "building.hospital", new[] { "Hastane", "Hospital" } },
            { "building.lab", new[] { "Araştırma Lab.", "Research Lab" } },
            { "building.farm", new[] { "Gıda Çiftliği", "Food Farm" } },
            { "building.steel", new[] { "Çelik Fabrikası", "Steel Factory" } },
            { "side.battlepass", new[] { "Savaş Bileti", "Battle Pass" } },

            { "popup.close", new[] { "Kapat", "Close" } },
            { "confirm.yes", new[] { "Evet", "Yes" } },
            { "confirm.no", new[] { "Hayır", "No" } },
            { "reward.claim", new[] { "Al", "Claim" } },
            { "loading.text", new[] { "Yükleniyor...", "Loading..." } },
            { "shop.buy", new[] { "Satın Al", "Buy" } },
            { "profile.title", new[] { "Komutan Profili", "Commander Profile" } },
            { "profile.id", new[] { "Oyuncu No", "Player ID" } },
            { "newgame.confirm.message", new[] { "Mevcut kayıt silinecek ve yeni oyun başlayacak.", "Your current save will be deleted and a new game will start." } },

            { "event.status.upcoming", new[] { "Yakında", "Upcoming" } },
            { "event.status.active", new[] { "Aktif", "Active" } },
            { "event.status.completed", new[] { "Tamamlandı", "Completed" } },
            { "event.status.rewardavailable", new[] { "Ödül Hazır", "Reward Available" } },
            { "event.status.expired", new[] { "Sona Erdi", "Expired" } },

            { "res.food", new[] { "Gıda", "Food" } },
            { "res.wood", new[] { "Odun", "Wood" } },
            { "res.steel", new[] { "Çelik", "Steel" } },
            { "res.fuel", new[] { "Yakıt", "Fuel" } },
            { "res.gold", new[] { "Altın", "Gold" } },
            { "res.diamonds", new[] { "Elmas", "Diamonds" } },
            { "res.energy", new[] { "Enerji", "Energy" } },
            { "res.alliancecredits", new[] { "İttifak Kredisi", "Alliance Credits" } },
            { "res.vippoints", new[] { "VIP Puanı", "VIP Points" } },

            { "building.level", new[] { "Seviye", "Level" } },
            { "building.upgrade", new[] { "Geliştir", "Upgrade" } }
        };

        public static Language Current { get; private set; } = Language.Turkish;

        public static void SetLanguage(Language language)
        {
            if (Current == language) return;
            Current = language;
            GameEvents.RaiseLanguageChanged();
        }

        public static string T(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            if (Table.TryGetValue(key, out string[] values))
            {
                int index = (int)Current;
                if (index < values.Length && !string.IsNullOrEmpty(values[index])) return values[index];
                if (values.Length > 0 && !string.IsNullOrEmpty(values[0])) return values[0];
            }
            return key;
        }

        public static IEnumerable<string> FindIncompleteKeys()
        {
            int languageCount = System.Enum.GetValues(typeof(Language)).Length;
            foreach (KeyValuePair<string, string[]> pair in Table)
            {
                bool incomplete = pair.Value.Length < languageCount;
                for (int i = 0; !incomplete && i < languageCount; i++)
                {
                    incomplete = string.IsNullOrWhiteSpace(pair.Value[i]);
                }
                if (incomplete) yield return pair.Key;
            }
        }
    }
}
