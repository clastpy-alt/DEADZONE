using System;
using Deadzone.Buildings;
using Deadzone.Core;
using Deadzone.SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>
    /// Window opened from the HUD construction button: every running construction with its target level, remaining time,
    /// progress, speed-up (diamonds, confirmed) and cancel (refund, confirmed). Rows beyond the queue size stay hidden,
    /// so extra VIP slots only need more rows, not new code.
    /// </summary>
    public sealed class ConstructionQueuePanel : MonoBehaviour
    {
        [Serializable]
        public sealed class Row
        {
            public GameObject root;
            public Image icon;
            public TMP_Text iconLabel;
            public TMP_Text nameText;
            public TMP_Text levelText;
            public TMP_Text remainingText;
            public ProgressBarView bar;
            public Button speedupButton;
            public Button cancelButton;
        }

        [SerializeField] private UIPanel panel;
        [SerializeField] private TMP_Text slotsText;
        [SerializeField] private TMP_Text emptyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimButton;
        [SerializeField] private Row[] rows = new Row[0];

        private const float RefreshInterval = 0.2f;
        private float timer;

        public void Bind(UIPanel uiPanel, TMP_Text slots, TMP_Text empty, Button close, Button dim, Row[] queueRows)
        {
            panel = uiPanel;
            slotsText = slots;
            emptyText = empty;
            closeButton = close;
            dimButton = dim;
            rows = queueRows;
        }

        /// <summary>Opens the (possibly inactive) panel of the current scene.</summary>
        public static void Open()
        {
            var instance = FindAnyObjectByType<ConstructionQueuePanel>(FindObjectsInactive.Include);
            if (instance == null)
            {
                ToastView.Show(Loc.T("side.construction") + ": " + Loc.T("feature.locked"));
                return;
            }
            instance.Show();
        }

        public void Show()
        {
            panel.Show();
            Refresh();
        }

        public void Hide() => panel.Hide();

        private void OnEnable()
        {
            GameEvents.ConstructionChanged += Refresh;
            GameEvents.ConstructionCompleted += OnCompleted;
            GameEvents.LanguageChanged += Refresh;

            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (dimButton != null) dimButton.onClick.AddListener(Hide);
            for (int i = 0; i < rows.Length; i++)
            {
                int index = i;
                if (rows[i].speedupButton != null) rows[i].speedupButton.onClick.AddListener(() => OnSpeedup(index));
                if (rows[i].cancelButton != null) rows[i].cancelButton.onClick.AddListener(() => OnCancel(index));
            }
            timer = 0f;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.ConstructionChanged -= Refresh;
            GameEvents.ConstructionCompleted -= OnCompleted;
            GameEvents.LanguageChanged -= Refresh;

            if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
            if (dimButton != null) dimButton.onClick.RemoveListener(Hide);
            foreach (Row row in rows)
            {
                if (row.speedupButton != null) row.speedupButton.onClick.RemoveAllListeners();
                if (row.cancelButton != null) row.cancelButton.onClick.RemoveAllListeners();
            }
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer < RefreshInterval) return;
            timer = 0f;
            Refresh();
        }

        private void OnCompleted(string buildingId, int level) => Refresh();

        // ------------------------------------------------------------------ actions

        private static ConstructionJobData JobAt(int index)
        {
            if (!Game.IsInitialized) return null;
            var queue = Game.Upgrades.Queue;
            return index >= 0 && index < queue.Count ? queue[index] : null;
        }

        private void OnSpeedup(int index)
        {
            ConstructionJobData job = JobAt(index);
            if (job == null) return;

            string buildingId = job.buildingId;
            long cost = Game.Upgrades.GetInstantFinishCost(buildingId);
            string message = string.Format(Loc.T("cq.speedup.confirm"), cost);

            UIOverlay.Confirm(Loc.T("cq.speedup"), message, () =>
            {
                // The job may have finished while the dialog was open; TryInstantFinish then simply does nothing.
                if (Game.Upgrades.GetJob(buildingId) == null) return;
                if (!Game.Upgrades.TryInstantFinish(buildingId)) ToastView.Show(Loc.T("cq.nodiamonds"));
                Refresh();
            });
        }

        private void OnCancel(int index)
        {
            ConstructionJobData job = JobAt(index);
            if (job == null) return;

            string buildingId = job.buildingId;
            string message = string.Format(Loc.T("cq.cancel.confirm"), Game.Config.cancelRefundPercent);

            UIOverlay.Confirm(Loc.T("cq.cancel"), message, () =>
            {
                if (Game.Upgrades.Cancel(buildingId)) ToastView.Show(Loc.T("cq.cancelled"));
                Refresh();
            });
        }

        // ------------------------------------------------------------------ refresh

        private void Refresh()
        {
            if (!Game.IsInitialized) return;

            var queue = Game.Upgrades.Queue;
            int slots = Game.Upgrades.QueueSlots;
            if (slotsText != null) slotsText.text = $"{Loc.T("cq.slots")}: {queue.Count}/{slots}";
            if (emptyText != null) emptyText.gameObject.SetActive(queue.Count == 0);

            for (int i = 0; i < rows.Length; i++)
            {
                Row row = rows[i];
                ConstructionJobData job = i < queue.Count ? queue[i] : null;
                if (row.root != null) row.root.SetActive(job != null);
                if (job == null) continue;

                BuildingDefinition definition = Game.Buildings.GetDefinition(job.buildingId);
                if (definition != null)
                {
                    if (row.icon != null) row.icon.color = definition.accentColor;
                    if (row.iconLabel != null) row.iconLabel.text = definition.iconLabel;
                    if (row.nameText != null) row.nameText.text = Loc.T(definition.nameKey);
                }
                if (row.levelText != null) row.levelText.text = $"{Loc.T("building.level")} {job.fromLevel} > {job.targetLevel}";
                if (row.remainingText != null)
                {
                    row.remainingText.text = $"{Loc.T("cq.remaining")}: {NumberFormatter.Duration(TimeSpan.FromSeconds(Game.Upgrades.RemainingSeconds(job)))}";
                }
                if (row.bar != null) row.bar.SetProgress(Game.Upgrades.Progress01(job));
            }
        }
    }
}
