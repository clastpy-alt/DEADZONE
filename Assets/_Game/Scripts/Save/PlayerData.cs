using System;
using System.Collections.Generic;
using System.Globalization;
using Deadzone.Buildings;
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

        // ---- Save version 2 (all timestamps are UTC unix seconds)
        public List<BuildingInstanceData> buildings = new List<BuildingInstanceData>();
        public List<ConstructionJobData> constructionQueue = new List<ConstructionJobData>();
        public long lastLoginUtc;
        /// <summary>Highest clock value ever observed; protects against setting the device clock backwards.</summary>
        public long lastSeenUtc;

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

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            AddStartingBuildings(data, config.startingBuildings, now);
            data.lastLoginUtc = now;
            data.lastSeenUtc = now;
            return data;
        }

        /// <summary>Adds missing starting buildings at their start level. Existing instances are never touched.</summary>
        public static void AddStartingBuildings(PlayerData data, IEnumerable<GameConfig.StartingBuilding> starting, long nowUtc)
        {
            foreach (GameConfig.StartingBuilding start in starting)
            {
                if (string.IsNullOrEmpty(start.buildingId)) continue;
                if (data.buildings.Exists(b => b.buildingId == start.buildingId)) continue;
                data.buildings.Add(new BuildingInstanceData
                {
                    instanceId = start.buildingId,
                    buildingId = start.buildingId,
                    level = Math.Max(1, start.level),
                    productionAnchorUtc = nowUtc,
                    lastCollectedUtc = nowUtc
                });
            }
        }
    }
}
