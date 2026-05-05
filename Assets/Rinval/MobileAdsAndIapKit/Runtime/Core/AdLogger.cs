using UnityEngine;
using Object = UnityEngine.Object;

namespace Rinval.MobileAdsAndIapKit
{
    public static class AdLogger
    {
        private const string PrefixLog       = "<color=#FFFFFF>[ADS]</color>";
        private const string PrefixWarning   = "<color=#FFD23F>[ADS:WARN]</color>";
        private const string PrefixError     = "<color=#FF4D4D>[ADS:ERROR]</color>";
        private const string PrefixSuccess   = "<color=#7CFF6B>[ADS:OK]</color>";
        private const string DefaultTagColor = "#7AC5FF";

        private static bool _verbose = true;

        public static bool VerboseEnabled => _verbose;

        public static void SetVerbose(bool value)
        {
            _verbose = value;
        }

        public static void Log(string message, Object context = null)
        {
            if (!_verbose) return;
            Debug.Log($"{PrefixLog} {message}", context);
        }

        public static void Warn(string message, Object context = null)
        {
            if (!_verbose) return;
            Debug.LogWarning($"{PrefixWarning} {message}", context);
        }

        public static void Error(string message, Object context = null)
        {
            Debug.LogError($"{PrefixError} {message}", context);
        }

        public static void Success(string message, Object context = null)
        {
            if (!_verbose) return;
            Debug.Log($"{PrefixSuccess} {message}", context);
        }

        public static void Tag(string tag, string message, Object context = null, string colorHex = DefaultTagColor)
        {
            if (!_verbose) return;
            Debug.Log($"<color={colorHex}>[ADS:{tag.ToUpperInvariant()}]</color> {message}", context);
        }

        public static void Network(string networkName, string message, Object context = null)
        {
            if (!_verbose) return;
            Tag(networkName, message, context, "#A8E6FF");
        }

        public static void Lifecycle(string method, string detail = null, Object context = null)
        {
            if (!_verbose) return;
            string body = string.IsNullOrEmpty(detail) ? method : $"{method} | {detail}";
            Tag("LIFECYCLE", body, context, "#C9A8FF");
        }
    }
}
