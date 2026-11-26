using Deadzone.Buildings;
using Deadzone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>Binds the commander header (name, level, power, VIP), the settings button and the profile popup. Saving is automatic (timer, pause, quit, every construction); a manual save lives in the settings panel.</summary>
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text vipText;
        [FormerlySerializedAs("saveButton")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private TMP_Text levelBadgeText;
        [SerializeField] private Button avatarButton;

        public void Bind(TMP_Text nameText, TMP_Text level, TMP_Text power, TMP_Text vip, Button settings, Button avatar)
        {
            playerNameText = nameText;
            levelText = level;
            powerText = power;
            vipText = vip;
            settingsButton = settings;
            avatarButton = avatar;
        }

        /// <summary>Optional: number shown on the level badge of the avatar (PHASE 02.5 premium HUD).</summary>
        public void BindLevelBadge(TMP_Text badgeText) => levelBadgeText = badgeText;

        private void OnEnable()
        {
            Game.EnsureInitialized();
            Game.SetState(GameState.Base);

            GameEvents.PlayerProfileChanged += Refresh;
            GameEvents.PowerChanged += Refresh;
            GameEvents.LanguageChanged += Refresh;
            GameEvents.GameSaved += OnGameSaved;
            GameEvents.ConstructionCompleted += OnConstructionCompleted;
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (avatarButton != null) avatarButton.onClick.AddListener(OnAvatarClicked);

            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.PlayerProfileChanged -= Refresh;
            GameEvents.PowerChanged -= Refresh;
            GameEvents.LanguageChanged -= Refresh;
            GameEvents.GameSaved -= OnGameSaved;
            GameEvents.ConstructionCompleted -= OnConstructionCompleted;
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (avatarButton != null) avatarButton.onClick.RemoveListener(OnAvatarClicked);
        }

        private void Start() => ShowOfflineCompletions();

        /// <summary>Welcome-back toast for constructions that finished while the game was closed (shown once).</summary>
        private static void ShowOfflineCompletions()
        {
            var finished = Game.ConsumeOfflineCompletions();
            if (finished.Count == 0) return;

            if (finished.Count == 1) ToastView.Show(ConstructionDoneText(finished[0].Key, finished[0].Value));
            else ToastView.Show(string.Format(Loc.T("cq.done.many"), finished.Count));
        }

        private static string ConstructionDoneText(string buildingId, int level)
        {
            BuildingDefinition definition = Game.Buildings != null ? Game.Buildings.GetDefinition(buildingId) : null;
            string name = definition != null ? Loc.T(definition.nameKey) : buildingId;
            return string.Format(Loc.T("cq.done"), name, level);
        }

        private void OnSettingsClicked() => SettingsPanel.Open();

        private void OnGameSaved(bool manual)
        {
            if (manual) ToastView.Show(Loc.T("hud.saved"));
        }

        /// <summary>Completion notification (also fires for constructions that finished while the game was closed).</summary>
        private void OnConstructionCompleted(string buildingId, int level)
        {
            ToastView.Show(ConstructionDoneText(buildingId, level));
        }

        private void OnAvatarClicked()
        {
            var player = Game.Player;
            if (player == null) return;

            string shortId = player.playerId.Length > 8 ? player.playerId.Substring(0, 8) : player.playerId;
            string body =
                $"{player.playerName}\n" +
                $"{Loc.T("hud.level")} {player.commanderLevel}\n" +
                $"{Loc.T("hud.power")}: {NumberFormatter.Compact(player.combatPower)}\n" +
                $"VIP {player.vipLevel}\n" +
                $"{Loc.T("profile.id")}: {shortId}";
            UIOverlay.ShowPopup(Loc.T("profile.title"), body);
        }

        private void Refresh()
        {
            var player = Game.Player;
            if (player == null) return;

            if (playerNameText != null) playerNameText.text = player.playerName;
            if (levelBadgeText != null) levelBadgeText.text = player.commanderLevel.ToString();
            if (levelText != null) levelText.text = $"{Loc.T("hud.level")} {player.commanderLevel}";
            if (powerText != null) powerText.text = $"{Loc.T("hud.power")}: {NumberFormatter.Compact(player.combatPower)}";
            if (vipText != null) vipText.text = $"VIP {player.vipLevel}";
        }
    }
}
