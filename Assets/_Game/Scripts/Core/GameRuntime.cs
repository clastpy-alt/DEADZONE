using UnityEngine;

namespace Deadzone.Core
{
    /// <summary>Persistent runtime host: autosave timer, save on pause/quit, audio service.</summary>
    public sealed class GameRuntime : MonoBehaviour
    {
        private static GameRuntime instance;
        private float timer;

        public static AudioService Audio { get; private set; }

        public static void EnsureExists()
        {
            if (instance != null || !Application.isPlaying) return;

            var go = new GameObject("[GameRuntime]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameRuntime>();
            Audio = go.AddComponent<AudioService>();
        }

        private void Update()
        {
            if (!Game.IsInitialized) return;

            timer += Time.unscaledDeltaTime;
            if (timer >= Game.Config.autosaveIntervalSeconds)
            {
                timer = 0f;
                Game.Save(false);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && Game.IsInitialized) Game.Save(false);
        }

        private void OnApplicationQuit()
        {
            if (Game.IsInitialized) Game.Save(false);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                Audio = null;
            }
        }
    }
}
