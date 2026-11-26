using System;
using System.Collections.Generic;
using Deadzone.SaveSystem;

namespace Deadzone.Buildings
{
    /// <summary>Saved state of one building. All timestamps are UTC unix seconds.</summary>
    [Serializable]
    public sealed class BuildingInstanceData
    {
        public string instanceId;
        public string buildingId;
        public int level;
        /// <summary>Production has been credited up to this moment.</summary>
        public long productionAnchorUtc;
        /// <summary>Produced but not yet collected.</summary>
        public long storedAmount;
        public long lastCollectedUtc;
    }

    /// <summary>One running construction. 'paid' remembers what was charged, so cancel refunds the real price.</summary>
    [Serializable]
    public sealed class ConstructionJobData
    {
        public string jobId;
        public string buildingId;
        public int fromLevel;
        public int targetLevel;
        public long startUtc;
        public long endUtc;
        public List<ResourceAmount> paid = new List<ResourceAmount>();
    }
}
