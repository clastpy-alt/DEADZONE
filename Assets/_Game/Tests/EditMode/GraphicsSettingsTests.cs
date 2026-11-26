using Deadzone.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace Deadzone.Tests
{
    /// <summary>LOW/MEDIUM/HIGH preference: defaults, persistence and the generated preset asset (PHASE 02.5).</summary>
    public sealed class GraphicsSettingsTests
    {
        private bool hadKey;
        private int storedValue;

        [SetUp]
        public void SetUp()
        {
            hadKey = PlayerPrefs.HasKey(GraphicsSettingsService.PrefKey);
            storedValue = PlayerPrefs.GetInt(GraphicsSettingsService.PrefKey, 0);
            PlayerPrefs.DeleteKey(GraphicsSettingsService.PrefKey);
            GraphicsSettingsService.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            if (hadKey) PlayerPrefs.SetInt(GraphicsSettingsService.PrefKey, storedValue);
            else PlayerPrefs.DeleteKey(GraphicsSettingsService.PrefKey);
            GraphicsSettingsService.ResetForTests();
        }

        [Test]
        public void DetectDefault_Desktop_IsHigh()
        {
            Assert.AreEqual(GraphicsQuality.High, GraphicsSettingsService.DetectDefault(2000, 2, false));
        }

        [TestCase(3600, 8, GraphicsQuality.Low)]
        [TestCase(4000, 8, GraphicsQuality.Low)]
        [TestCase(8000, 4, GraphicsQuality.Low)]
        [TestCase(5800, 8, GraphicsQuality.Medium)]
        [TestCase(7000, 8, GraphicsQuality.Medium)]
        [TestCase(7600, 8, GraphicsQuality.High)]
        [TestCase(11000, 8, GraphicsQuality.High)]
        public void DetectDefault_Phone_FollowsMemoryAndCores(int memoryMb, int cores, GraphicsQuality expected)
        {
            Assert.AreEqual(expected, GraphicsSettingsService.DetectDefault(memoryMb, cores, true));
        }

        [Test]
        public void Set_PersistsAndLoads()
        {
            Assert.IsFalse(GraphicsSettingsService.HasSavedPreference);

            GraphicsSettingsService.Set(GraphicsQuality.Low);
            Assert.IsTrue(GraphicsSettingsService.HasSavedPreference);

            GraphicsSettingsService.ResetForTests();
            Assert.AreEqual(GraphicsQuality.Low, GraphicsSettingsService.Current);

            GraphicsSettingsService.Set(GraphicsQuality.Medium);
            GraphicsSettingsService.ResetForTests();
            Assert.AreEqual(GraphicsQuality.Medium, GraphicsSettingsService.Load());
        }

        [Test]
        public void Set_WithoutSave_DoesNotWritePreference()
        {
            GraphicsSettingsService.Set(GraphicsQuality.Medium, false);
            Assert.AreEqual(GraphicsQuality.Medium, GraphicsSettingsService.Current);
            Assert.IsFalse(GraphicsSettingsService.HasSavedPreference);
        }

        [Test]
        public void Load_InvalidStoredValue_FallsBackToDetectedDefault()
        {
            PlayerPrefs.SetInt(GraphicsSettingsService.PrefKey, 42);
            GraphicsQuality expected = GraphicsSettingsService.DetectDefault(SystemInfo.systemMemorySize, SystemInfo.processorCount, Application.isMobilePlatform);
            Assert.AreEqual(expected, GraphicsSettingsService.Load());
        }

        [Test]
        public void Set_RaisesChangedEvent()
        {
            GraphicsQuality seen = GraphicsQuality.High;
            int calls = 0;
            GraphicsSettingsService.Changed += q => { seen = q; calls++; };

            GraphicsSettingsService.Set(GraphicsQuality.Low, false);

            Assert.AreEqual(1, calls);
            Assert.AreEqual(GraphicsQuality.Low, seen);
        }

        [Test]
        public void FallbackProfiles_FrameRateTargetsRiseWithQuality()
        {
            int low = GraphicsProfile.Fallback(GraphicsQuality.Low).targetFrameRate;
            int medium = GraphicsProfile.Fallback(GraphicsQuality.Medium).targetFrameRate;
            int high = GraphicsProfile.Fallback(GraphicsQuality.High).targetFrameRate;
            Assert.AreEqual(30, low);
            Assert.AreEqual(60, high);
            Assert.Less(low, medium);
            Assert.Less(medium, high);
            Assert.IsFalse(GraphicsProfile.Fallback(GraphicsQuality.Low).fog, "Heavy effects must be off in Low.");
        }

        [Test]
        public void GeneratedProfileSet_HasThreeConsistentPresets()
        {
            var set = Resources.Load<GraphicsProfileSet>(GraphicsSettingsService.ProfileResource);
            Assert.IsNotNull(set, "Run Tools > DEADZONE > Setup Lighting Profiles.");

            GraphicsProfile low = set.Get(GraphicsQuality.Low);
            GraphicsProfile medium = set.Get(GraphicsQuality.Medium);
            GraphicsProfile high = set.Get(GraphicsQuality.High);

            Assert.IsTrue(low.HasPipeline);
            Assert.IsTrue(medium.HasPipeline);
            Assert.IsTrue(high.HasPipeline);
            Assert.AreNotSame(low.pipelineAsset, high.pipelineAsset);

            Assert.IsFalse(low.postProcessing, "Post-processing must be off in Low.");
            Assert.IsFalse(low.HasVolume);
            Assert.IsTrue(high.postProcessing);
            Assert.IsTrue(high.HasVolume);
            Assert.LessOrEqual(low.targetFrameRate, medium.targetFrameRate);
            Assert.LessOrEqual(medium.targetFrameRate, high.targetFrameRate);
        }
    }
}
