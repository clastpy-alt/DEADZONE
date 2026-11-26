using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Deadzone.Core;
using Deadzone.UI;
using Deadzone.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.5 menu tools: Generate LOD Models, Validate Graphics, Generate Performance Report.
    /// The report is a STATIC analysis of project files and the Base scene in the editor - it is not a device measurement.
    /// </summary>
    internal static class GraphicsTools
    {
        public const string ReportPath = "PERFORMANCE_REPORT.md";
        private const string LodChildName = "Mil_LOD1";
        private const float Lod1Threshold = 0.02f;

        // ------------------------------------------------------------------ LOD

        /// <summary>
        /// Adds a LODGroup to every building prefab: LOD0 = Mil_Visual (full model), LOD1 = a one-box proxy that only shows when the
        /// building is tiny on screen (below 2%, which the normal camera zoom range rarely reaches). Re-running replaces only the
        /// generated Mil_LOD1 child.
        /// </summary>
        public static string GenerateLodModels()
        {
            var report = new StringBuilder();
            int done = 0;
            Material proxyMaterial = MilitaryMaterials.Get("Mil_ConcreteDark");
            Mesh cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { EditorPaths.PrefabsBuildings }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform visual = root.transform.Find("Mil_Visual");
                    if (visual == null) continue;

                    Transform old = root.transform.Find(LodChildName);
                    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

                    Renderer[] full = visual.GetComponentsInChildren<Renderer>(true);
                    if (full.Length == 0) continue;
                    Bounds bounds = full[0].bounds;
                    foreach (Renderer renderer in full) bounds.Encapsulate(renderer.bounds);

                    var proxy = new GameObject(LodChildName);
                    proxy.transform.SetParent(root.transform, false);
                    proxy.transform.position = bounds.center;
                    proxy.transform.localScale = new Vector3(bounds.size.x * 0.9f, bounds.size.y * 0.85f, bounds.size.z * 0.9f);
                    proxy.AddComponent<MeshFilter>().sharedMesh = cube;
                    var proxyRenderer = proxy.AddComponent<MeshRenderer>();
                    proxyRenderer.sharedMaterial = proxyMaterial;
                    proxyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    var group = root.GetComponent<LODGroup>();
                    if (group == null) group = root.AddComponent<LODGroup>();
                    group.SetLODs(new[]
                    {
                        new LOD(Lod1Threshold, full),
                        new LOD(0f, new Renderer[] { proxyRenderer }),
                    });
                    group.RecalculateBounds();

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    done++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            report.AppendLine($"LOD groups written on {done} building prefab(s): LOD0 full model, LOD1 box proxy below {Lod1Threshold:P0} screen height.");
            report.AppendLine("Note: LOD1 is intentionally rare in the normal zoom range; the Low profile (lodBias 0.7) switches earlier.");
            return report.ToString().Trim();
        }

        // ------------------------------------------------------------------ validation

        public static List<string> ValidateGraphics(out string summary)
        {
            var problems = new List<string>();
            var info = new StringBuilder();

            // Profiles and quality levels
            var set = Resources.Load<GraphicsProfileSet>(GraphicsSettingsService.ProfileResource);
            if (set == null) problems.Add("Resources/GraphicsProfiles.asset missing (Tools > DEADZONE > Setup Lighting Profiles)");
            else
            {
                foreach (GraphicsQuality quality in Enum.GetValues(typeof(GraphicsQuality)))
                {
                    if (!set.Get(quality).HasPipeline) problems.Add($"{quality}: no URP pipeline asset assigned");
                }
            }
            var levels = new List<string>(QualitySettings.names);
            foreach (string name in new[] { "DZ_Low", "DZ_Medium", "DZ_High" })
            {
                if (!levels.Contains(name)) problems.Add($"Quality level {name} missing in Project Settings");
            }

            // Icons and materials
            problems.AddRange(UiIconGenerator.FindMissing().ConvertAll(n => "Icon missing: " + n));
            int missingMaterials = 0;
            foreach (string name in MilitaryMaterialNames())
            {
                if (AssetDatabase.LoadAssetAtPath<Material>($"{EditorPaths.Materials}/{name}.mat") == null) missingMaterials++;
            }
            if (missingMaterials > 0) problems.Add($"{missingMaterials} military material(s) missing (Tools > DEADZONE > Generate Military Materials)");

            // Building prefabs: materials and shaders
            int prefabs = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { EditorPaths.PrefabsBuildings }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                prefabs++;
                foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material == null) { problems.Add($"{prefab.name}/{renderer.name}: missing material"); continue; }
                        if (material.shader != null && !material.shader.name.StartsWith("Universal Render Pipeline") && !material.shader.name.StartsWith("Sprites"))
                        {
                            problems.Add($"{prefab.name}/{renderer.name}: shader '{material.shader.name}' is not URP");
                        }
                    }
                }
            }
            info.AppendLine($"Building prefabs checked: {prefabs}");

            // Textures: budget rule max 2048
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { EditorPaths.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.maxTextureSize > 2048)
                {
                    problems.Add($"{path}: max texture size {importer.maxTextureSize} (> 2048 budget rule)");
                }
            }

            // Base scene: lighting rig, lights, HUD icons
            SceneCheck(problems, info);

            summary = info.ToString().Trim();
            return problems;
        }

        private static IEnumerable<string> MilitaryMaterialNames()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Material Mil_", new[] { EditorPaths.Materials }))
            {
                yield return Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        private static void SceneCheck(List<string> problems, StringBuilder info)
        {
            WithBaseScene(scene =>
            {
                bool rig = false;
                bool settings = false;
                int lights = 0;
                int directional = 0;
                int missingIcons = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == "Lighting Rig [GENERATED]") rig = true;
                    foreach (Light light in root.GetComponentsInChildren<Light>(false))
                    {
                        lights++;
                        if (light.type == LightType.Directional) directional++;
                    }
                    if (root.GetComponentInChildren<SettingsPanel>(true) != null) settings = true;
                    foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    {
                        bool wantsIcon = t.name == "Icon" && t.parent != null &&
                                         (t.parent.name.StartsWith("Nav_") || t.parent.name.StartsWith("Side_") || t.parent.name.StartsWith("Counter_"));
                        if (!wantsIcon) continue;
                        var image = t.GetComponent<Image>();
                        if (image == null || image.sprite == null) missingIcons++;
                    }
                }
                if (!rig) problems.Add("Base scene: 'Lighting Rig [GENERATED]' missing");
                if (directional != 1) problems.Add($"Base scene: expected 1 active directional light, found {directional}");
                if (lights > 2) problems.Add($"Base scene: {lights} active lights (budget: 1 sun + at most 1 accent)");
                if (!settings) problems.Add("Base scene: Settings panel missing (Tools > DEADZONE > Generate Premium HUD)");
                if (missingIcons > 0) problems.Add($"Base scene: {missingIcons} HUD button/counter without icon sprite (Generate Premium HUD)");
                info.AppendLine($"Base scene lights: {lights} (directional {directional})");
                return true;
            });
        }

        // ------------------------------------------------------------------ performance report

        public static string GeneratePerformanceReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# PERFORMANCE_REPORT");
            sb.AppendLine();
            sb.AppendLine($"Üretim: {DateTime.Now:yyyy-MM-dd HH:mm} (Unity {Application.unityVersion}). **Statik analiz**: proje dosyaları ve editörde açılan Base sahnesi. Cihaz FPS / RAM / ısınma ölçümü DEĞİLDİR; bu satırlar PERFORMANCE_BUDGET.md'de \"ÖLÇÜLMEDİ\" kalır.");
            sb.AppendLine();

            // Asset sizes by category
            var sizes = new Dictionary<string, long>();
            long total = 0;
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.StartsWith(EditorPaths.Root + "/") || !File.Exists(path)) continue;
                long length = new FileInfo(path).Length;
                string category = Categorize(path);
                sizes[category] = (sizes.TryGetValue(category, out long current) ? current : 0) + length;
                total += length;
            }
            sb.AppendLine("## Proje varlık boyutları (Assets/_Game, kaynak dosya; build boyutu değil)");
            sb.AppendLine("| Kategori | MB | Bütçe (MB) |");
            sb.AppendLine("|---|---|---|");
            AppendRow(sb, "Doku", sizes, "Textures", 900);
            AppendRow(sb, "3D modeller (mesh)", sizes, "Meshes", 650);
            AppendRow(sb, "Materyal", sizes, "Materials", -1);
            AppendRow(sb, "Ses", sizes, "Audio", 250);
            AppendRow(sb, "Prefab / sahne", sizes, "Prefabs & scenes", -1);
            AppendRow(sb, "Kod", sizes, "Code", 200);
            AppendRow(sb, "Diğer", sizes, "Other", -1);
            sb.AppendLine($"| **Toplam** | **{total / 1048576.0:F2}** | 3000 |");
            sb.AppendLine();

            // Base scene static metrics
            sb.AppendLine("## Base sahnesi (editör, aktif nesneler)");
            string scene = WithBaseScene(s => SceneMetrics(s)) ?? "Base sahnesi bulunamadı.";
            sb.AppendLine(scene);
            sb.AppendLine();

            // Graphics profiles
            sb.AppendLine("## Grafik profilleri (ayar; hedef FPS ölçülmedi)");
            var set = Resources.Load<GraphicsProfileSet>(GraphicsSettingsService.ProfileResource);
            if (set != null)
            {
                foreach (GraphicsQuality quality in Enum.GetValues(typeof(GraphicsQuality)))
                {
                    GraphicsProfile p = set.Get(quality);
                    sb.AppendLine($"- {quality}: hedef {p.targetFrameRate} FPS, LOD bias {p.lodBias:F1}, post-process {(p.postProcessing ? "açık" : "kapalı")}, yumuşak gölge {(p.softShadows ? "açık" : "kapalı")}, sis {(p.fog ? "açık" : "kapalı")}");
                }
            }
            else sb.AppendLine("- GraphicsProfiles.asset bulunamadı.");

            File.WriteAllText(ReportPath, sb.ToString(), new UTF8Encoding(false));
            return $"Report written to {ReportPath}\n" + sb;
        }

        private static string SceneMetrics(Scene scene)
        {
            long triangles = 0;
            int renderers = 0;
            int staticRenderers = 0;
            var materials = new HashSet<Material>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(false))
                {
                    if (!renderer.enabled) continue;
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null) continue;
                    renderers++;
                    if (renderer.gameObject.isStatic) staticRenderers++;
                    Mesh mesh = filter.sharedMesh;
                    for (int i = 0; i < mesh.subMeshCount; i++) triangles += mesh.GetIndexCount(i) / 3;
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material != null) materials.Add(material);
                    }
                }
            }
            return $"- Aktif MeshRenderer: {renderers} (statik: {staticRenderers})\n" +
                   $"- Üçgen (LOD0, tüm bina katmanları dahil; kamera kırpması yok): {triangles:N0} (bütçe: Düşük ≤150 bin, Orta ≤250 bin, Yüksek ≤400 bin)\n" +
                   $"- Benzersiz materyal: {materials.Count}\n" +
                   "- Draw call sayısı burada HESAPLANMAZ; Game > Stats / Frame Debugger ile editörde ölçülür.";
        }

        private static string Categorize(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".png": case ".jpg": case ".jpeg": case ".tga": case ".exr": case ".psd": case ".hdr": return "Textures";
                case ".fbx": case ".obj": case ".mesh": return "Meshes";
                case ".wav": case ".mp3": case ".ogg": return "Audio";
                case ".mat": return "Materials";
                case ".prefab": case ".unity": return "Prefabs & scenes";
                case ".cs": case ".asmdef": return "Code";
                case ".asset":
                    return path.Contains("/Models/") ? "Meshes" : "Other";
                default: return "Other";
            }
        }

        private static void AppendRow(StringBuilder sb, string label, Dictionary<string, long> sizes, string key, int budgetMb)
        {
            long bytes = sizes.TryGetValue(key, out long value) ? value : 0;
            sb.AppendLine($"| {label} | {bytes / 1048576.0:F2} | {(budgetMb > 0 ? budgetMb.ToString() : "-")} |");
        }

        // ------------------------------------------------------------------ scene access

        private static T WithBaseScene<T>(Func<Scene, T> action)
        {
            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path)) return default;

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                return action(scene);
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
