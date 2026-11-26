using Deadzone.Core;

namespace Deadzone.UI
{
    public enum HudFeature
    {
        Base, Heroes, Troops, Inventory, World, Alliance, Shop,
        ConstructionQueue, ResearchQueue, TroopTraining, Missions, Vip,
        Events, EventCalendar, DailyRewards, BattlePass, SpecialOffers
    }

    /// <summary>
    /// Single source of truth for which HUD buttons work. Unlock a feature here when its
    /// system exists (and add its case to Open). Everything else shows as locked.
    /// </summary>
    public static class FeatureRouter
    {
        public static bool IsAvailable(HudFeature feature)
        {
            return feature == HudFeature.Base || feature == HudFeature.ConstructionQueue;
        }

        public static string NameKey(HudFeature feature)
        {
            switch (feature)
            {
                case HudFeature.Base: return "nav.base";
                case HudFeature.Heroes: return "nav.heroes";
                case HudFeature.Troops: return "nav.troops";
                case HudFeature.Inventory: return "nav.inventory";
                case HudFeature.World: return "nav.world";
                case HudFeature.Alliance: return "nav.alliance";
                case HudFeature.Shop: return "nav.shop";
                case HudFeature.ConstructionQueue: return "side.construction";
                case HudFeature.ResearchQueue: return "side.research";
                case HudFeature.TroopTraining: return "side.training";
                case HudFeature.Missions: return "side.missions";
                case HudFeature.Vip: return "side.vip";
                case HudFeature.Events: return "side.events";
                case HudFeature.EventCalendar: return "side.calendar";
                case HudFeature.DailyRewards: return "side.daily";
                case HudFeature.BattlePass: return "side.battlepass";
                case HudFeature.SpecialOffers: return "side.offers";
                default: return string.Empty;
            }
        }

        public static void Open(HudFeature feature)
        {
            if (!IsAvailable(feature))
            {
                ToastView.Show(Loc.T(NameKey(feature)) + ": " + Loc.T("feature.locked"));
                return;
            }

            switch (feature)
            {
                case HudFeature.Base:
                    var camera = UnityEngine.Object.FindAnyObjectByType<IsometricCameraController>();
                    if (camera != null) camera.ResetView();
                    break;
                case HudFeature.ConstructionQueue:
                    ConstructionQueuePanel.Open();
                    break;
            }
        }
    }
}
