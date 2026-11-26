using System;
using Deadzone.Data;

namespace Deadzone.SaveSystem
{
    /// <summary>Upgrades older save payloads to the current format without losing data. Pure and unit-testable.</summary>
    public static class SaveMigration
    {
        public static void Migrate(PlayerData data, int fromVersion, long nowUtc)
        {
            if (fromVersion < 2) MigrateV1ToV2(data, nowUtc);
        }

        /// <summary>
        /// Version 1 had no buildings or timestamps. The starting base is added at level 1 with the production
        /// anchor set to 'now', so a migrated save neither loses nor gains resources.
        /// </summary>
        private static void MigrateV1ToV2(PlayerData data, long nowUtc)
        {
            if (data.buildings == null) data.buildings = new System.Collections.Generic.List<Buildings.BuildingInstanceData>();
            if (data.constructionQueue == null) data.constructionQueue = new System.Collections.Generic.List<Buildings.ConstructionJobData>();

            PlayerData.AddStartingBuildings(data, GameConfig.DefaultStartingBuildings(), nowUtc);
            if (data.lastLoginUtc <= 0) data.lastLoginUtc = nowUtc;
            if (data.lastSeenUtc <= 0) data.lastSeenUtc = nowUtc;
        }

        /// <summary>Repairs impossible values instead of rejecting the whole save.</summary>
        public static void Sanitize(PlayerData data, long nowUtc)
        {
            if (data.buildings == null) data.buildings = new System.Collections.Generic.List<Buildings.BuildingInstanceData>();
            if (data.constructionQueue == null) data.constructionQueue = new System.Collections.Generic.List<Buildings.ConstructionJobData>();

            data.buildings.RemoveAll(b => b == null || string.IsNullOrEmpty(b.buildingId));
            var seen = new System.Collections.Generic.HashSet<string>();
            data.buildings.RemoveAll(b => !seen.Add(b.buildingId));
            foreach (Buildings.BuildingInstanceData b in data.buildings)
            {
                if (string.IsNullOrEmpty(b.instanceId)) b.instanceId = b.buildingId;
                b.level = Math.Max(0, Math.Min(Buildings.BuildingDefinition.AbsoluteMaxLevel, b.level));
                if (b.storedAmount < 0) b.storedAmount = 0;
                if (b.productionAnchorUtc <= 0) b.productionAnchorUtc = nowUtc;
            }

            // At most one job per building; broken jobs are dropped (their cost is not refunded: a corrupt job has no trustworthy price).
            var jobBuildings = new System.Collections.Generic.HashSet<string>();
            data.constructionQueue.RemoveAll(j => j == null || string.IsNullOrEmpty(j.buildingId) || j.targetLevel < 1 || !jobBuildings.Add(j.buildingId));
            foreach (Buildings.ConstructionJobData job in data.constructionQueue)
            {
                if (string.IsNullOrEmpty(job.jobId)) job.jobId = Guid.NewGuid().ToString("N");
                if (job.paid == null) job.paid = new System.Collections.Generic.List<ResourceAmount>();
            }

            if (data.lastLoginUtc < 0) data.lastLoginUtc = 0;
            if (data.lastSeenUtc < 0) data.lastSeenUtc = 0;
        }
    }
}
