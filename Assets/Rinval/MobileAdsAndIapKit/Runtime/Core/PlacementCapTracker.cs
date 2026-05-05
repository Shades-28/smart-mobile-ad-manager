using System.Collections.Generic;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Tracks per-placement show counts and last-shown time. One instance shared by AdManager across interstitial / rewarded / mrec shows. Time source is injectable for testability.</summary>
    public class PlacementCapTracker
    {
        private struct State
        {
            public float LastShownUnscaled;
            public float WindowStartUnscaled;
            public int CountInWindow;
        }

        private readonly Dictionary<string, State> _state = new Dictionary<string, State>();
        private readonly Dictionary<string, PlacementRule> _rules = new Dictionary<string, PlacementRule>();

        public void SetRules(IList<PlacementRule> rules)
        {
            _rules.Clear();
            if (rules == null) return;
            foreach (var r in rules)
            {
                if (r == null || string.IsNullOrEmpty(r.Placement)) continue;
                _rules[r.Placement] = r;
            }
        }

        public bool TryGetRule(string placement, out PlacementRule rule)
        {
            rule = null;
            return !string.IsNullOrEmpty(placement) && _rules.TryGetValue(placement, out rule);
        }

        public bool CanShow(string placement, float nowUnscaled, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrEmpty(placement)) return true;
            if (!_rules.TryGetValue(placement, out var rule)) return true;

            _state.TryGetValue(placement, out var st);

            if (nowUnscaled - st.LastShownUnscaled < rule.MinIntervalSeconds && st.LastShownUnscaled > 0f)
            {
                reason = $"placement '{placement}' interval cap: {nowUnscaled - st.LastShownUnscaled:0.0}s < {rule.MinIntervalSeconds}s";
                return false;
            }

            if (nowUnscaled - st.WindowStartUnscaled > rule.WindowSeconds)
            {
                st.WindowStartUnscaled = nowUnscaled;
                st.CountInWindow = 0;
                _state[placement] = st;
            }
            if (st.CountInWindow >= rule.MaxPerWindow)
            {
                reason = $"placement '{placement}' window cap: {st.CountInWindow} >= {rule.MaxPerWindow}";
                return false;
            }
            return true;
        }

        public void RecordShown(string placement, float nowUnscaled)
        {
            if (string.IsNullOrEmpty(placement)) return;
            _state.TryGetValue(placement, out var st);
            if (nowUnscaled - st.WindowStartUnscaled > GetWindowSeconds(placement) || st.WindowStartUnscaled == 0f)
            {
                st.WindowStartUnscaled = nowUnscaled;
                st.CountInWindow = 0;
            }
            st.LastShownUnscaled = nowUnscaled;
            st.CountInWindow++;
            _state[placement] = st;
        }

        public void Reset()
        {
            _state.Clear();
        }

        private int GetWindowSeconds(string placement)
        {
            return _rules.TryGetValue(placement, out var r) ? r.WindowSeconds : int.MaxValue;
        }
    }
}
