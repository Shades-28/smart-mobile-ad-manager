using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class TestDeviceRegistryTests
    {
        [SetUp]
        public void Setup() => TestDeviceRegistry.Clear();

        [TearDown]
        public void TearDown() => TestDeviceRegistry.Clear();

        [Test]
        public void Add_AddsId()
        {
            TestDeviceRegistry.Add("DEVICE-A");
            Assert.IsTrue(TestDeviceRegistry.Contains("DEVICE-A"));
            Assert.AreEqual(1, TestDeviceRegistry.Ids.Count);
        }

        [Test]
        public void Add_DeduplicatesIds()
        {
            TestDeviceRegistry.Add("DEVICE-A");
            TestDeviceRegistry.Add("DEVICE-A");
            Assert.AreEqual(1, TestDeviceRegistry.Ids.Count);
        }

        [Test]
        public void Add_IgnoresEmptyOrWhitespace()
        {
            TestDeviceRegistry.Add("");
            TestDeviceRegistry.Add(null);
            TestDeviceRegistry.Add("   ");
            Assert.AreEqual(0, TestDeviceRegistry.Ids.Count);
        }

        [Test]
        public void AddRange_AddsMultiple()
        {
            TestDeviceRegistry.AddRange(new[] { "A", "B", "C" });
            Assert.AreEqual(3, TestDeviceRegistry.Ids.Count);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            TestDeviceRegistry.Add("A");
            TestDeviceRegistry.Add("B");
            TestDeviceRegistry.Clear();
            Assert.AreEqual(0, TestDeviceRegistry.Ids.Count);
        }
    }
}
