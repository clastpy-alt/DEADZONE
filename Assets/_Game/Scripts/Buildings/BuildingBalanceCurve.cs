using System;
using System.Collections.Generic;
using Deadzone.Data;
using Deadzone.SaveSystem;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Editable balance inputs stored in each BuildingDefinition. BuildLevels() turns them into explicit per-level rows,
    /// so designers can tune the curve here or edit single levels afterwards. Pure math, no Unity state.
    /// </summary>
    [Serializable]
    public sealed class BuildingBalanceCurve
    {
        public List<ResourceAmount> baseCost = new List<ResourceAmount>();   // cost to reach level 2
        public float costGrowth = 1.4f;
        public int[] firstDurationsSeconds = { 30, 120, 300, 900 };         // 1->2, 2->3, 3->4, 4->5
        public float durationGrowth = 1.25f;                                // applied after the table
        public float durationMultiplier = 1f;
        public long powerBase = 50;
        public float powerExponent = 1.4f;
        public long productionBase;                                          // per hour at level 1
        public float productionGrowth = 1.18f;
        public long storageBase;
        public float storageGrowth = 1.2f;
        public int hqLevelOffset = 1;                                        // target level N needs HQ >= N - offset

        public List<BuildingLevelData> BuildLevels(int maxLevel, string hqId, bool isHeadquarters)
        {
            var levels = new List<BuildingLevelData>(maxLevel);
            for (int level = 1; level <= maxLevel; level++)
            {
                var data = new BuildingLevelData { level = level };

                if (level >= 2)
                {
                    foreach (ResourceAmount cost in baseCost)
                    {
                        long amount = RoundTo10((long)Math.Round(cost.amount * Math.Pow(costGrowth, level - 2)));
                        if (amount > 0) data.upgradeCost.Add(new ResourceAmount { type = cost.type, amount = amount });
                    }
                    data.upgradeDurationSeconds = Duration(level);
                    if (!isHeadquarters) data.requirements.Add(new BuildingRequirement(hqId, Math.Max(1, level - hqLevelOffset)));
                }

                data.powerContribution = (long)Math.Round(powerBase * Math.Pow(level, powerExponent));
                data.productionPerHour = productionBase <= 0 ? 0 : (long)Math.Round(productionBase * Math.Pow(productionGrowth, level - 1));
                data.storageCapacity = storageBase <= 0 ? 0 : (long)Math.Round(storageBase * Math.Pow(storageGrowth, level - 1));
                levels.Add(data);
            }
            return levels;
        }

        private int Duration(int targetLevel)
        {
            int index = targetLevel - 2;
            double seconds;
            if (firstDurationsSeconds != null && index < firstDurationsSeconds.Length)
            {
                seconds = firstDurationsSeconds[index];
            }
            else
            {
                int last = firstDurationsSeconds == null || firstDurationsSeconds.Length == 0 ? 900 : firstDurationsSeconds[firstDurationsSeconds.Length - 1];
                int extra = index - (firstDurationsSeconds == null ? 0 : firstDurationsSeconds.Length - 1);
                seconds = last * Math.Pow(durationGrowth, extra);
            }
            return (int)Math.Max(1, Math.Round(seconds * durationMultiplier));
        }

        private static long RoundTo10(long value) => (value + 5) / 10 * 10;
    }
}
