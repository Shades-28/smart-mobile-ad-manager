using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public static class TestLabDetector
    {
        private static bool? _cached;

        public static bool IsTestLab()
        {
            if (_cached.HasValue) return _cached.Value;
            _cached = DetectInternal();
            return _cached.Value;
        }

        public static void OverrideForTests(bool? value) => _cached = value;

        private static bool DetectInternal()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var contentResolverClass = new AndroidJavaClass("android.provider.Settings$System"))
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
                {
                    var value = contentResolverClass.CallStatic<string>("getString", resolver, "firebase.test.lab");
                    return value == "true";
                }
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }
    }
}
