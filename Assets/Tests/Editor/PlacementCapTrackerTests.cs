using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class PlacementCapTrackerTests
    {
        private PlacementCapTracker _tracker;

        [SetUp]
        public void Setup() => _tracker = new PlacementCapTracker();

        [Test]
        public void NoRules_AlwaysAllowsShow()
        {
            Assert.IsTrue(_tracker.CanShow("anything", 0f, out _));
            Assert.IsTrue(_tracker.CanShow(null, 0f, out _));
            Assert.IsTrue(_tracker.CanShow("", 0f, out _));
        }

        [Test]
        public void Rule_AllowsFirstShow()
        {
            _tracker.SetRules(new[] { new PlacementRule("level_complete", 30, 3, 300) });
            Assert.IsTrue(_tracker.CanShow("level_complete", 0f, out _));
        }

        [Test]
        public void Rule_BlocksSecondShowWithinInterval()
        {
            _tracker.SetRules(new[] { new PlacementRule("level_complete", 30, 100, 300) });
            _tracker.RecordShown("level_complete", 0f);
            Assert.IsFalse(_tracker.CanShow("level_complete", 5f, out var reason));
            StringAssert.Contains("interval cap", reason);
        }

        [Test]
        public void Rule_AllowsShowAfterInterval()
        {
            _tracker.SetRules(new[] { new PlacementRule("level_complete", 30, 100, 300) });
            _tracker.RecordShown("level_complete", 0f);
            Assert.IsTrue(_tracker.CanShow("level_complete", 31f, out _));
        }

        [Test]
        public void Rule_BlocksAfterMaxPerWindow()
        {
            _tracker.SetRules(new[] { new PlacementRule("shop", 0, 2, 300) });
            _tracker.RecordShown("shop", 0f);
            _tracker.RecordShown("shop", 1f);
            Assert.IsFalse(_tracker.CanShow("shop", 2f, out var reason));
            StringAssert.Contains("window cap", reason);
        }

        [Test]
        public void Rule_AllowsShowAfterWindowExpires()
        {
            _tracker.SetRules(new[] { new PlacementRule("shop", 0, 2, 60) });
            _tracker.RecordShown("shop", 0f);
            _tracker.RecordShown("shop", 1f);
            Assert.IsFalse(_tracker.CanShow("shop", 2f, out _));
            Assert.IsTrue(_tracker.CanShow("shop", 100f, out _));
        }

        [Test]
        public void Rule_OtherPlacementUnaffected()
        {
            _tracker.SetRules(new[] {
                new PlacementRule("a", 30, 1, 300),
                new PlacementRule("b", 30, 1, 300)
            });
            _tracker.RecordShown("a", 0f);
            Assert.IsFalse(_tracker.CanShow("a", 5f, out _));
            Assert.IsTrue(_tracker.CanShow("b", 5f, out _));
        }

        [Test]
        public void UnknownPlacement_NotGated()
        {
            _tracker.SetRules(new[] { new PlacementRule("known", 30, 1, 300) });
            _tracker.RecordShown("known", 0f);
            // Different placement key has no rule → always allowed
            Assert.IsTrue(_tracker.CanShow("unknown", 5f, out _));
        }

        [Test]
        public void Reset_ClearsHistory()
        {
            _tracker.SetRules(new[] { new PlacementRule("p", 30, 1, 300) });
            _tracker.RecordShown("p", 0f);
            Assert.IsFalse(_tracker.CanShow("p", 5f, out _));
            _tracker.Reset();
            Assert.IsTrue(_tracker.CanShow("p", 5f, out _));
        }
    }
}
