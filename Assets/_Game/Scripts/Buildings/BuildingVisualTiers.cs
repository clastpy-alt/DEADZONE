using Deadzone.Core;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Visual progression of a building (PHASE 02.5): seven additive tiers, one every five levels (L1-5, L6-10 ... L31-35).
    /// The model keeps one child group per tier; this component switches them on as the building levels up.
    /// It only changes what is visible: building id, level, saves and colliders are not touched.
    /// </summary>
    public sealed class BuildingVisualTiers : MonoBehaviour
    {
        public const int TierCount = 7;
        public const int LevelsPerTier = 5;

        [SerializeField] private GameObject[] tierGroups;

        private BuildingSelectable owner;
        private int shownTier;

        /// <summary>Tier (1..7) that belongs to a building level. Levels below 1 count as level 1, above 35 as the last tier.</summary>
        public static int TierForLevel(int level)
        {
            if (level < 1) level = 1;
            return Mathf.Clamp((level - 1) / LevelsPerTier + 1, 1, TierCount);
        }

        public int CurrentTier => shownTier;

        public void Configure(GameObject[] groups) => tierGroups = groups;

        /// <summary>Shows tiers 1..tier (additive) and hides the rest.</summary>
        public void ShowTier(int tier)
        {
            shownTier = Mathf.Clamp(tier, 1, TierCount);
            if (tierGroups == null) return;

            for (int i = 0; i < tierGroups.Length; i++)
            {
                if (tierGroups[i] != null) tierGroups[i].SetActive(i + 1 <= shownTier);
            }
        }

        private void OnEnable()
        {
            owner = GetComponent<BuildingSelectable>();
            GameEvents.BuildingChanged += OnBuildingChanged;
            GameEvents.ConstructionCompleted += OnConstructionCompleted;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.BuildingChanged -= OnBuildingChanged;
            GameEvents.ConstructionCompleted -= OnConstructionCompleted;
        }

        private void Start() => Refresh();

        private void OnBuildingChanged(string buildingId)
        {
            if (owner != null && owner.BuildingId == buildingId) Refresh();
        }

        private void OnConstructionCompleted(string buildingId, int newLevel)
        {
            if (owner != null && owner.BuildingId == buildingId) Refresh();
        }

        private void Refresh()
        {
            int level = owner != null ? owner.Level : 1;
            int tier = TierForLevel(level);
            if (tier != shownTier) ShowTier(tier);
        }
    }
}
