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
        public static event Action<string> BuildingChanged;
        public static event Action ConstructionChanged;
        public static event Action<string, int> ConstructionCompleted;
        public static event Action PowerChanged;

        public static void RaiseResourceChanged(ResourceType type, long amount) => ResourceChanged?.Invoke(type, amount);
        public static void RaiseBuildingSelected(BuildingSelectable building)
        {
            // Selection state lives in a static so it works without a scene component (the close button relies on it).
            BuildingSelectionController.Track(building);
            BuildingSelected?.Invoke(building);
        }
        public static void RaiseGameStateChanged(GameState oldState, GameState newState) => GameStateChanged?.Invoke(oldState, newState);
        public static void RaisePlayerProfileChanged() => PlayerProfileChanged?.Invoke();
        public static void RaiseGameSaved(bool manual) => GameSaved?.Invoke(manual);
        public static void RaiseLanguageChanged() => LanguageChanged?.Invoke();
        public static void RaiseBuildingChanged(string buildingId) => BuildingChanged?.Invoke(buildingId);
        public static void RaiseConstructionChanged() => ConstructionChanged?.Invoke();
        public static void RaiseConstructionCompleted(string buildingId, int newLevel) => ConstructionCompleted?.Invoke(buildingId, newLevel);
        public static void RaisePowerChanged() => PowerChanged?.Invoke();

        /// <summary>Used when Enter Play Mode runs without a domain reload.</summary>
        public static void ClearAll()
        {
            ResourceChanged = null;
            BuildingSelected = null;
            GameStateChanged = null;
            PlayerProfileChanged = null;
            GameSaved = null;
            LanguageChanged = null;
            BuildingChanged = null;
            ConstructionChanged = null;
            ConstructionCompleted = null;
            PowerChanged = null;
            BuildingSelectionController.Track(null);
        }
    }
}
