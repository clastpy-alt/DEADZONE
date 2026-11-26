using System.Collections.Generic;
using Deadzone.Buildings;
using Deadzone.Data;
using Deadzone.Economy;
using Deadzone.SaveSystem;
using UnityEngine;

namespace Deadzone.Tests
{
    /// <summary>Controllable clock: tests move time explicitly, nothing depends on the real device clock.</summary>
    public sealed class FakeClock : IGameClock
    {
        public long Now;
        public FakeClock(long start) { Now = start; }
        public long UtcNowSeconds => Now;
        public void Advance(long seconds) => Now += seconds;
    }

    /// <summary>
    /// A small, fully in-memory game: its own database (HQ, farm, barracks, warehouse), wallet, manager and upgrade service,
    /// wired exactly like Game.cs does. No Unity scene, no files, no real clock.
    /// </summary>
    public sealed class TestWorld
    {
        public const long StartTime = 1_700_000_000L;
        public const string Hq = "hq";
        public const string Farm = "farm";
        public const string Barracks = "barracks";
        public const string Warehouse = "warehouse";

        public GameConfig Config;
        public BuildingDatabase Database;
        public PlayerData Data;
        public FakeClock Clock;
        public ResourceWallet Wallet;
        public BuildingManager Buildings;
        public BuildingUpgradeService Upgrades;
        public int SaveRequests;

        private readonly List<Object> created = new List<Object>();

        /// <summary>New world with HQ, farm and barracks at level 1 (warehouse optional) and generous starting resources.</summary>
        public static TestWorld Create(bool withWarehouse = false, long capacity = 1_000_000)
        {
            var world = new TestWorld();
            world.Config = world.Track(ScriptableObject.CreateInstance<GameConfig>());
            world.Config.defaultResourceCapacity = capacity;
            world.Config.maxOfflineHours = 8f;
            world.Config.constructionQueueSlots = 1;
            world.Config.cancelRefundPercent = 80;
            world.Config.instantFinishDiamondsPerMinute = 1;

            world.Database = world.Track(ScriptableObject.CreateInstance<BuildingDatabase>());
            world.Database.definitions.Add(world.MakeDefinition(Hq, BuildingType.Headquarters, isProducer: false, global: false, productionBase: 0, storageBase: 0));
            world.Database.definitions.Add(world.MakeDefinition(Farm, BuildingType.FoodFarm, isProducer: true, global: false, productionBase: 3600, storageBase: 100_000));
            world.Database.definitions.Add(world.MakeDefinition(Barracks, BuildingType.Barracks, isProducer: false, global: false, productionBase: 0, storageBase: 0));
            world.Database.definitions.Add(world.MakeDefinition(Warehouse, BuildingType.Warehouse, isProducer: true, global: true, productionBase: 0, storageBase: 2000));

            var data = new PlayerData { playerId = "test", playerName = "Tester", lastLoginUtc = StartTime, lastSeenUtc = StartTime };
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                data.resources.Add(new ResourceAmount { type = type, amount = type == ResourceType.Diamonds ? 10 : 10_000 });
            }
            world.AddInstance(data, Hq);
            world.AddInstance(data, Farm);
            world.AddInstance(data, Barracks);
            if (withWarehouse) world.AddInstance(data, Warehouse);

            world.Wire(data, new FakeClock(StartTime));
            return world;
        }

        /// <summary>Simulates closing and reopening the game: the player data is serialized and parsed again, all services are new.</summary>
        public TestWorld Reload(long advanceSeconds)
        {
            string json = JsonUtility.ToJson(Data);
            var reloaded = new TestWorld
            {
                Config = Config,
                Database = Database
            };
            reloaded.Wire(JsonUtility.FromJson<PlayerData>(json), new FakeClock(Clock.Now + advanceSeconds));
            return reloaded;
        }

        public void Wire(PlayerData data, FakeClock clock)
        {
            Data = data;
            Clock = clock;
            Wallet = new ResourceWallet(Data, type => Config.GetCapacity(type) + (Buildings != null ? Buildings.GetCapacityBonus(type) : 0));
            Buildings = new BuildingManager(Data, Database, Wallet, Clock, Config);
            Upgrades = new BuildingUpgradeService(Data, Buildings, Wallet, Config, () => SaveRequests++);
        }

        public void Cleanup()
        {
            foreach (Object item in created)
            {
                if (item != null) Object.DestroyImmediate(item);
            }
            created.Clear();
        }

        public long Get(ResourceType type) => Wallet.Get(type);

        public void Set(ResourceType type, long amount)
        {
            ResourceAmount entry = Data.resources.Find(r => r.type == type);
            if (entry == null) { entry = new ResourceAmount { type = type }; Data.resources.Add(entry); }
            entry.amount = amount;
        }

        public void AddInstance(PlayerData data, string id, int level = 1)
        {
            data.buildings.Add(new BuildingInstanceData
            {
                instanceId = id,
                buildingId = id,
                level = level,
                productionAnchorUtc = StartTime,
                lastCollectedUtc = StartTime
            });
        }

        public void SetLevel(string id, int level) => Buildings.GetInstance(id).level = level;

        private T Track<T>(T item) where T : Object
        {
            created.Add(item);
            return item;
        }

        private BuildingDefinition MakeDefinition(string id, BuildingType type, bool isProducer, bool global, long productionBase, long storageBase)
        {
            BuildingDefinition definition = Track(ScriptableObject.CreateInstance<BuildingDefinition>());
            definition.buildingId = id;
            definition.type = type;
            definition.availability = BuildingAvailability.Functional;
            definition.nameKey = "test.name";
            definition.descriptionKey = "test.desc";
            definition.maxLevel = 10;
            definition.production = new BuildingProductionData { producesResource = isProducer, resource = ResourceType.Food, storageIsGlobal = global };
            definition.balance = new BuildingBalanceCurve
            {
                baseCost = new List<ResourceAmount> { new ResourceAmount { type = ResourceType.Food, amount = 100 } },
                productionBase = productionBase,
                storageBase = storageBase
            };
            definition.levels = definition.balance.BuildLevels(definition.maxLevel, Hq, type == BuildingType.Headquarters);
            return definition;
        }
    }
}
