using System;
using System.IO;
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
            State = GameState.Booting;
            GameEvents.ClearAll();
        }

        public static void EnsureInitialized()
        {
            if (IsInitialized) return;

            Config = GameConfig.Load();
            Application.targetFrameRate = Config.targetFrameRate;

            Saves = new SaveService(Path.Combine(Application.persistentDataPath, "saves"));
            SaveService.LoadResult result = Saves.Load(Config);
            LastLoadOutcome = result.Outcome;
            Bind(result.Data);

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

        public static void StartNewGame()
        {
            EnsureInitialized();
            Saves.DeleteAll();
            Bind(PlayerData.CreateNew(Config));
            Save(false);

            GameEvents.RaisePlayerProfileChanged();
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
            Wallet = new ResourceWallet(Player, Config.GetCapacity);
            Wallet.Changed += GameEvents.RaiseResourceChanged;
            Loc.SetLanguage(Player.language);
        }
    }
}
