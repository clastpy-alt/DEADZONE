using Deadzone.Buildings;
using NUnit.Framework;
using UnityEngine;

namespace Deadzone.Tests.EditMode
{
    /// <summary>Building Visual Tier System (PHASE 02.5): level to tier mapping and additive tier switching.</summary>
    public sealed class BuildingVisualTierTests
    {
        [TestCase(1, 1)]
        [TestCase(5, 1)]
        [TestCase(6, 2)]
        [TestCase(10, 2)]
        [TestCase(11, 3)]
        [TestCase(20, 4)]
        [TestCase(25, 5)]
        [TestCase(30, 6)]
        [TestCase(31, 7)]
        [TestCase(35, 7)]
        public void TierForLevel_MapsFiveLevelsPerTier(int level, int expectedTier)
        {
            Assert.AreEqual(expectedTier, BuildingVisualTiers.TierForLevel(level));
        }

        [TestCase(0)]
        [TestCase(-4)]
        public void TierForLevel_BelowOne_IsTierOne(int level)
        {
            Assert.AreEqual(1, BuildingVisualTiers.TierForLevel(level));
        }

        [Test]
        public void TierForLevel_AboveMaximum_StaysAtLastTier()
        {
            Assert.AreEqual(BuildingVisualTiers.TierCount, BuildingVisualTiers.TierForLevel(99));
        }

        [Test]
        public void TierForLevel_NeverDecreasesWithLevel()
        {
            int previous = 0;
            for (int level = 1; level <= 35; level++)
            {
                int tier = BuildingVisualTiers.TierForLevel(level);
                Assert.GreaterOrEqual(tier, previous, "level " + level);
                previous = tier;
            }
        }

        [Test]
        public void ShowTier_IsAdditive_AndHidesHigherTiers()
        {
            var root = new GameObject("TierTest");
            try
            {
                var groups = new GameObject[BuildingVisualTiers.TierCount];
                for (int i = 0; i < groups.Length; i++)
                {
                    groups[i] = new GameObject("Tier" + (i + 1));
                    groups[i].transform.SetParent(root.transform, false);
                }

                var tiers = root.AddComponent<BuildingVisualTiers>();
                tiers.Configure(groups);

                tiers.ShowTier(3);
                for (int i = 0; i < groups.Length; i++)
                {
                    Assert.AreEqual(i < 3, groups[i].activeSelf, "tier " + (i + 1) + " at tier 3");
                }
                Assert.AreEqual(3, tiers.CurrentTier);

                tiers.ShowTier(1);
                Assert.IsTrue(groups[0].activeSelf);
                Assert.IsFalse(groups[1].activeSelf, "dropping back hides the higher tiers");

                tiers.ShowTier(99);
                Assert.AreEqual(BuildingVisualTiers.TierCount, tiers.CurrentTier);
                Assert.IsTrue(groups[groups.Length - 1].activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ShowTier_WithoutGroups_DoesNotThrow()
        {
            var root = new GameObject("TierTestEmpty");
            try
            {
                var tiers = root.AddComponent<BuildingVisualTiers>();
                Assert.DoesNotThrow(() => tiers.ShowTier(4));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
