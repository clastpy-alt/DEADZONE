using System;
using System.IO;
using Deadzone.Buildings;
using Deadzone.Data;
using Deadzone.Economy;
using Deadzone.SaveSystem;
using UnityEngine;

namespace Deadzone.Core
{
    /// <summary>
    /// Thin service holder: config, player data, wallet, save and game state.
    /// No gameplay logic lives here. EnsureInitialized() is safe to call from any scene,
    /// so pressing Play directly in the Base scene works.
    /// </summary>
    public static class Game
    {
        public static bool IsInitialized { get; private set; }
        public static GameConfig Config { get; private set; }
        public static SaveService Saves { get; private set; }
        public static PlayerData Player { get; private set; }
        public static ResourceWallet Wallet { get; private set; }
        public static BuildingDatabase BuildingDb { get; private set; }
        public static BuildingManager Buildings { get; private set; }
        public static BuildingUpgradeService Upgrades { get; private set; }
        /// <summary>Time source for production and construction. Replace with a server clock for multiplayer.</summary>
        public static IGameClock Clock { get; set; } = new SystemGameClock();
        public static GameState State { get; private set; } = GameState.Booting;
        public static SaveService.LoadOutcome LastLoadOutcome { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsInitialized = false;
            Config = null;
            Saves = null;
            Player = null;
            Wallet = null;
            BuildingDb = null;
            Buildings = null;
            Upgrades = null;
            Clock = new SystemGameClock();
            State = GameState.Booting;
            PlayerPower.ClearSources();
            GameEvents.ClearAll();
        }

        public static void EnsureInitialized()
        {
            if (IsInitialized) return;

            Config = GameConfig.Load();
            BuildingDb = BuildingDatabase.Load();
            Application.targetFrameRate = Config.targetFrameRate;

            Saves = new SaveService(Path.Combine(Application.persistentDataPath, "saves"));
            SaveService.LoadResult result = Saves.Load(Config);
            LastLoadOutcome = result.Outcome;
            Bind(result.Data);

            // Offline catch-up: finish constructions and credit capped production exactly once, then record the login.
            Upgrades.CompleteFinished();
            Buildings.AccrueAll();
            Buildings.RecalculatePower();
            Player.lastLoginUtc = Buildings.Now();

            if (result.Outcome != SaveService.LoadOutcome.Loaded)
            {
                Debug.Log($"[Game] Save load outcome: {result.Outcome}");
                Save(false);
            }

            IsInitialized = true;
            GameRuntime.EnsureExists();
        }

        public static void Save(bool manual)
        {
            if (Saves == null || Player == null) return;
            try
            {
                Saves.Save(Player);
                GameEvents.RaiseGameSaved(manual);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Game] Save failed: {ex}");
            }
        }

        /// <summary>Called by GameRuntime a few times per second and when the app resumes.</summary>
        public static void Tick()
        {
            if (!IsInitialized) return;
            Upgrades.CompleteFinished();
            Buildings.AccrueAll();
        }

        public static void StartNewGame()
        {
            EnsureInitialized();
            Saves.DeleteAll();
            Bind(PlayerData.CreateNew(Config));
            Save(false);

            Buildings.RecalculatePower();
            GameEvents.RaisePlayerProfileChanged();
            GameEvents.RaiseConstructionChanged();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                GameEvents.RaiseResourceChanged(type, Wallet.Get(type));
            }
        }

        public static void SetLanguage(Language language)
        {
            EnsureInitialized();
            Player.language = language;
            Loc.SetLanguage(language);
            Save(false);
        }

        public static void SetState(GameState state)
        {
            if (State == state) return;
            GameState old = State;
            State = state;
            GameEvents.RaiseGameStateChanged(old, state);
        }

        private static void Bind(PlayerData data)
        {
            if (Wallet != null) Wallet.Changed -= GameEvents.RaiseResourceChanged;

            Player = data;
            Wallet = new ResourceWallet(Player, type => Config.GetCapacity(type) + (Buildings != null ? Buildings.GetCapacityBonus(type) : 0));
            Wallet.Changed += GameEvents.RaiseResourceChanged;
            Buildings = new BuildingManager(Player, BuildingDb, Wallet, Clock, Config);
            Upgrades = new BuildingUpgradeService(Player, Buildings, Wallet, Config, () => Save(false));
            Loc.SetLanguage(Player.language);
        }
    }
}
