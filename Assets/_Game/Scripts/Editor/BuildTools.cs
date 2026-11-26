using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Deadzone.EditorTools
{
    internal static class BuildTools
    {
        public static void BuildDevelopmentApk()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                EditorUtility.DisplayDialog("Android build support missing",
                    "Install 'Android Build Support' for this Unity version in Unity Hub (Installs > Add modules).", "OK");
                return;
            }

            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("No scenes", "Run Tools > DEADZONE > Setup Phase 01 first.", "OK");
                return;
            }

            Directory.CreateDirectory("Builds/Android");
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/Android/DEADZONE-dev.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[DEADZONE] APK build result: {report.summary.result} ({report.summary.totalErrors} errors)");
        }
    }
}
