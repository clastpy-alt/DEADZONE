using System.Linq;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.UI;
using NUnit.Framework;

namespace Deadzone.Tests
{
    public sealed class UtilityTests
    {
        [TearDown]
        public void TearDown()
        {
            Loc.SetLanguage(Language.Turkish);
        }

        [TestCase(0L, "0")]
        [TestCase(999L, "999")]
        [TestCase(1000L, "1K")]
        [TestCase(12345L, "12.3K")]
        [TestCase(999999L, "999.9K")]
        [TestCase(1500000L, "1.5M")]
        [TestCase(2000000000L, "2B")]
        public void Compact_FormatsNumbers(long value, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Compact(value));
        }

        [Test]
        public void Localization_AllKeysHaveBothLanguages()
        {
            Assert.IsEmpty(Loc.FindIncompleteKeys().ToList());
        }

        [Test]
        public void Localization_SwitchesLanguage_AndFallsBackToKey()
        {
            Loc.SetLanguage(Language.English);
            Assert.AreEqual("Base", Loc.T("nav.base"));
            Loc.SetLanguage(Language.Turkish);
            Assert.AreEqual("Üs", Loc.T("nav.base"));
            Assert.AreEqual("does.not.exist", Loc.T("does.not.exist"));
        }

        [TestCase(0, 0, 0, 0, "00:00:00")]
        [TestCase(0, 1, 2, 3, "01:02:03")]
        [TestCase(0, 23, 59, 59, "23:59:59")]
        [TestCase(1, 1, 30, 0, "1d 01h")]
        [TestCase(2, 3, 0, 0, "2d 03h")]
        public void Duration_FormatsTimeSpans(int days, int hours, int minutes, int seconds, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Duration(new System.TimeSpan(days, hours, minutes, seconds)));
        }

        [Test]
        public void Duration_NegativeBecomesZero()
        {
            Assert.AreEqual("00:00:00", NumberFormatter.Duration(System.TimeSpan.FromSeconds(-30)));
        }

        [Test]
        public void Localization_ResourceNamesExistForEveryResourceType()
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                string key = "res." + type.ToString().ToLowerInvariant();
                Assert.AreNotEqual(key, Loc.T(key), "Missing localization key: " + key);
            }
        }
    }
}
