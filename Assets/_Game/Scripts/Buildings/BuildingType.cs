namespace Deadzone.Buildings
{
    /// <summary>Explicit values: referenced by data assets, never renumber.</summary>
    public enum BuildingType
    {
        Headquarters = 0, Barracks = 1, TankFactory = 2, AirBase = 3, MilitaryHospital = 4, TrainingGround = 5,
        FoodFarm = 6, LumberMill = 7, SteelFactory = 8, OilRefinery = 9, GoldMine = 10, PowerPlant = 11, Warehouse = 12,
        ResearchLaboratory = 13, RadarStation = 14, IntelligenceCenter = 15,
        AllianceCenter = 16, RecruitmentCenter = 17, TradingCenter = 18,
        Watchtower = 19, DefenseTurret = 20, DefensiveWall = 21
    }

    public enum BuildingCategory { Military = 0, Economy = 1, Technology = 2, Social = 3, Defense = 4 }

    /// <summary>Planned buildings exist as data but cannot be upgraded yet.</summary>
    public enum BuildingAvailability { Functional = 0, Planned = 1 }
}
