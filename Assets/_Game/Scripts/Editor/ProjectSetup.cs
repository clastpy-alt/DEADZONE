using Deadzone.Data;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    internal static class ProjectSetup
    {
        /// <summary>TextMeshPro needs its essential resources (default font) before any text can be generated.</summary>
        public static bool CheckTextMeshPro()
        {
            if (TMP_Settings.instance != null) return true;

            EditorUtility.DisplayDialog(
                "TextMeshPro resources missing",
                "Import them first: Window > TextMeshPro > Import TMP Essential Resources.\n\nThen run the tool again.",
                "OK");
            return false;
        }

        public static void ApplyProjectSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.productName = "DEADZONE: LAST EMPIRE";
            AssetDatabase.SaveAssets();
        }

        public static void EnsureGameConfig()
        {
            EditorPaths.EnsureFolder(EditorPaths.Resources);
            if (AssetDatabase.LoadAssetAtPath<GameConfig>(EditorPaths.GameConfigAsset) != null) return;

            var config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, EditorPaths.GameConfigAsset);
            AssetDatabase.SaveAssets();
        }
    }
}
