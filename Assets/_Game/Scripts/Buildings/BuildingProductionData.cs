using System;
using Deadzone.Data;

namespace Deadzone.Buildings
{
    /// <summary>What a building produces. The amount per hour lives in BuildingLevelData.</summary>
    [Serializable]
    public sealed class BuildingProductionData
    {
        public bool producesResource;
        public ResourceType resource;
        /// <summary>True: storageCapacity raises the global wallet capacity (Warehouse). False: it is the building's own output buffer.</summary>
        public bool storageIsGlobal;
    }
}
