using UnityEngine;

namespace Deadzone.Core
{
    /// <summary>Lives in the Bootstrap scene: initializes services, then loads the first scene.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string firstScene = SceneNames.MainMenu;

        private void Start()
        {
            Game.EnsureInitialized();
            SceneLoader.Load(firstScene);
        }
    }
}
