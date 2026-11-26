using System.Collections.Generic;
using Deadzone.Core;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>All building definitions. Loaded from Resources/BuildingDatabase.</summary>
    [CreateAssetMenu(menuName = "DEADZONE/Building Database", fileName = "BuildingDatabase")]
    public sealed class BuildingDatabase : ScriptableObject
    {
        public const string ResourcesName = "BuildingDatabase";

        public List<BuildingDefinition> definitions = new List<BuildingDefinition>();

        private Dictionary<string, BuildingDefinition> index;

        public BuildingDefinition Get(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId)) return null;
            if (index == null || index.Count != definitions.Count) Rebuild();
            index.TryGetValue(buildingId, out BuildingDefinition definition);
            return definition;
        }

        public void Rebuild()
        {
            index = new Dictionary<string, BuildingDefinition>();
            foreach (BuildingDefinition definition in definitions)
            {
                if (definition != null && !string.IsNullOrEmpty(definition.buildingId)) index[definition.buildingId] = definition;
            }
        }

        public BuildingDefinition GetHeadquarters()
        {
            foreach (BuildingDefinition definition in definitions)
            {
                if (definition != null && definition.IsHeadquarters) return definition;
            }
            return null;
        }

        public static BuildingDatabase Load()
        {
            BuildingDatabase database = Resources.Load<BuildingDatabase>(ResourcesName);
            if (database == null)
            {
                Debug.LogWarning("[BuildingDatabase] Resources/BuildingDatabase not found. Run Tools > DEADZONE > Setup Phase 02.");
                database = CreateInstance<BuildingDatabase>();
            }
            return database;
        }

        /// <summary>One message per data problem; empty means valid. Used by tests and Tools > DEADZONE > Validate Buildings.</summary>
        public List<string> Validate()
        {
            var problems = new List<string>();
            var ids = new HashSet<string>();
            var types = new HashSet<BuildingType>();

            foreach (BuildingDefinition definition in definitions)
            {
                if (definition == null) { problems.Add("Database contains an empty entry"); continue; }

                string label = string.IsNullOrEmpty(definition.buildingId) ? definition.name : definition.buildingId;
                if (string.IsNullOrEmpty(definition.buildingId)) problems.Add(label + ": buildingId is empty");
                else if (!ids.Add(definition.buildingId)) problems.Add(label + ": duplicate buildingId");
                if (!types.Add(definition.type)) problems.Add(label + ": duplicate building type " + definition.type);

                if (definition.maxLevel < 1 || definition.maxLevel > BuildingDefinition.AbsoluteMaxLevel)
                    problems.Add(label + ": maxLevel must be 1.." + BuildingDefinition.AbsoluteMaxLevel);
                if (definition.levels.Count != definition.maxLevel)
                    problems.Add(label + ": has " + definition.levels.Count + " level rows, expected " + definition.maxLevel);
                if (string.IsNullOrEmpty(definition.nameKey) || string.IsNullOrEmpty(definition.descriptionKey))
                    problems.Add(label + ": localization key missing");
                if (definition.production.producesResource && !definition.production.storageIsGlobal && definition.levels.Count > 0
                    && definition.levels[0].storageCapacity <= 0)
                    problems.Add(label + ": produces a resource but has no storage capacity");

                for (int i = 0; i < definition.levels.Count; i++)
                {
                    BuildingLevelData row = definition.levels[i];
                    if (row == null) { problems.Add(label + ": level row " + (i + 1) + " is empty"); continue; }
                    if (row.level != i + 1) problems.Add(label + ": level row " + (i + 1) + " has wrong level number " + row.level);
                    if (i >= 1)
                    {
                        if (row.upgradeDurationSeconds <= 0) problems.Add(label + " L" + row.level + ": duration must be > 0");
                        if (row.upgradeCost.Count == 0) problems.Add(label + " L" + row.level + ": no upgrade cost");
                        foreach (var cost in row.upgradeCost)
                        {
                            if (cost.amount <= 0) problems.Add(label + " L" + row.level + ": cost amount must be > 0");
                        }
                    }
                    if (row.powerContribution < 0 || row.productionPerHour < 0 || row.storageCapacity < 0)
                        problems.Add(label + " L" + row.level + ": negative power/production/storage");
                    if (i >= 1 && definition.levels[i - 1] != null && row.powerContribution < definition.levels[i - 1].powerContribution)
                        problems.Add(label + " L" + row.level + ": power decreases");
                    if (i >= 1 && definition.levels[i - 1] != null && row.productionPerHour < definition.levels[i - 1].productionPerHour)
                        problems.Add(label + " L" + row.level + ": production decreases");
                }
            }

            foreach (BuildingDefinition definition in definitions)
            {
                if (definition == null) continue;
                foreach (BuildingLevelData row in definition.levels)
                {
                    if (row == null) continue;
                    foreach (BuildingRequirement requirement in row.requirements)
                    {
                        if (!ids.Contains(requirement.buildingId))
                            problems.Add(definition.buildingId + " L" + row.level + ": requirement references unknown building '" + requirement.buildingId + "'");
                    }
                }
            }

            foreach (BuildingDefinition definition in definitions)
            {
                if (definition == null) continue;
                if (!Loc.Has(definition.nameKey)) problems.Add(definition.buildingId + ": missing localization key " + definition.nameKey);
                if (!Loc.Has(definition.descriptionKey)) problems.Add(definition.buildingId + ": missing localization key " + definition.descriptionKey);
            }
            if (GetHeadquarters() == null) problems.Add("No Headquarters definition");
            return problems;
        }
    }
}
