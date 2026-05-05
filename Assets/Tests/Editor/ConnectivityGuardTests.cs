using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class ConnectivityGuardTests
    {
        [TearDown]
        public void TearDown() => ConnectivityGuard.OverrideForTests(null);

        [Test]
        public void Override_True_ReportsOnline()
        {
            ConnectivityGuard.OverrideForTests(() => true);
            Assert.IsTrue(ConnectivityGuard.IsOnline());
        }

        [Test]
        public void Override_False_ReportsOffline()
        {
            ConnectivityGuard.OverrideForTests(() => false);
            Assert.IsFalse(ConnectivityGuard.IsOnline());
        }

        [Test]
        public void NoOverride_FallsBackToApplicationReachability()
        {
            ConnectivityGuard.OverrideForTests(null);
            // Just ensure it doesn't throw — actual value depends on test machine.
            Assert.DoesNotThrow(() => ConnectivityGuard.IsOnline());
        }
    }
}
