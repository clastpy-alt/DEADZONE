using System.Collections.Generic;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>Static data of one building type (ScriptableObject). Nothing in here changes at runtime.</summary>
    [CreateAssetMenu(menuName = "DEADZONE/Building Definition", fileName = "Building_")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        public const int AbsoluteMaxLevel = 35;

        [Header("Identity")]
        public string buildingId;
        public BuildingType type;
        public BuildingCategory category;
        public BuildingAvailability availability = BuildingAvailability.Planned;
        public string nameKey;
        public string descriptionKey;
        public string iconLabel = "BLD";
        public Color accentColor = new Color(0.4f, 0.45f, 0.5f, 1f);

        [Header("Progression")]
        [Range(1, AbsoluteMaxLevel)] public int maxLevel = AbsoluteMaxLevel;
        [Tooltip("Headquarters level needed before this building can be built or upgraded at all.")]
        public int unlockHeadquartersLevel = 1;

        [Header("Economy")]
        public BuildingProductionData production = new BuildingProductionData();

        [Header("Visuals")]
        public GameObject prefab;
        public Sprite icon;

        [Header("Levels (generated from the curve, editable)")]
        public BuildingBalanceCurve balance = new BuildingBalanceCurve();
        public List<BuildingLevelData> levels = new List<BuildingLevelData>();

        public bool IsHeadquarters => type == BuildingType.Headquarters;
        public bool IsFunctional => availability == BuildingAvailability.Functional;

        /// <summary>Data for a 1-based level, or null when out of range.</summary>
        public BuildingLevelData GetLevel(int level)
        {
            int index = level - 1;
            return index >= 0 && index < levels.Count ? levels[index] : null;
        }
    }
}
