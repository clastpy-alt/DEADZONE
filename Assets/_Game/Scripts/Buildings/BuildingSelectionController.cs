using Deadzone.Core;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Keeps track of which building is selected. The camera detects the tap and raises GameEvents.BuildingSelected;
    /// the state is kept in a static (set by GameEvents) so UI can close the panel and clear the selection ring.
    /// </summary>
    public sealed class BuildingSelectionController : MonoBehaviour
    {
        public static BuildingSelectable Current { get; private set; }

        /// <summary>Called by GameEvents.RaiseBuildingSelected, so no scene component is needed.</summary>
        internal static void Track(BuildingSelectable building) => Current = building;

        private void OnDisable() => Current = null;

        /// <summary>Clears the selection ring and tells the UI (panel hides).</summary>
        public static void Deselect()
        {
            if (Current != null) Current.SetSelected(false);
            Current = null;
            GameEvents.RaiseBuildingSelected(null);
        }
    }
}
