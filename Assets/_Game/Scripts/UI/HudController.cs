using Deadzone.Buildings;
using Deadzone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>Binds the commander header (name, level, power, VIP), the save button and the profile popup.</summary>
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text vipText;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button avatarButton;

        public void Bind(TMP_Text nameText, TMP_Text level, TMP_Text power, TMP_Text vip, Button save, Button avatar)
        {
            playerNameText = nameText;
            levelText = level;
            powerText = power;
            vipText = vip;
            saveButton = save;
            avatarButton = avatar;
        }

        private void OnEnable()
        {
            Game.EnsureInitialized();
            Game.SetState(GameState.Base);

            GameEvents.PlayerProfileChanged += Refresh;
            GameEvents.PowerChanged += Refresh;
            GameEvents.LanguageChanged += Refresh;
            GameEvents.GameSaved += OnGameSaved;
            GameEvents.ConstructionCompleted += OnConstructionCompleted;
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
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
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (avatarButton != null) avatarButton.onClick.RemoveListener(OnAvatarClicked);
        }

        private void OnSaveClicked() => Game.Save(true);

        private void OnGameSaved(bool manual)
        {
            if (manual) ToastView.Show(Loc.T("hud.saved"));
        }

        /// <summary>Completion notification (also fires for constructions that finished while the game was closed).</summary>
        private void OnConstructionCompleted(string buildingId, int level)
        {
            BuildingDefinition definition = Game.Buildings != null ? Game.Buildings.GetDefinition(buildingId) : null;
            string name = definition != null ? Loc.T(definition.nameKey) : buildingId;
            ToastView.Show(string.Format(Loc.T("cq.done"), name, level));
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
            if (levelText != null) levelText.text = $"{Loc.T("hud.level")} {player.commanderLevel}";
            if (powerText != null) powerText.text = $"{Loc.T("hud.power")}: {NumberFormatter.Compact(player.combatPower)}";
            if (vipText != null) vipText.text = $"VIP {player.vipLevel}";
        }
    }
}
