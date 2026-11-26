using Deadzone.Data;
using Deadzone.Economy;
using Deadzone.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Deadzone.Tests
{
    public sealed class ResourceWalletTests
    {
        private GameConfig config;
        private PlayerData data;
        private ResourceWallet wallet;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            data = PlayerData.CreateNew(config);
            wallet = new ResourceWallet(data, config.GetCapacity);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Add_IncreasesBalance()
        {
            long before = wallet.Get(ResourceType.Wood);
            long added = wallet.Add(ResourceType.Wood, 250);
            Assert.AreEqual(250, added);
            Assert.AreEqual(before + 250, wallet.Get(ResourceType.Wood));
        }

        [Test]
        public void Add_NegativeOrZero_IsRejected()
        {
            long before = wallet.Get(ResourceType.Food);
            Assert.AreEqual(0, wallet.Add(ResourceType.Food, -100));
            Assert.AreEqual(0, wallet.Add(ResourceType.Food, 0));
            Assert.AreEqual(before, wallet.Get(ResourceType.Food));
        }

        [Test]
        public void Add_ClampsToCapacity()
        {
            long capacity = wallet.GetCapacity(ResourceType.Food);
            wallet.Add(ResourceType.Food, capacity * 2);
            Assert.AreEqual(capacity, wallet.Get(ResourceType.Food));
            Assert.AreEqual(0, wallet.Add(ResourceType.Food, 10));
        }

        [Test]
        public void TrySpend_InsufficientFunds_ChangesNothing()
        {
            long before = wallet.Get(ResourceType.Steel);
            Assert.IsFalse(wallet.TrySpend(ResourceType.Steel, before + 1));
            Assert.AreEqual(before, wallet.Get(ResourceType.Steel));
        }

        [Test]
        public void TrySpend_InvalidAmounts_AreRejected()
        {
            long before = wallet.Get(ResourceType.Gold);
            Assert.IsFalse(wallet.TrySpend(ResourceType.Gold, 0));
            Assert.IsFalse(wallet.TrySpend(ResourceType.Gold, -5));
            Assert.AreEqual(before, wallet.Get(ResourceType.Gold));
        }

        [Test]
        public void TrySpend_ExactBalance_LeavesZeroNeverNegative()
        {
            long balance = wallet.Get(ResourceType.Fuel);
            Assert.IsTrue(wallet.TrySpend(ResourceType.Fuel, balance));
            Assert.AreEqual(0, wallet.Get(ResourceType.Fuel));
            Assert.IsFalse(wallet.TrySpend(ResourceType.Fuel, 1));
            Assert.AreEqual(0, wallet.Get(ResourceType.Fuel));
        }

        [Test]
        public void Changed_IsRaisedWithNewBalance()
        {
            int calls = 0;
            long reported = -1;
            wallet.Changed += (type, amount) => { calls++; reported = amount; };

            wallet.Add(ResourceType.Wood, 10);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(wallet.Get(ResourceType.Wood), reported);

            wallet.TrySpend(ResourceType.Wood, 999999999); // rejected -> no event
            Assert.AreEqual(1, calls);
        }
    }
}
