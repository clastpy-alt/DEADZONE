using System;
using System.Collections;
using System.IO;
using Deadzone.Buildings;
using Deadzone.Core;
using Deadzone.UI;
using Deadzone.Visuals;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Deadzone.Tests.PlayMode
{
    /// <summary>
    /// QA helper (explicit, not part of "Run All"): plays the Base scene through the PHASE 02.5 screens and saves portrait PNGs to
    /// Docs/Screenshots/Phase025 inside the project. Set the Game view to "Portrait 1080x1920" before running it.
    /// </summary>
    public sealed class Phase025ScreenshotTests
    {
        private sealed class TestClock : IGameClock
        {
            public long Now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            public long UtcNowSeconds => Now;
        }

        private string saveDirectory;
        private string outputDirectory;
        private bool hadQualityPref;
        private int originalQualityPref;

        [SetUp]
        public void SetUp()
        {
            saveDirectory = Path.Combine(Path.GetTempPath(), "deadzone-shots-" + Guid.NewGuid().ToString("N"));
            outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "Screenshots", "Phase025"));
            Directory.CreateDirectory(outputDirectory);
            Game.ResetForTests(saveDirectory, new TestClock());
            hadQualityPref = PlayerPrefs.HasKey(GraphicsSettingsService.PrefKey);
            originalQualityPref = PlayerPrefs.GetInt(GraphicsSettingsService.PrefKey, 0);
        }

        [TearDown]
        public void TearDown()
        {
            if (hadQualityPref) PlayerPrefs.SetInt(GraphicsSettingsService.PrefKey, originalQualityPref);
            else PlayerPrefs.DeleteKey(GraphicsSettingsService.PrefKey);
            PlayerPrefs.Save();
            GraphicsSettingsService.ResetForTests();
            GraphicsSettingsService.Load();
            GraphicsSettingsService.ApplyCurrent();
            Game.ResetForTests(null, null);
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(outputDirectory, name + ".png"), texture.EncodeToPNG());
            Debug.Log($"[DEADZONE] Screenshot {name}.png {texture.width}x{texture.height}");
            UnityEngine.Object.Destroy(texture);
        }

        private static BuildingSelectable Find(string id)
        {
            foreach (BuildingSelectable selectable in UnityEngine.Object.FindObjectsByType<BuildingSelectable>())
            {
                if (selectable.BuildingId == id) return selectable;
            }
            return null;
        }

        [UnityTest, Explicit("QA screenshots - run on demand")]
        public IEnumerator CapturePhase025Screens()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(SceneNames.Base);
            while (!load.isDone) yield return null;
            GraphicsSettingsService.Set(GraphicsQuality.High, false);
            yield return new WaitForSecondsRealtime(1.5f);

            yield return Capture("01_base_overview");

            GameEvents.RaiseBuildingSelected(Find("headquarters"));
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Capture("02_hq_closeup");

            GameEvents.RaiseBuildingSelected(Find("hospital"));
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Capture("03_building_info");

            Game.Upgrades.TryStartUpgrade("food_farm");
            BuildingSelectionController.Deselect();
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture("04_construction");

            yield return Capture("05_premium_hud");

            SettingsPanel.Open();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("06_graphics_settings");

            UnityEngine.Object.FindAnyObjectByType<SettingsPanel>(FindObjectsInactive.Include).Hide();
            GameEvents.RaiseBuildingSelected(Find("barracks"));
            GraphicsSettingsService.Set(GraphicsQuality.Low, false);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Capture("07_low_mode");

            GraphicsSettingsService.Set(GraphicsQuality.High, false);
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("08_high_mode");

            Assert.GreaterOrEqual(Directory.GetFiles(outputDirectory, "0*.png").Length, 8);
        }
    }
}
