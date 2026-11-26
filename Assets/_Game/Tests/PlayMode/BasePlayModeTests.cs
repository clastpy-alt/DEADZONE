using System;
using System.Collections;
using System.IO;
using Deadzone.Buildings;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Deadzone.Tests.PlayMode
{
    /// <summary>
    /// Runs the real Base scene against an isolated save folder and a controllable clock (the player's own save is never touched).
    /// Requires the scenes in Build Settings (Setup Phase 01/02 does this).
    /// </summary>
    public sealed class BasePlayModeTests
    {
        private sealed class TestClock : IGameClock
        {
            public long Now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            public long UtcNowSeconds => Now;
        }

        private string saveDirectory;
        private TestClock clock;

        [SetUp]
        public void SetUp()
        {
            saveDirectory = Path.Combine(Path.GetTempPath(), "deadzone-playmode-" + Guid.NewGuid().ToString("N"));
            clock = new TestClock();
            Game.ResetForTests(saveDirectory, clock);
        }

        [TearDown]
        public void TearDown()
        {
            Game.ResetForTests(null, null); // back to the normal save folder and the real clock
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        private static IEnumerator LoadBase()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(SceneNames.Base);
            Assert.IsNotNull(load, "Base scene is not in Build Settings (run Setup Phase 01)");
            while (!load.isDone) yield return null;
            yield return null;
            yield return null;
        }

        private static BuildingSelectable FindBuilding(string id)
        {
            foreach (BuildingSelectable selectable in UnityEngine.Object.FindObjectsByType<BuildingSelectable>())
            {
                if (selectable.BuildingId == id) return selectable;
            }
            return null;
        }

        /// <summary>The info panel hides its sheet by deactivating its child content, not the panel object itself.</summary>
        private static bool HasVisibleChild(Component panel)
        {
            foreach (Transform child in panel.transform)
            {
                if (child.gameObject.activeInHierarchy) return true;
            }
            return false;
        }

        [UnityTest]
        public IEnumerator BaseScene_Loads_WithHudCameraToastAndAllBuildings()
        {
            yield return LoadBase();

            Assert.IsTrue(Game.IsInitialized);
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<HudController>(), "HUD");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<IsometricCameraController>(), "camera controller");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<ToastView>(FindObjectsInactive.Include), "toast");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<ConstructionQueuePanel>(FindObjectsInactive.Include), "construction queue panel");
            foreach (string id in new[] { "headquarters", "barracks", "hospital", "research_lab", "food_farm", "steel_factory", "warehouse" })
            {
                Assert.IsNotNull(FindBuilding(id), "building in scene: " + id);
            }
        }

        [UnityTest]
        public IEnumerator FreshSave_UsesTheTestEconomy()
        {
            yield return LoadBase();
            Assert.AreEqual(5000, Game.Wallet.Get(ResourceType.Food));
            Assert.AreEqual(2500, Game.Wallet.Get(ResourceType.Fuel));
            Assert.AreEqual(200, Game.Wallet.Get(ResourceType.Diamonds));
            Assert.Greater(Game.Player.combatPower, 0);
        }

        [UnityTest]
        public IEnumerator SelectingABuilding_ShowsTheInfoPanel_AndDeselectHidesIt()
        {
            yield return LoadBase();
            BuildingSelectable farm = FindBuilding("food_farm");

            GameEvents.RaiseBuildingSelected(farm);
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.AreSame(farm, BuildingSelectionController.Current);
            var panel = UnityEngine.Object.FindAnyObjectByType<BuildingInfoPanel>();
            Assert.IsNotNull(panel, "info panel exists");
            Assert.IsTrue(HasVisibleChild(panel), "the sheet content is shown after selecting");

            BuildingSelectionController.Deselect();
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.IsNull(BuildingSelectionController.Current);
            Assert.IsFalse(HasVisibleChild(panel), "the sheet content is hidden after deselect");
        }

        [UnityTest]
        public IEnumerator UpgradeLoop_PaysThenCompletesWhenTheClockAdvances()
        {
            yield return LoadBase();
            long woodBefore = Game.Wallet.Get(ResourceType.Wood);
            long powerBefore = Game.Player.combatPower;

            Assert.AreEqual(UpgradeBlockReason.None, Game.Upgrades.TryStartUpgrade("food_farm"));
            Assert.Less(Game.Wallet.Get(ResourceType.Wood), woodBefore, "the Food Farm upgrade costs Wood");
            Assert.AreEqual(1, Game.Upgrades.Queue.Count);

            clock.Now += 31;
            Game.Tick();
            yield return null;

            Assert.AreEqual(2, Game.Buildings.GetLevel("food_farm"));
            Assert.AreEqual(0, Game.Upgrades.Queue.Count);
            Assert.Greater(Game.Player.combatPower, powerBefore);
        }

        [UnityTest]
        public IEnumerator Production_IsCollectedIntoTheWallet()
        {
            yield return LoadBase();
            long foodBefore = Game.Wallet.Get(ResourceType.Food);

            clock.Now += 3600;
            Game.Tick();
            long collected = Game.Buildings.Collect("food_farm");

            Assert.AreEqual(1200, collected, "one hour of a level 1 Food Farm");
            Assert.AreEqual(foodBefore + 1200, Game.Wallet.Get(ResourceType.Food));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SaveReload_KeepsProgress_AndOfflineCompletionIsReported()
        {
            yield return LoadBase();
            Game.Upgrades.TryStartUpgrade("food_farm");
            Game.Save(true);

            // Close the game, wait one hour, open it again.
            clock.Now += 3600;
            Game.ResetForTests(saveDirectory, clock);
            yield return LoadBase();

            Assert.AreEqual(2, Game.Buildings.GetLevel("food_farm"), "construction finished while closed");
            Assert.AreEqual(1, Game.LastOfflineCompletionCount);
            Assert.AreEqual(0, Game.Upgrades.Queue.Count);

            var toast = UnityEngine.Object.FindAnyObjectByType<ToastView>(FindObjectsInactive.Include);
            string text = toast.GetComponentInChildren<TMP_Text>(true).text;
            StringAssert.Contains("2", text, "the HUD shows the welcome-back construction toast");
            Assert.AreEqual(0, Game.ConsumeOfflineCompletions().Count, "reported only once");
        }

        [UnityTest]
        public IEnumerator OfflineProduction_IsCappedAtEightHours()
        {
            yield return LoadBase();
            Game.Save(true);

            clock.Now += 20 * 3600;
            Game.ResetForTests(saveDirectory, clock);
            yield return LoadBase();

            // 20 h away: production is limited by the 8 h offline cap AND by the building's own output buffer.
            long capByHours = 8 * 1200;
            long buffer = Game.Buildings.GetBufferCapacity("food_farm");
            Assert.Less(capByHours, 20 * 1200, "sanity: the 8 h cap is below 20 h of production");
            Assert.AreEqual(System.Math.Min(capByHours, buffer), Game.Buildings.GetStored("food_farm"),
                "min(8 h x 1200/h, buffer) - never 20 h of production");
        }

        [UnityTest]
        public IEnumerator LanguageSwitch_ChangesTextsAndIsSaved()
        {
            yield return LoadBase();
            Assert.AreEqual("Topla", Loc.T("bi.collect"));

            Game.SetLanguage(Language.English);
            Assert.AreEqual("Collect", Loc.T("bi.collect"));

            Game.ResetForTests(saveDirectory, clock);
            yield return LoadBase();
            Assert.AreEqual(Language.English, Game.Player.language, "language is part of the save");
            Assert.AreEqual("Collect", Loc.T("bi.collect"));
        }

        [UnityTest]
        public IEnumerator Camera_StartsWide_AndFocusMovesIt()
        {
            yield return LoadBase();
            var controller = UnityEngine.Object.FindAnyObjectByType<IsometricCameraController>();
            Camera cam = controller.GetComponent<Camera>();
            Assert.IsTrue(cam.orthographic);
            Assert.GreaterOrEqual(cam.orthographicSize, 18f, "the start view shows the base layout, not a close-up");

            Vector3 before = cam.transform.position;
            controller.FocusOn(FindBuilding("research_lab").transform.position);
            for (int i = 0; i < 30; i++) yield return null;
            Assert.AreNotEqual(before, cam.transform.position);
        }
    }
}
