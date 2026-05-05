using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class TestLabDetectorTests
    {
        [TearDown]
        public void TearDown() => TestLabDetector.OverrideForTests(null);

        [Test]
        public void InEditor_DefaultIsFalse()
        {
            TestLabDetector.OverrideForTests(null);
            Assert.IsFalse(TestLabDetector.IsTestLab());
        }

        [Test]
        public void Override_True_IsHonored()
        {
            TestLabDetector.OverrideForTests(true);
            Assert.IsTrue(TestLabDetector.IsTestLab());
        }

        [Test]
        public void Override_False_IsHonored()
        {
            TestLabDetector.OverrideForTests(false);
            Assert.IsFalse(TestLabDetector.IsTestLab());
        }

        [Test]
        public void Override_Null_RecomputesFromPlatform()
        {
            TestLabDetector.OverrideForTests(true);
            Assert.IsTrue(TestLabDetector.IsTestLab());
            TestLabDetector.OverrideForTests(null);
            Assert.IsFalse(TestLabDetector.IsTestLab(), "Should fall back to editor default after override cleared");
        }
    }
}
