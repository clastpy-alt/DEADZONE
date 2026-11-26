using System;

namespace Deadzone.Buildings
{
    /// <summary>"Building X must be at least level N".</summary>
    [Serializable]
    public sealed class BuildingRequirement
    {
        public string buildingId;
        public int level;

        public BuildingRequirement() { }

        public BuildingRequirement(string buildingId, int level)
        {
            this.buildingId = buildingId;
            this.level = level;
        }

        public bool IsMet(Func<string, int> levelOf) => levelOf(buildingId) >= level;
    }
}
