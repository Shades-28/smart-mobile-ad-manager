using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdResultTests
    {
        [Test]
        public void Rewarded_OnlyTrueForRewardedCode()
        {
            Assert.IsTrue(new AdResult(AdResultCode.Rewarded).WasRewarded);
            Assert.IsFalse(new AdResult(AdResultCode.Closed).WasRewarded);
            Assert.IsFalse(new AdResult(AdResultCode.Cancelled).WasRewarded);
        }

        [Test]
        public void WasShown_TrueForClosedOrShown()
        {
            Assert.IsTrue(new AdResult(AdResultCode.Closed).WasShown);
            Assert.IsTrue(new AdResult(AdResultCode.Shown).WasShown);
            Assert.IsFalse(new AdResult(AdResultCode.Failed).WasShown);
            Assert.IsFalse(new AdResult(AdResultCode.NotReady).WasShown);
        }

        [Test]
        public void Failed_TrueForAllFailureCodes()
        {
            Assert.IsTrue(new AdResult(AdResultCode.Failed).Failed);
            Assert.IsTrue(new AdResult(AdResultCode.LoadFailed).Failed);
            Assert.IsTrue(new AdResult(AdResultCode.TimedOut).Failed);
            Assert.IsFalse(new AdResult(AdResultCode.Cancelled).Failed);
            Assert.IsFalse(new AdResult(AdResultCode.NotReady).Failed);
        }

        [Test]
        public void ImplicitConversion_ToAndFromAdResultCode()
        {
            AdResultCode code = AdResultCode.Rewarded;
            AdResult result = code; // implicit
            Assert.IsTrue(result.WasRewarded);

            AdResultCode roundtrip = result; // implicit back
            Assert.AreEqual(AdResultCode.Rewarded, roundtrip);
        }

        [Test]
        public void Disabled_AndNotReady_AreDistinct()
        {
            Assert.IsTrue(new AdResult(AdResultCode.Disabled).WasDisabled);
            Assert.IsFalse(new AdResult(AdResultCode.Disabled).WasNotReady);
            Assert.IsTrue(new AdResult(AdResultCode.NotReady).WasNotReady);
            Assert.IsFalse(new AdResult(AdResultCode.NotReady).WasDisabled);
        }
    }
}
