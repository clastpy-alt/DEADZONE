using System.IO;
using UnityEditor;

namespace Deadzone.EditorTools
{
    internal static class EditorPaths
    {
        public const string Root = "Assets/_Game";
        public const string Scenes = Root + "/Scenes";
        public const string Materials = Root + "/Art/Materials";
        public const string PrefabsBuildings = Root + "/Prefabs/Buildings";
        public const string PrefabsUI = Root + "/Prefabs/UI";
        public const string Resources = Root + "/Resources";
        public const string BuildingData = Root + "/ScriptableObjects/Buildings";

        public const string GameConfigAsset = Resources + "/GameConfig.asset";
        public const string BuildingDatabaseAsset = Resources + "/BuildingDatabase.asset";

        public static string ScenePath(string sceneName) => Scenes + "/" + sceneName + ".unity";

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
