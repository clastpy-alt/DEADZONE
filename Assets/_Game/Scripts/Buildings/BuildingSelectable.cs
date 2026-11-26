using Deadzone.Core;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Selectable building in the Base scene. Identity (id, name key) is on the prefab; level, maximum level and
    /// everything else come from the saved data and the BuildingDatabase at runtime.
    /// </summary>
    public sealed class BuildingSelectable : MonoBehaviour
    {
        [SerializeField] private string buildingId;
        [SerializeField] private string nameKey;
        [SerializeField] private int maxLevel = 35;
        [SerializeField] private GameObject selectionRing;

        public string BuildingId => buildingId;
        public BuildingDefinition Definition => Game.IsInitialized ? Game.Buildings.GetDefinition(buildingId) : null;
        public string DisplayName => Loc.T(Definition != null ? Definition.nameKey : nameKey);
        public int Level => Game.IsInitialized ? Game.Buildings.GetLevel(buildingId) : 1;
        public int MaxLevel => Definition != null ? Definition.maxLevel : maxLevel;

        public void Configure(string id, string key, int max, GameObject ring)
        {
            buildingId = id;
            nameKey = key;
            maxLevel = max;
            selectionRing = ring;
            if (selectionRing != null) selectionRing.SetActive(false);
        }

        private void Start() => BuildingWorldLabel.Attach(this);

        public void SetSelected(bool isSelected)
        {
            if (selectionRing != null) selectionRing.SetActive(isSelected);
        }
    }
}
