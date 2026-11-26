using System;
using Deadzone.Buildings;
using Deadzone.Data;

namespace Deadzone.Core
{
    /// <summary>Decoupled gameplay events. Subscribers must unsubscribe in OnDisable/OnDestroy.</summary>
    public static class GameEvents
    {
        public static event Action<ResourceType, long> ResourceChanged;
        public static event Action<BuildingSelectable> BuildingSelected;
        public static event Action<GameState, GameState> GameStateChanged;
        public static event Action PlayerProfileChanged;
        public static event Action<bool> GameSaved;
        public static event Action LanguageChanged;

        public static void RaiseResourceChanged(ResourceType type, long amount) => ResourceChanged?.Invoke(type, amount);
        public static void RaiseBuildingSelected(BuildingSelectable building) => BuildingSelected?.Invoke(building);
        public static void RaiseGameStateChanged(GameState oldState, GameState newState) => GameStateChanged?.Invoke(oldState, newState);
        public static void RaisePlayerProfileChanged() => PlayerProfileChanged?.Invoke();
        public static void RaiseGameSaved(bool manual) => GameSaved?.Invoke(manual);
        public static void RaiseLanguageChanged() => LanguageChanged?.Invoke();

        /// <summary>Used when Enter Play Mode runs without a domain reload.</summary>
        public static void ClearAll()
        {
            ResourceChanged = null;
            BuildingSelected = null;
            GameStateChanged = null;
            PlayerProfileChanged = null;
            GameSaved = null;
            LanguageChanged = null;
        }
    }
}
