using Deadzone.Buildings;
using Deadzone.Core;
using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>
    /// Live status on the HUD construction button: red count bubble with the number of running constructions and the shortest
    /// remaining time. Only used where the feature really exists (construction queue); locked buttons show no fake numbers.
    /// </summary>
    public sealed class HudStatusView : MonoBehaviour
    {
        private const float RefreshInterval = 0.5f;

        [SerializeField] private NotificationBadgeView badge;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject timerRoot;

        private float nextRefresh;

        public void Bind(NotificationBadgeView badgeView, TMP_Text timer, GameObject timerContainer)
        {
            badge = badgeView;
            timerText = timer;
            timerRoot = timerContainer;
        }

        private void OnEnable()
        {
            GameEvents.ConstructionChanged += Refresh;
            GameEvents.ConstructionCompleted += OnCompleted;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.ConstructionChanged -= Refresh;
            GameEvents.ConstructionCompleted -= OnCompleted;
        }

        private void OnCompleted(string buildingId, int level) => Refresh();

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        private void Refresh()
        {
            if (!Game.IsInitialized || Game.Upgrades == null) return;

            int count = Game.Upgrades.Queue.Count;
            long shortest = long.MaxValue;
            for (int i = 0; i < count; i++)
            {
                shortest = System.Math.Min(shortest, Game.Upgrades.RemainingSeconds(Game.Upgrades.Queue[i]));
            }

            if (badge != null) badge.SetCount(count);
            if (timerText != null)
            {
                if (timerRoot != null) timerRoot.SetActive(count > 0);
                if (count > 0) timerText.text = NumberFormatter.Duration(System.TimeSpan.FromSeconds(shortest));
            }
        }
    }
}
