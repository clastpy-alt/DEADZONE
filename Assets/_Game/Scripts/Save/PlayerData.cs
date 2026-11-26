using System;
using System.Collections.Generic;
using System.Globalization;
using Deadzone.Data;

namespace Deadzone.SaveSystem
{
    [Serializable]
    public sealed class ResourceAmount
    {
        public ResourceType type;
        public long amount;
    }

    /// <summary>All persistent per-player state. Plain data only (JsonUtility friendly).</summary>
    [Serializable]
    public sealed class PlayerData
    {
        public string playerId;
        public string playerName;
        public Language language;
        public int commanderLevel = 1;
        public long commanderExperience;
        public int vipLevel;
        public long combatPower;
        public List<ResourceAmount> resources = new List<ResourceAmount>();
        public string createdUtc;
        public string lastSavedUtc;

        public static PlayerData CreateNew(GameConfig config)
        {
            var data = new PlayerData
            {
                playerId = Guid.NewGuid().ToString("N"),
                playerName = config.defaultPlayerName,
                language = config.defaultLanguage,
                createdUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            };

            foreach (GameConfig.StartingResource start in config.startingResources)
            {
                data.resources.Add(new ResourceAmount { type = start.type, amount = Math.Max(0, start.amount) });
            }
            return data;
        }
    }
}
