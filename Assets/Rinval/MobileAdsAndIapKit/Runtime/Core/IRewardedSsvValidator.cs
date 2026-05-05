using System;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Server-side rewarded video reward verification hook. Implement this to call your backend after a rewarded ad earns reward, before granting the in-game currency. If the validator returns false (or fails), the publisher should NOT grant the reward client-side.</summary>
    public interface IRewardedSsvValidator
    {
        /// <param name="placement">Placement key passed to ShowRewarded.</param>
        /// <param name="onResult">Invoke with true to grant reward, false to deny.</param>
        void Validate(string placement, Action<bool> onResult);
    }
}
