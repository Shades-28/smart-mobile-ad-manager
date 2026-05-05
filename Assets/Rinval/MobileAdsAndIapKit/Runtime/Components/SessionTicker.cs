using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Auto-ticks RateGameManager.RecordSessionEnd on app quit / pause so devs don't have to remember. Spawned by MobileKitBootstrap when present in the scene.</summary>
    [AddComponentMenu("")]
    public class SessionTicker : MonoBehaviour
    {
        public static void EnsureRunning()
        {
            if (FindObjectOfType<SessionTicker>() != null) return;
            var go = new GameObject("Rinval.MobileAdsIap.SessionTicker");
            DontDestroyOnLoad(go);
            go.AddComponent<SessionTicker>();
        }

        private void OnApplicationQuit() => RateGameManager.RecordSessionEnd();

        private void OnApplicationPause(bool paused)
        {
            if (paused) RateGameManager.RecordSessionEnd();
        }
    }
}
