using Deadzone.Core;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Keeps track of which building is selected. The camera detects the tap and raises GameEvents.BuildingSelected;
    /// this component owns the state (so UI can close the panel and clear the selection ring) and clears it on leaving the scene.
    /// </summary>
    public sealed class BuildingSelectionController : MonoBehaviour
    {
        public static BuildingSelectable Current { get; private set; }

        private void OnEnable() => GameEvents.BuildingSelected += OnSelected;

        private void OnDisable()
        {
            GameEvents.BuildingSelected -= OnSelected;
            Current = null;
        }

        private static void OnSelected(BuildingSelectable building) => Current = building;

        /// <summary>Clears the selection ring and tells the UI (panel hides).</summary>
        public static void Deselect()
        {
            if (Current != null) Current.SetSelected(false);
            Current = null;
            GameEvents.RaiseBuildingSelected(null);
        }
    }
}
