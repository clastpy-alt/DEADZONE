using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deadzone.Core
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            Game.EnsureInitialized();

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not in Build Settings. Run Tools > DEADZONE > Setup Phase 01.");
                return;
            }

            Game.Save(false);
            Game.SetState(StateFor(sceneName));
            SceneManager.LoadScene(sceneName);
        }

        private static GameState StateFor(string sceneName)
        {
            switch (sceneName)
            {
                case SceneNames.MainMenu: return GameState.MainMenu;
                case SceneNames.Base: return GameState.Base;
                case SceneNames.WorldMap: return GameState.WorldMap;
                case SceneNames.Battle: return GameState.Battle;
                default: return GameState.Booting;
            }
        }
    }
}
