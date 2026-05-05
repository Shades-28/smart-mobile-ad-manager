using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdLoadSchedulerTests
    {
        // -- Backoff math -----------------------------------------------------

        [Test]
        public void ComputeDelay_AttemptOne_EqualsBaseDelay_NoJitter()
        {
            var s = NewScheduler(jitter: 0f, baseDelay: 2f, maxDelay: 60f);
            Assert.AreEqual(2f, s.ComputeDelay(1), 1e-4);
        }

        [Test]
        public void ComputeDelay_DoublesEachAttempt_NoJitter()
        {
            var s = NewScheduler(jitter: 0f, baseDelay: 2f, maxDelay: 1000f);
            Assert.AreEqual(2f, s.ComputeDelay(1), 1e-4);
            Assert.AreEqual(4f, s.ComputeDelay(2), 1e-4);
            Assert.AreEqual(8f, s.ComputeDelay(3), 1e-4);
            Assert.AreEqual(16f, s.ComputeDelay(4), 1e-4);
            Assert.AreEqual(32f, s.ComputeDelay(5), 1e-4);
        }

        [Test]
        public void ComputeDelay_CapsAtMaxDelay()
        {
            var s = NewScheduler(jitter: 0f, baseDelay: 2f, maxDelay: 60f);
            Assert.AreEqual(60f, s.ComputeDelay(20), 1e-4);
            Assert.AreEqual(60f, s.ComputeDelay(100), 1e-4);
        }

        [Test]
        public void ComputeDelay_AppliesJitterWithinBounds()
        {
            // rand01 returns 1.0 → jitter coefficient is +1*frac (max positive)
            var sMax = NewScheduler(jitter: 0.5f, baseDelay: 10f, maxDelay: 100f, rand01: _ => 1f);
            // rand01 returns 0.0 → jitter coefficient is -1*frac (max negative)
            var sMin = NewScheduler(jitter: 0.5f, baseDelay: 10f, maxDelay: 100f, rand01: _ => 0f);

            // attempt=1, base=10, jitter ±50% → 5..15
            Assert.AreEqual(15f, sMax.ComputeDelay(1), 1e-4);
            Assert.AreEqual(5f, sMin.ComputeDelay(1), 1e-4);
        }

        [Test]
        public void ComputeDelay_ZeroOrNegativeAttempt_TreatedAsAttemptOne()
        {
            var s = NewScheduler(jitter: 0f, baseDelay: 3f, maxDelay: 60f);
            Assert.AreEqual(3f, s.ComputeDelay(0), 1e-4);
            Assert.AreEqual(3f, s.ComputeDelay(-5), 1e-4);
        }

        [Test]
        public void ComputeDelay_NeverDropsBelowFloor()
        {
            // Even with maximum negative jitter, delay must stay above 0.1
            var s = NewScheduler(jitter: 1f, baseDelay: 0.2f, maxDelay: 1f, rand01: _ => 0f);
            // attempt=1, base=0.2, jitter -100% → 0 → floored at 0.1
            Assert.GreaterOrEqual(s.ComputeDelay(1), 0.1f);
        }

        // -- Lifecycle --------------------------------------------------------

        [Test]
        public void Enable_LoadsImmediately_WhenNotReady()
        {
            int loadCalls = 0;
            bool ready = false;
            var s = new AdLoadScheduler(
                AdFormat.Interstitial,
                onLoaded => { loadCalls++; onLoaded(true); ready = true; },
                () => ready);
            s.Enable();
            Assert.AreEqual(1, loadCalls, "Enable should trigger one load when not ready");
        }

        [Test]
        public void Enable_DoesNotLoad_WhenAlreadyReady()
        {
            int loadCalls = 0;
            var s = new AdLoadScheduler(
                AdFormat.Interstitial,
                onLoaded => { loadCalls++; onLoaded(true); },
                () => true);
            s.Enable();
            Assert.AreEqual(0, loadCalls, "Should skip load when adapter already reports ready");
        }

        [Test]
        public void OnAdConsumed_ResetsAttemptCounter_AndKicksReload()
        {
            int loadCalls = 0;
            bool ready = true;
            var s = new AdLoadScheduler(
                AdFormat.Interstitial,
                onLoaded => { loadCalls++; ready = true; onLoaded(true); },
                () => ready);
            s.Enable();
            Assert.AreEqual(0, loadCalls, "Initial Enable shouldn't load when ready");

            ready = false;
            s.OnAdConsumed();
            Assert.AreEqual(1, loadCalls, "OnAdConsumed should kick a fresh load when not ready");
            Assert.AreEqual(0, s.Attempt);
        }

        [Test]
        public void Disable_StopsRetries_AndResetsAttempt()
        {
            // Use a force-fail adapter so we accumulate attempts.
            int loadCalls = 0;
            var s = new AdLoadScheduler(
                AdFormat.Interstitial,
                onLoaded => { loadCalls++; onLoaded(false); },
                () => false,
                jitterFraction: 0f);
            s.Enable();
            Assert.AreEqual(1, loadCalls);
            Assert.AreEqual(1, s.Attempt);

            s.Disable();
            Assert.AreEqual(0, s.Attempt);
            Assert.IsFalse(s.RetryPending);

            // Tick after disable should never re-load
            s.Tick(Time.unscaledTime + 1000f);
            Assert.AreEqual(1, loadCalls, "No more loads after Disable");
        }

        [Test]
        public void Tick_BeforeRetryDeadline_DoesNotReload()
        {
            int loadCalls = 0;
            var s = new AdLoadScheduler(
                AdFormat.Interstitial,
                onLoaded => { loadCalls++; onLoaded(false); },
                () => false,
                baseDelaySeconds: 5f,
                jitterFraction: 0f);

            s.Enable();
            Assert.AreEqual(1, loadCalls);
            // After Enable in Edit Mode, _nextRetryAtUnscaled = 0 + 5 = 5.
            // Tick at t=4.99 should not trigger a retry.
            s.Tick(4.99f);
            Assert.AreEqual(1, loadCalls, "Tick before deadline must not retry");
        }

        [Test]
        public void Tick_AfterRetryDeadline_TriggersReload()
        {
            int loadCalls = 0;
            var s = new AdLoadScheduler(
                AdFormat.Interstitial,
                onLoaded => { loadCalls++; onLoaded(false); },
                () => false,
                baseDelaySeconds: 5f,
                jitterFraction: 0f);

            s.Enable();
            Assert.AreEqual(1, loadCalls);
            // _nextRetryAtUnscaled was 5; tick past it.
            s.Tick(5.01f);
            Assert.AreEqual(2, loadCalls, "Tick after deadline should retry");
            Assert.AreEqual(2, s.Attempt);
        }

        // -- Helpers ----------------------------------------------------------

        private static AdLoadScheduler NewScheduler(
            float jitter = 0f,
            float baseDelay = 2f,
            float maxDelay = 60f,
            System.Func<float, float> rand01 = null)
        {
            return new AdLoadScheduler(
                AdFormat.Interstitial,
                _ => { },
                () => false,
                baseDelaySeconds: baseDelay,
                maxDelaySeconds: maxDelay,
                jitterFraction: jitter,
                rand01: rand01 ?? (_ => 0.5f));
        }
    }
}
