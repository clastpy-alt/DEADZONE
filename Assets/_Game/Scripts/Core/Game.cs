using System;
using System.Collections.Generic;
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

        /// <summary>Tests and tools can redirect the save folder (never set in a shipping flow). Null = Application.persistentDataPath/saves.</summary>
        public static string SaveDirectoryOverride { get; set; }
        /// <summary>How many constructions finished while the game was closed (set during start-up, for diagnostics and the welcome-back toast).</summary>
        public static int LastOfflineCompletionCount { get; private set; }

        private static readonly System.Collections.Generic.List<KeyValuePair<string, int>> OfflineCompletions = new System.Collections.Generic.List<KeyValuePair<string, int>>();

        /// <summary>Returns the constructions that finished while the game was closed and forgets them (called once by the HUD).</summary>
        public static System.Collections.Generic.List<KeyValuePair<string, int>> ConsumeOfflineCompletions()
        {
            var copy = new System.Collections.Generic.List<KeyValuePair<string, int>>(OfflineCompletions);
            OfflineCompletions.Clear();
            return copy;
        }

        /// <summary>Test hook: forgets all runtime state so the next EnsureInitialized() loads from 'saveDirectory' with 'clock'.</summary>
        public static void ResetForTests(string saveDirectory, IGameClock clock)
        {
            ResetStatics();
            SaveDirectoryOverride = saveDirectory;
            if (clock != null) Clock = clock;
        }

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
            SaveDirectoryOverride = null;
            LastOfflineCompletionCount = 0;
            OfflineCompletions.Clear();
        }

        public static void EnsureInitialized()
        {
            if (IsInitialized) return;

            Config = GameConfig.Load();
            BuildingDb = BuildingDatabase.Load();
            Application.targetFrameRate = Config.targetFrameRate;

            Saves = new SaveService(string.IsNullOrEmpty(SaveDirectoryOverride) ? Path.Combine(Application.persistentDataPath, "saves") : SaveDirectoryOverride) { Clock = Clock };
            SaveService.LoadResult result = Saves.Load(Config);
            LastLoadOutcome = result.Outcome;
            Bind(result.Data);

            // Buildings that became part of the starting base after this save was created are added once (existing ones are never touched).
            PlayerData.AddStartingBuildings(Player, Config.startingBuildings, Buildings.Now());

            // Offline catch-up: finish constructions and credit capped production exactly once, then record the login.
            // The HUD does not exist yet, so the completions are remembered and shown by the HUD when it starts.
            OfflineCompletions.Clear();
            void Remember(string buildingId, int level) => OfflineCompletions.Add(new KeyValuePair<string, int>(buildingId, level));
            GameEvents.ConstructionCompleted += Remember;
            try { LastOfflineCompletionCount = Upgrades.CompleteFinished(); }
            finally { GameEvents.ConstructionCompleted -= Remember; }
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
            Bind(PlayerData.CreateNew(Config, Clock.UtcNowSeconds));
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
