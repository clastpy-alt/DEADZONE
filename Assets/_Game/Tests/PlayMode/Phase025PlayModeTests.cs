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
using UnityEngine.UI;

namespace Deadzone.Tests.PlayMode
{
    /// <summary>PHASE 02.5: graphics quality persistence, settings panel, HUD gear, building preview and camera defaults in the real Base scene.</summary>
    public sealed class Phase025PlayModeTests
    {
        private sealed class TestClock : IGameClock
        {
            public long Now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            public long UtcNowSeconds => Now;
        }

        private string saveDirectory;
        private bool hadQualityPref;
        private int originalQualityPref;

        [SetUp]
        public void SetUp()
        {
            saveDirectory = Path.Combine(Path.GetTempPath(), "deadzone-playmode25-" + Guid.NewGuid().ToString("N"));
            Game.ResetForTests(saveDirectory, new TestClock());
            hadQualityPref = PlayerPrefs.HasKey(GraphicsSettingsService.PrefKey);
            originalQualityPref = PlayerPrefs.GetInt(GraphicsSettingsService.PrefKey, 0);
        }

        [TearDown]
        public void TearDown()
        {
            // Put the developer's own quality choice back.
            if (hadQualityPref) PlayerPrefs.SetInt(GraphicsSettingsService.PrefKey, originalQualityPref);
            else PlayerPrefs.DeleteKey(GraphicsSettingsService.PrefKey);
            PlayerPrefs.Save();
            GraphicsSettingsService.ResetForTests();
            GraphicsSettingsService.Load();
            GraphicsSettingsService.ApplyCurrent();

            Game.ResetForTests(null, null);
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        private static IEnumerator LoadBase()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(SceneNames.Base);
            Assert.IsNotNull(load, "Base scene is not in Build Settings");
            while (!load.isDone) yield return null;
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator GraphicsQuality_IsSaved_AndRestoredAfterAResetForEveryLevel()
        {
            yield return LoadBase();
            foreach (GraphicsQuality quality in new[] { GraphicsQuality.Low, GraphicsQuality.Medium, GraphicsQuality.High })
            {
                GraphicsSettingsService.Set(quality, true);
                Assert.AreEqual((int)quality, PlayerPrefs.GetInt(GraphicsSettingsService.PrefKey, -1), "saved: " + quality);

                GraphicsSettingsService.ResetForTests();
                Assert.AreEqual(quality, GraphicsSettingsService.Load(), "restored: " + quality);
                Assert.IsNotNull(GraphicsSettingsService.ActiveProfile, "profile: " + quality);
            }
        }

        [UnityTest]
        public IEnumerator SettingsPanel_OpensFromTheGear_AndClosesAgain()
        {
            yield return LoadBase();
            var panel = UnityEngine.Object.FindAnyObjectByType<SettingsPanel>(FindObjectsInactive.Include);
            Assert.IsNotNull(panel, "settings panel is part of the Base scene (run Tools > DEADZONE > Generate Premium HUD)");

            // The HUD gear must be wired to the panel.
            Button gear = null;
            foreach (Button button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
            {
                if (button.name == "SettingsButton") gear = button;
            }
            Assert.IsNotNull(gear, "HUD gear button");
            gear.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsTrue(panel.gameObject.activeInHierarchy, "panel is shown after the gear is pressed");

            panel.Hide();
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsFalse(panel.gameObject.activeInHierarchy, "panel is hidden after Close");
        }

        [UnityTest]
        public IEnumerator HudGear_ReplacesTheSaveButton()
        {
            yield return LoadBase();
            var hud = UnityEngine.Object.FindAnyObjectByType<HudController>();
            Assert.IsNotNull(hud);
            bool gear = false;
            foreach (Button button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
            {
                if (button.name == "SettingsButton") gear = button.gameObject.activeInHierarchy;
                if (button.name == "SaveButton") Assert.IsFalse(button.gameObject.activeInHierarchy, "the old save button must not be visible in the HUD");
            }
            Assert.IsTrue(gear, "gear button exists");
        }

        [UnityTest]
        public IEnumerator BuildingPreview_RendersTheSelectedBuilding_IntoARenderTexture()
        {
            yield return LoadBase();
            BuildingSelectable hospital = null;
            foreach (BuildingSelectable selectable in UnityEngine.Object.FindObjectsByType<BuildingSelectable>())
            {
                if (selectable.BuildingId == "hospital") hospital = selectable;
            }
            Assert.IsNotNull(hospital);

            GameEvents.RaiseBuildingSelected(hospital);
            yield return new WaitForSecondsRealtime(0.8f);

            var rig = GameObject.Find("BuildingPreviewRig");
            Assert.IsNotNull(rig, "the preview rig is created on the first selection");
            Camera previewCamera = rig.GetComponentInChildren<Camera>(true);
            Assert.IsNotNull(previewCamera, "preview camera");
            Assert.IsTrue(previewCamera.enabled, "preview camera runs while a building is selected");
            Assert.IsNotNull(previewCamera.targetTexture, "renders into a texture");

            // Regression: the camera must not be moved together with the model it films.
            Transform pivot = rig.transform.Find("Pivot");
            Assert.IsNotNull(pivot);
            Assert.IsFalse(previewCamera.transform.IsChildOf(pivot) || pivot.IsChildOf(previewCamera.transform), "camera and pivot are separate");
            Assert.Greater(pivot.GetComponentsInChildren<Renderer>().Length, 0, "the clone has renderers");
            Bounds bounds = pivot.GetComponentInChildren<Renderer>().bounds;
            foreach (Renderer renderer in pivot.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);
            Vector3 viewport = previewCamera.WorldToViewportPoint(bounds.center);
            Assert.That(viewport.z, Is.GreaterThan(0f), "model is in front of the camera");
            Assert.That(viewport.x, Is.InRange(0.2f, 0.8f), "model is horizontally centred in the preview");
            Assert.That(viewport.y, Is.InRange(0.2f, 0.8f), "model is vertically centred in the preview");

            BuildingSelectionController.Deselect();
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsFalse(previewCamera.enabled, "the preview camera stops when nothing is selected");
        }

        [UnityTest]
        public IEnumerator Camera_ShowsAWideEnoughViewOfTheBase()
        {
            yield return LoadBase();
            var controller = UnityEngine.Object.FindAnyObjectByType<IsometricCameraController>();
            Assert.IsNotNull(controller);
            Camera main = Camera.main;
            Assert.IsNotNull(main);
            Assert.IsTrue(main.orthographic);
            Assert.GreaterOrEqual(main.orthographicSize, 22f, "start zoom shows a good part of the base");
            Assert.LessOrEqual(main.orthographicSize, 40f, "within the zoom-out limit");
        }
    }
}
