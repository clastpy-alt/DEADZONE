using System.Collections.Generic;
using Deadzone.SaveSystem;

namespace Deadzone.Buildings
{
    public enum PowerCategory { Research = 0, Hero = 1, Troop = 2 }

    /// <summary>Future systems (research, heroes, troops) register one of these. Until they exist they contribute zero.</summary>
    public interface IPowerSource
    {
        PowerCategory Category { get; }
        long GetPower();
    }

    public struct PowerBreakdown
    {
        public long headquarters;
        public long buildings;
        public long research;
        public long heroes;
        public long troops;
        public long Total => headquarters + buildings + research + heroes + troops;
    }

    /// <summary>Total combat power, always computed from the systems that are actually active.</summary>
    public static class PlayerPower
    {
        private static readonly List<IPowerSource> Sources = new List<IPowerSource>();

        public static void Register(IPowerSource source)
        {
            if (source != null && !Sources.Contains(source)) Sources.Add(source);
        }

        public static void Unregister(IPowerSource source) => Sources.Remove(source);
        public static void ClearSources() => Sources.Clear();

        public static PowerBreakdown Calculate(PlayerData data, BuildingDatabase database)
        {
            var result = new PowerBreakdown();

            foreach (BuildingInstanceData instance in data.buildings)
            {
                BuildingDefinition definition = database.Get(instance.buildingId);
                BuildingLevelData level = definition?.GetLevel(instance.level);
                if (level == null) continue;

                if (definition.IsHeadquarters) result.headquarters += level.powerContribution;
                else result.buildings += level.powerContribution;
            }

            foreach (IPowerSource source in Sources)
            {
                long power = System.Math.Max(0, source.GetPower());
                switch (source.Category)
                {
                    case PowerCategory.Research: result.research += power; break;
                    case PowerCategory.Hero: result.heroes += power; break;
                    case PowerCategory.Troop: result.troops += power; break;
                }
            }
            return result;
        }
    }
}
