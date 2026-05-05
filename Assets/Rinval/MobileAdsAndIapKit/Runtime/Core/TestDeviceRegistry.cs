using System.Collections.Generic;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Central registry of test device IDs. Adapters consult this on init and apply IDs to each underlying SDK so all networks see the same set without per-network duplication.</summary>
    public static class TestDeviceRegistry
    {
        private static readonly List<string> _ids = new List<string>();

        public static IReadOnlyList<string> Ids => _ids;

        public static void Add(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return;
            if (_ids.Contains(deviceId)) return;
            _ids.Add(deviceId);
        }

        public static void AddRange(IEnumerable<string> deviceIds)
        {
            if (deviceIds == null) return;
            foreach (var id in deviceIds) Add(id);
        }

        public static void Clear() => _ids.Clear();

        public static bool Contains(string deviceId) => !string.IsNullOrEmpty(deviceId) && _ids.Contains(deviceId);
    }
}
