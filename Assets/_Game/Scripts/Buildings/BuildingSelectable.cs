using Deadzone.Core;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Phase 01: selectable building marker with placeholder level data.
    /// Phase 02 replaces level/maxLevel with data-driven definitions and saved state.
    /// </summary>
    public sealed class BuildingSelectable : MonoBehaviour
    {
        [SerializeField] private string buildingId;
        [SerializeField] private string nameKey;
        [SerializeField] private int level = 1;
        [SerializeField] private int maxLevel = 35;
        [SerializeField] private GameObject selectionRing;

        public string BuildingId => buildingId;
        public string DisplayName => Loc.T(nameKey);
        public int Level => level;
        public int MaxLevel => maxLevel;

        public void Configure(string id, string key, int max, GameObject ring)
        {
            buildingId = id;
            nameKey = key;
            maxLevel = max;
            selectionRing = ring;
            if (selectionRing != null) selectionRing.SetActive(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionRing != null) selectionRing.SetActive(isSelected);
        }
    }
}
