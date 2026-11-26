using Deadzone.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deadzone.Tests
{
    /// <summary>
    /// Requires the generated prefabs. If these tests fail with "Folder missing", run Tools > DEADZONE > Setup Phase 01 first;
    /// that is a setup problem, not something to hide by disabling the test.
    /// </summary>
    public sealed class UiReferenceValidationTests
    {
        private static readonly string[] RequiredUiPrefabs =
        {
            "PrimaryButton", "SecondaryButton", "IconButton", "ResourceCounter", "PopupPanel", "ConfirmationDialog",
            "RewardPopup", "Tooltip", "LoadingScreen", "ProgressBar", "NotificationBadge", "TabNavigation",
            "ScrollablePanel", "SafeAreaController", "HeroCard", "ItemCard", "EventCard", "ShopCard", "UIOverlay"
        };

        [Test]
        public void AllRequiredUiPrefabsExist()
        {
            foreach (string name in RequiredUiPrefabs)
            {
                string path = EditorPaths.PrefabsUI + "/" + name + ".prefab";
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>(path), "Missing prefab: " + path + " (run Setup Phase 01)");
            }
        }

        [Test]
        public void NoUnassignedSerializedReferences()
        {
            var problems = ReferenceValidator.Validate();
            Assert.IsEmpty(problems, "Reference problems:\n" + string.Join("\n", problems));
        }
    }
}
