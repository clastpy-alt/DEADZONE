using Deadzone.Core;
using Deadzone.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button languageButton;

        public void Bind(Button play, Button newGame, Button language)
        {
            playButton = play;
            newGameButton = newGame;
            languageButton = language;
        }

        private void OnEnable()
        {
            if (playButton == null || newGameButton == null || languageButton == null)
            {
                Debug.LogError("[MainMenu] Button references are missing. Re-run Tools > DEADZONE > Setup Phase 01.");
                enabled = false;
                return;
            }

            Game.EnsureInitialized();
            Game.SetState(GameState.MainMenu);

            playButton.onClick.AddListener(OnPlay);
            newGameButton.onClick.AddListener(OnNewGame);
            languageButton.onClick.AddListener(OnLanguage);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlay);
            if (newGameButton != null) newGameButton.onClick.RemoveListener(OnNewGame);
            if (languageButton != null) languageButton.onClick.RemoveListener(OnLanguage);
        }

        private void OnPlay() => SceneLoader.Load(SceneNames.Base);

        private void OnNewGame()
        {
            UIOverlay.Confirm(Loc.T("menu.newgame"), Loc.T("newgame.confirm.message"), () =>
            {
                Game.StartNewGame();
                SceneLoader.Load(SceneNames.Base);
            });
        }

        private void OnLanguage()
        {
            Game.SetLanguage(Loc.Current == Language.Turkish ? Language.English : Language.Turkish);
        }
    }
}
