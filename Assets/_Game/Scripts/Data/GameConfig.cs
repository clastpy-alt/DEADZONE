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

        [Header("General")]
        [Min(15)] public int targetFrameRate = 60;
        [Min(5f)] public float autosaveIntervalSeconds = 30f;
        public Language defaultLanguage = Language.Turkish;
        public string defaultPlayerName = "Commander";

        [Header("Economy")]
        public long defaultResourceCapacity = 100000;

        public List<StartingResource> startingResources = new List<StartingResource>
        {
            new StartingResource(ResourceType.Food, 5000),
            new StartingResource(ResourceType.Wood, 5000),
            new StartingResource(ResourceType.Steel, 3000),
            new StartingResource(ResourceType.Fuel, 2000),
            new StartingResource(ResourceType.Gold, 10000),
            new StartingResource(ResourceType.Diamonds, 100),
            new StartingResource(ResourceType.Energy, 100)
        };

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
