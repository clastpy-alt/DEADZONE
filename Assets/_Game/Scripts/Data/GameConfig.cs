using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deadzone.Data
{
    /// <summary>Static game configuration (not player data). Loaded from Resources/GameConfig.</summary>
    [CreateAssetMenu(menuName = "DEADZONE/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        public const string ResourcesName = "GameConfig";

        [Serializable]
        public struct StartingResource
        {
            public ResourceType type;
            public long amount;

            public StartingResource(ResourceType type, long amount)
            {
                this.type = type;
                this.amount = amount;
            }
        }

        [Serializable]
        public struct StartingBuilding
        {
            public string buildingId;
            public int level;

            public StartingBuilding(string buildingId, int level)
            {
                this.buildingId = buildingId;
                this.level = level;
            }
        }

        /// <summary>Buildings a brand new base starts with. Also used when migrating version 1 saves.</summary>
        public static List<StartingBuilding> DefaultStartingBuildings()
        {
            return new List<StartingBuilding>
            {
                new StartingBuilding("headquarters", 1),
                new StartingBuilding("barracks", 1),
                new StartingBuilding("hospital", 1),
                new StartingBuilding("research_lab", 1),
                new StartingBuilding("food_farm", 1),
                new StartingBuilding("steel_factory", 1),
                new StartingBuilding("warehouse", 1)
            };
        }

        /// <summary>The prototype test economy. Only applied to NEW saves.</summary>
        public static List<StartingResource> DefaultStartingResources()
        {
            return new List<StartingResource>
            {
                new StartingResource(ResourceType.Food, 5000),
                new StartingResource(ResourceType.Wood, 5000),
                new StartingResource(ResourceType.Steel, 5000),
                new StartingResource(ResourceType.Fuel, 2500),
                new StartingResource(ResourceType.Gold, 1000),
                new StartingResource(ResourceType.Diamonds, 200),
                new StartingResource(ResourceType.Energy, 100)
            };
        }

        [Header("General")]
        [Min(15)] public int targetFrameRate = 60;
        [Min(5f)] public float autosaveIntervalSeconds = 30f;
        public Language defaultLanguage = Language.Turkish;
        public string defaultPlayerName = "Commander";

        [Header("Economy")]
        public long defaultResourceCapacity = 100000;

        public List<StartingResource> startingResources = DefaultStartingResources();

        [Header("Buildings and time")]
        public List<StartingBuilding> startingBuildings = DefaultStartingBuildings();
        [Tooltip("Production is credited for at most this many hours while the game is closed.")]
        [Min(0.1f)] public float maxOfflineHours = 8f;
        [Min(1)] public int constructionQueueSlots = 1;
        [Range(0, 100)] public int cancelRefundPercent = 80;
        [Min(1)] public int instantFinishDiamondsPerMinute = 1;
        [Min(0.1f)] public float tickIntervalSeconds = 0.5f;

        public long GetCapacity(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Energy:
                    return 120;
                case ResourceType.Gold:
                case ResourceType.Diamonds:
                case ResourceType.AllianceCredits:
                case ResourceType.VipPoints:
                    return 999999999L;
                default:
                    return defaultResourceCapacity;
            }
        }

        public static GameConfig Load()
        {
            GameConfig config = Resources.Load<GameConfig>(ResourcesName);
            if (config == null)
            {
                Debug.LogWarning("[GameConfig] Resources/GameConfig asset not found. Using in-memory defaults. " +
                                 "Run Tools > DEADZONE > Setup Phase 01.");
                config = CreateInstance<GameConfig>();
            }
            return config;
        }
    }
}
