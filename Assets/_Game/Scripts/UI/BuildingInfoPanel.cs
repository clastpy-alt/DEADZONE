using System;
using System.Text;
using Deadzone.Buildings;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>
    /// Bottom sheet for the selected building: image, level, benefits (current / next), cost, duration,
    /// requirements (green = met, red = missing), collect / upgrade / info / close. Everything shown comes from
    /// BuildingDatabase + the services; the panel only formats it and forwards clicks.
    /// </summary>
    public sealed class BuildingInfoPanel : MonoBehaviour
    {
        /// <summary>Bind payload used by the editor generator.</summary>
        public sealed class Widgets
        {
            public GameObject content;
            public Image icon;
            public RawImage preview;
            public TMP_Text iconLabel, nameText, levelText, descriptionText, currentText, nextText, costText, requirementsText, reasonText;
            public GameObject progressGroup;
            public ProgressBarView progressBar;
            public TMP_Text remainingText;
            public Button collectButton, upgradeButton, infoButton, closeButton;
            public TMP_Text collectLabel, upgradeLabel;
        }

        /// <summary>Bump when the generated layout changes: the scene patcher then rebuilds older panels.</summary>
        public const int CurrentLayout = 3;

        [SerializeField] private int layoutVersion;
        [SerializeField] private GameObject content;
        [SerializeField] private Image iconImage;
        [SerializeField] private BuildingPreview preview;
        [SerializeField] private TMP_Text iconLabel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text currentText;
        [SerializeField] private TMP_Text nextText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text requirementsText;
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] private GameObject progressGroup;
        [SerializeField] private ProgressBarView progressBar;
        [SerializeField] private TMP_Text remainingText;
        [SerializeField] private Button collectButton;
        [SerializeField] private TMP_Text collectLabel;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text upgradeLabel;
        [SerializeField] private Button infoButton;
        [SerializeField] private Button closeButton;

        private const float RefreshInterval = 0.25f;
        private static readonly string GoodHex = ColorUtility.ToHtmlStringRGB(UIColors.Success);
        private static readonly string BadHex = ColorUtility.ToHtmlStringRGB(UIColors.Danger);
        private static readonly string DimHex = ColorUtility.ToHtmlStringRGB(UIColors.TextSecondary);
        private static readonly string GoldHex = ColorUtility.ToHtmlStringRGB(UIColors.Gold);

        private BuildingSelectable current;
        private float timer;

        /// <summary>True when the panel was generated with the full PHASE 02 layout (the editor patcher rebuilds older ones).</summary>
        public bool HasDetailLayout => layoutVersion >= CurrentLayout && descriptionText != null && costText != null && collectButton != null && infoButton != null && closeButton != null;

        public void Bind(Widgets w)
        {
            layoutVersion = CurrentLayout;
            content = w.content;
            iconImage = w.icon;
            preview = w.preview != null ? w.preview.GetComponent<BuildingPreview>() : null;
            iconLabel = w.iconLabel;
            nameText = w.nameText;
            levelText = w.levelText;
            descriptionText = w.descriptionText;
            currentText = w.currentText;
            nextText = w.nextText;
            costText = w.costText;
            requirementsText = w.requirementsText;
            reasonText = w.reasonText;
            progressGroup = w.progressGroup;
            progressBar = w.progressBar;
            remainingText = w.remainingText;
            collectButton = w.collectButton;
            collectLabel = w.collectLabel;
            upgradeButton = w.upgradeButton;
            upgradeLabel = w.upgradeLabel;
            infoButton = w.infoButton;
            closeButton = w.closeButton;
        }

        private void OnEnable()
        {
            GameEvents.BuildingSelected += OnBuildingSelected;
            GameEvents.LanguageChanged += Refresh;
            GameEvents.ResourceChanged += OnResourceChanged;
            GameEvents.ConstructionChanged += Refresh;
            GameEvents.ConstructionCompleted += OnConstructionCompleted;
            GameEvents.BuildingChanged += OnBuildingChanged;

            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (collectButton != null) collectButton.onClick.AddListener(OnCollectClicked);
            if (infoButton != null) infoButton.onClick.AddListener(OnInfoClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.BuildingSelected -= OnBuildingSelected;
            GameEvents.LanguageChanged -= Refresh;
            GameEvents.ResourceChanged -= OnResourceChanged;
            GameEvents.ConstructionChanged -= Refresh;
            GameEvents.ConstructionCompleted -= OnConstructionCompleted;
            GameEvents.BuildingChanged -= OnBuildingChanged;

            if (upgradeButton != null) upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            if (collectButton != null) collectButton.onClick.RemoveListener(OnCollectClicked);
            if (infoButton != null) infoButton.onClick.RemoveListener(OnInfoClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        private void Update()
        {
            if (current == null) return;

            timer += Time.unscaledDeltaTime;
            if (timer < RefreshInterval) return;
            timer = 0f;
            Refresh();
        }

        // ------------------------------------------------------------------ events

        private void OnBuildingSelected(BuildingSelectable building)
        {
            current = building;
            timer = 0f;
            if (preview != null) preview.Show(building);
            Refresh();
        }

        private void OnResourceChanged(ResourceType type, long amount) => Refresh();
        private void OnConstructionCompleted(string buildingId, int level)
        {
            // A finished upgrade can change the visual tier: rebuild the preview clone.
            if (preview != null && current != null && current.BuildingId == buildingId) preview.Show(current);
            Refresh();
        }

        private void OnBuildingChanged(string buildingId)
        {
            if (current != null && current.BuildingId == buildingId) Refresh();
        }

        private void OnCloseClicked() => BuildingSelectionController.Deselect();

        private void OnUpgradeClicked()
        {
            if (current == null || !Game.IsInitialized) return;

            if (Game.Upgrades.GetJob(current.BuildingId) != null)
            {
                ShowSpeedUp(current.BuildingId);
                return;
            }

            UpgradeBlockReason reason = Game.Upgrades.TryStartUpgrade(current.BuildingId);
            ToastView.Show(Loc.T(reason == UpgradeBlockReason.None ? "up.started" : ReasonKey(reason)));
            Refresh();
        }

        /// <summary>Speed-up screen: remaining time, diamond cost and balance, confirmed before anything is spent.</summary>
        private static void ShowSpeedUp(string buildingId)
        {
            ConstructionJobData job = Game.Upgrades.GetJob(buildingId);
            if (job == null) return;

            long cost = Game.Upgrades.GetInstantFinishCost(buildingId);
            string remaining = NumberFormatter.Duration(TimeSpan.FromSeconds(Game.Upgrades.RemainingSeconds(job)));
            string message = string.Format(Loc.T("bi.speedup.detail"), remaining, cost, Game.Wallet.Get(ResourceType.Diamonds));

            UIOverlay.Confirm(Loc.T("bi.speedup"), message, () =>
            {
                if (Game.Upgrades.GetJob(buildingId) == null) return;
                if (!Game.Upgrades.TryInstantFinish(buildingId)) ToastView.Show(Loc.T("cq.nodiamonds"));
            });
        }

        private void OnCollectClicked()
        {
            if (current == null || !Game.IsInitialized) return;

            BuildingDefinition definition = Game.Buildings.GetDefinition(current.BuildingId);
            long collected = Game.Buildings.Collect(current.BuildingId);
            if (collected > 0 && definition != null)
            {
                ToastView.Show($"{Loc.T("bi.collected")}: +{NumberFormatter.Compact(collected)} {ResourceName(definition.production.resource)}");
            }
            else
            {
                ToastView.Show(Loc.T(Game.Buildings.GetStored(current.BuildingId) > 0 ? "bi.full" : "bi.nothing"));
            }
            Refresh();
        }

        private void OnInfoClicked()
        {
            if (current == null || !Game.IsInitialized) return;
            BuildingDefinition definition = Game.Buildings.GetDefinition(current.BuildingId);
            if (definition == null) return;
            UIOverlay.ShowPopup(Loc.T(definition.nameKey), BuildInfoBody(definition));
        }

        // ------------------------------------------------------------------ refresh

        private void Refresh()
        {
            bool visible = current != null && Game.IsInitialized;
            if (content != null) content.SetActive(visible);
            if (!visible) return;

            string id = current.BuildingId;
            BuildingDefinition definition = Game.Buildings.GetDefinition(id);
            if (definition == null) return;

            int level = Game.Buildings.GetLevel(id);
            UpgradeCheck check = Game.Upgrades.CheckUpgrade(id);
            ConstructionJobData job = Game.Upgrades.GetJob(id);
            BuildingLevelData now = definition.GetLevel(level);
            BuildingLevelData target = check.target;

            Set(nameText, Loc.T(definition.nameKey));
            Set(levelText, $"{Loc.T("building.level")} {level}/{definition.maxLevel}");
            Set(descriptionText, Loc.T(definition.descriptionKey));
            if (iconImage != null) iconImage.color = definition.accentColor;
            Set(iconLabel, definition.iconLabel);

            Set(currentText, $"<color=#{GoldHex}><b>{Loc.T("bi.current")}</b></color>\n{DescribeLevel(definition, now, null)}");
            if (target != null)
            {
                Set(nextText, $"<color=#{GoldHex}><b>{Loc.T("bi.next")}</b></color>\n{DescribeLevel(definition, target, now)}");
            }
            else
            {
                string key = check.reason == UpgradeBlockReason.MaxLevel ? "bi.maxlevel" : "bi.none";
                Set(nextText, $"<color=#{GoldHex}><b>{Loc.T("bi.next")}</b></color>\n<color=#{DimHex}>{Loc.T(key)}</color>");
            }

            Set(costText, target != null ? BuildCostText(target) : $"<color=#{DimHex}>{Loc.T("bi.none")}</color>");
            Set(requirementsText, BuildRequirementsText(target));

            // Upgrade button + reason
            bool upgrading = job != null;
            if (upgradeButton != null) upgradeButton.interactable = check.CanUpgrade || upgrading;
            Set(upgradeLabel, Loc.T(upgrading ? "bi.speedup" : "building.upgrade"));
            if (reasonText != null)
            {
                bool showReason = !check.CanUpgrade && !upgrading;
                reasonText.gameObject.SetActive(showReason);
                if (showReason) reasonText.text = $"<color=#{BadHex}>{Loc.T(ReasonKey(check.reason))}</color>";
            }

            // Running construction
            if (progressGroup != null) progressGroup.SetActive(upgrading);
            if (upgrading)
            {
                if (progressBar != null) progressBar.SetProgress(Game.Upgrades.Progress01(job));
                Set(remainingText, $"{Loc.T("cq.remaining")}: {NumberFormatter.Duration(TimeSpan.FromSeconds(Game.Upgrades.RemainingSeconds(job)))}");
            }

            RefreshCollect(definition, level);
        }

        private void RefreshCollect(BuildingDefinition definition, int level)
        {
            if (collectButton == null) return;

            bool producer = level >= 1 && definition.production.producesResource && !definition.production.storageIsGlobal;
            collectButton.gameObject.SetActive(producer);
            if (!producer) return;

            long stored = Game.Buildings.GetStored(definition.buildingId);
            collectButton.interactable = stored > 0;
            Set(collectLabel, stored > 0
                ? $"{Loc.T("bi.collect")}\n{NumberFormatter.Compact(stored)} {ResourceName(definition.production.resource)}"
                : Loc.T("bi.collect"));
        }

        // ------------------------------------------------------------------ text builders

        internal static string ReasonKey(UpgradeBlockReason reason)
        {
            switch (reason)
            {
                case UpgradeBlockReason.UnknownBuilding: return "up.err.unknown";
                case UpgradeBlockReason.NotAvailable: return "up.err.planned";
                case UpgradeBlockReason.NotBuilt: return "up.err.notbuilt";
                case UpgradeBlockReason.MaxLevel: return "up.err.max";
                case UpgradeBlockReason.AlreadyUpgrading: return "up.err.busy";
                case UpgradeBlockReason.QueueFull: return "up.err.queue";
                case UpgradeBlockReason.RequirementNotMet: return "up.err.req";
                case UpgradeBlockReason.InsufficientResources: return "up.err.res";
                case UpgradeBlockReason.StorageTooSmall: return "up.err.storage";
                default: return "up.err.unknown";
            }
        }

        internal static string ResourceName(ResourceType type) => Loc.T("res." + type.ToString().ToLowerInvariant());

        /// <summary>Power / production / storage of one level; green "(+n)" shows the gain over 'previous'.</summary>
        internal static string DescribeLevel(BuildingDefinition definition, BuildingLevelData data, BuildingLevelData previous)
        {
            if (data == null) return $"<color=#{DimHex}>{Loc.T("bi.none")}</color>";

            var sb = new StringBuilder();
            sb.Append(Loc.T("bi.power")).Append(": ").Append(NumberFormatter.Compact(data.powerContribution)).Append(Gain(data.powerContribution, previous?.powerContribution));

            if (definition.production.producesResource)
            {
                string resource = ResourceName(definition.production.resource);
                if (definition.production.storageIsGlobal)
                {
                    sb.Append('\n').Append(Loc.T("bi.global")).Append(": +").Append(NumberFormatter.Compact(data.storageCapacity)).Append(Gain(data.storageCapacity, previous?.storageCapacity));
                }
                else
                {
                    sb.Append('\n').Append(Loc.T("bi.production")).Append(": ").Append(NumberFormatter.Compact(data.productionPerHour)).Append(Loc.T("bi.perhour"))
                      .Append(' ').Append(resource).Append(Gain(data.productionPerHour, previous?.productionPerHour));
                    sb.Append('\n').Append(Loc.T("bi.storage")).Append(": ").Append(NumberFormatter.Compact(data.storageCapacity)).Append(Gain(data.storageCapacity, previous?.storageCapacity));
                }
            }
            return sb.ToString();
        }

        private static string Gain(long value, long? previous)
        {
            if (previous == null || value <= previous.Value) return string.Empty;
            return $" <color=#{GoodHex}>(+{NumberFormatter.Compact(value - previous.Value)})</color>";
        }

        /// <summary>Cost line per resource (red when the wallet is short) and the duration.</summary>
        internal static string BuildCostText(BuildingLevelData target)
        {
            var sb = new StringBuilder();
            sb.Append($"<color=#{GoldHex}><b>{Loc.T("bi.cost")}</b></color>\n");

            for (int i = 0; i < target.upgradeCost.Count; i++)
            {
                ResourceAmount cost = target.upgradeCost[i];
                long have = Game.Wallet.Get(cost.type);
                bool enough = have >= cost.amount;
                string hex = enough ? GoodHex : BadHex;
                string amount = enough
                    ? NumberFormatter.Compact(cost.amount)
                    : $"{NumberFormatter.Compact(have)}/{NumberFormatter.Compact(cost.amount)}";
                if (i > 0) sb.Append("   ");
                sb.Append($"<color=#{hex}>{ResourceName(cost.type)} {amount}</color>");
            }

            sb.Append($"\n{Loc.T("bi.duration")}: {NumberFormatter.Duration(TimeSpan.FromSeconds(target.upgradeDurationSeconds))}");
            return sb.ToString();
        }

        internal static string BuildRequirementsText(BuildingLevelData target)
        {
            var sb = new StringBuilder();
            sb.Append($"<color=#{GoldHex}><b>{Loc.T("bi.requirements")}</b></color>");

            if (target == null || target.requirements.Count == 0)
            {
                sb.Append($"\n<color=#{DimHex}>{Loc.T("bi.none")}</color>");
                return sb.ToString();
            }

            foreach (BuildingRequirement requirement in target.requirements)
            {
                int have = Game.Buildings.GetLevel(requirement.buildingId);
                bool met = have >= requirement.level;
                BuildingDefinition required = Game.Buildings.GetDefinition(requirement.buildingId);
                string name = required != null ? Loc.T(required.nameKey) : requirement.buildingId;
                sb.Append($"\n<color=#{(met ? GoodHex : BadHex)}>{name} {Loc.T("building.level")} {requirement.level} ({have}/{requirement.level})</color>");
            }
            return sb.ToString();
        }

        /// <summary>Plain-text details for the Info popup: description, limits and the next few levels.</summary>
        internal static string BuildInfoBody(BuildingDefinition definition)
        {
            int level = Game.Buildings.GetLevel(definition.buildingId);
            var sb = new StringBuilder();
            sb.Append(Loc.T(definition.descriptionKey)).Append("\n\n");
            sb.Append(Loc.T("building.level")).Append(": ").Append(level).Append('/').Append(definition.maxLevel).Append('\n');
            sb.Append(Loc.T("bi.unlockhq")).Append(": ").Append(definition.unlockHeadquartersLevel).Append("\n\n");
            if (definition.availability != BuildingAvailability.Functional) sb.Append(Loc.T("bi.soon")).Append("\n\n");

            sb.Append(Loc.T("bi.levels")).Append(":\n");
            int shown = 0;
            for (int next = level + 1; next <= definition.maxLevel && shown < 4; next++, shown++)
            {
                BuildingLevelData data = definition.GetLevel(next);
                if (data == null) break;
                sb.Append(Loc.T("building.level")).Append(' ').Append(next).Append(" - ")
                  .Append(Loc.T("bi.power")).Append(' ').Append(NumberFormatter.Compact(data.powerContribution)).Append(" - ")
                  .Append(NumberFormatter.Duration(TimeSpan.FromSeconds(data.upgradeDurationSeconds))).Append('\n');
            }
            if (shown == 0) sb.Append(Loc.T("bi.maxlevel"));
            return sb.ToString();
        }

        private static void Set(TMP_Text text, string value)
        {
            if (text != null && text.text != value) text.text = value;
        }
    }
}
