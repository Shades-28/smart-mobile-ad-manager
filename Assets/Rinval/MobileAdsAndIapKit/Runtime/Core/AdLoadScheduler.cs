using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drives ad loads with exponential backoff retry on failure and auto-preload after consume. One instance per format that needs scheduling (interstitial, rewarded, MREC). All callbacks run on the main thread via MainThreadDispatcher.</summary>
    public class AdLoadScheduler
    {
        public const float DefaultBaseDelaySeconds = 2f;
        public const float DefaultMaxDelaySeconds = 60f;
        public const float DefaultJitterFraction = 0.2f;

        private readonly AdFormat _format;
        private readonly Action<Action<bool>> _loadFn;
        private readonly Func<bool> _isReadyFn;

        private readonly float _baseDelay;
        private readonly float _maxDelay;
        private readonly float _jitterFraction;
        private readonly Func<float, float> _rand01;

        private int _attempt;
        private bool _loadInFlight;
        private bool _enabled;
        private float _nextRetryAtUnscaled = -1f;

        /// <summary>Fires after every attempt with the result. True = success, false = retry scheduled.</summary>
        public event Action<bool> OnAttemptCompleted;

        public int Attempt => _attempt;
        public bool LoadInFlight => _loadInFlight;
        public bool RetryPending => _nextRetryAtUnscaled > 0f;
        public float NextRetryAtUnscaled => _nextRetryAtUnscaled;

        public AdLoadScheduler(
            AdFormat format,
            Action<Action<bool>> loadFn,
            Func<bool> isReadyFn,
            float baseDelaySeconds = DefaultBaseDelaySeconds,
            float maxDelaySeconds = DefaultMaxDelaySeconds,
            float jitterFraction = DefaultJitterFraction,
            Func<float, float> rand01 = null)
        {
            _format = format;
            _loadFn = loadFn ?? throw new ArgumentNullException(nameof(loadFn));
            _isReadyFn = isReadyFn ?? throw new ArgumentNullException(nameof(isReadyFn));
            _baseDelay = Mathf.Max(0.1f, baseDelaySeconds);
            _maxDelay = Mathf.Max(_baseDelay, maxDelaySeconds);
            _jitterFraction = Mathf.Clamp01(jitterFraction);
            _rand01 = rand01 ?? UnityRandom01;
        }

        public void Enable()
        {
            _enabled = true;
            if (!_loadInFlight && !_isReadyFn()) Kick();
        }

        public void Disable()
        {
            _enabled = false;
            _attempt = 0;
            _nextRetryAtUnscaled = -1f;
        }

        /// <summary>Call after a successful Show() to chain in a fresh preload.</summary>
        public void OnAdConsumed()
        {
            _attempt = 0;
            _nextRetryAtUnscaled = -1f;
            if (_enabled) Kick();
        }

        /// <summary>Drives time-based retries. Call once per frame from a MonoBehaviour Update.</summary>
        public void Tick(float unscaledTimeNow)
        {
            if (!_enabled) return;
            if (_loadInFlight) return;
            if (_nextRetryAtUnscaled < 0f) return;
            if (unscaledTimeNow < _nextRetryAtUnscaled) return;
            _nextRetryAtUnscaled = -1f;
            Kick();
        }

        private void Kick()
        {
            if (_isReadyFn())
            {
                _attempt = 0;
                return;
            }
            // Skip if offline - Tick will retry as soon as connectivity returns.
            if (!ConnectivityGuard.IsOnline())
            {
                _attempt++;
                _nextRetryAtUnscaled = SafeUnscaledTime() + ComputeDelay(_attempt);
                AdLogger.Network("AdLoadScheduler", $"{_format} load skipped: offline");
                SafeInvoke(false);
                return;
            }
            _loadInFlight = true;
            AdLogger.Network("AdLoadScheduler", $"{_format} load attempt #{_attempt + 1}");
            // Adapters MUST marshal native-thread callbacks via MainThreadDispatcher before
            // invoking onLoaded; the scheduler trusts that contract and runs synchronously.
            _loadFn(success =>
            {
                _loadInFlight = false;
                if (success)
                {
                    _attempt = 0;
                    SafeInvoke(true);
                    return;
                }
                _attempt++;
                var delay = ComputeDelay(_attempt);
                _nextRetryAtUnscaled = SafeUnscaledTime() + delay;
                AdLogger.Network("AdLoadScheduler",
                    $"{_format} load failed (attempt {_attempt}); retry in {delay:0.00}s");
                SafeInvoke(false);
            });
        }

        private void SafeInvoke(bool success)
        {
            try { OnAttemptCompleted?.Invoke(success); }
            catch (Exception e) { AdLogger.Error($"AdLoadScheduler.OnAttemptCompleted threw: {e}"); }
        }

        private static float SafeUnscaledTime()
        {
            // Time.unscaledTime is fine in Play Mode but may not advance during Edit Mode tests.
            // We still use it as a monotonic stamp; tests can pass an explicit time to Tick().
            return Application.isPlaying ? Time.unscaledTime : 0f;
        }

        public float ComputeDelay(int attempt)
        {
            if (attempt < 1) attempt = 1;
            // 2^(n-1) * base, capped
            double exp = Math.Min(_maxDelay, _baseDelay * Math.Pow(2, attempt - 1));
            float capped = (float)exp;
            float jitter = _jitterFraction <= 0f
                ? 0f
                : capped * _jitterFraction * (_rand01(0f) * 2f - 1f); // [-frac, +frac]
            return Mathf.Clamp(capped + jitter, 0.1f, _maxDelay);
        }

        private static float UnityRandom01(float _) => UnityEngine.Random.value;
    }
}
