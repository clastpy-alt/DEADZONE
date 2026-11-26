using System;
using System.Collections.Generic;
using Deadzone.SaveSystem;

namespace Deadzone.Buildings
{
    /// <summary>Everything that changes per level. Values describe the state AT this level; cost and duration are what it takes to REACH it.</summary>
    [Serializable]
    public sealed class BuildingLevelData
    {
        public int level;
        public List<ResourceAmount> upgradeCost = new List<ResourceAmount>();
        public int upgradeDurationSeconds;
        public long powerContribution;
        public long productionPerHour;
        public long storageCapacity;
        public List<BuildingRequirement> requirements = new List<BuildingRequirement>();
    }
}
