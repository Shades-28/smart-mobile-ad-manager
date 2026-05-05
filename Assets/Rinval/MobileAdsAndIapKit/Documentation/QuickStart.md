# Quick Start - Mobile Ads & IAP Kit

Pick your path. Both work; both are supported.

---

## Path A: No code (60 seconds)

1. Create an empty GameObject. Name it `MobileKit`.
2. **Add Component → Mobile Ads & IAP Kit → Mobile Kit Bootstrap**.
3. Drag your `AdManagerConfig` asset into the inspector. (Don't have one? Right-click in Project → **Create → Rinval → Mobile Ads & IAP → Ad Manager Config**.)
4. Optional: drag an `IapProductCatalog`, tick `Use App Open`, etc.
5. **Rinval → Mobile Ads & IAP Kit → Create Consent Popup Prefab**, drop the resulting prefab into the scene.
6. On any UI Button: **Add Component → Mobile Ads & IAP Kit → IAP Buy Button** / **Rewarded Ad Button** / etc. Type the product ID or placement key. Wire `OnPurchaseSuccess` / `OnRewarded` UnityEvents.
7. Press **Play**. Done. Zero C# written.

The components do everything. You assign IDs in the inspector and react via UnityEvents - same workflow as Unity Events on Buttons or DOTween's drop-in components.

---

## Path B: Code (5 lines)

```csharp
using Rinval.MobileAdsAndIapKit;

async void Start() {
    await MobileKit.InitializeAsync(new KitConfig {
        AdConfig    = Resources.Load<AdManagerConfig>("AdManagerConfig"),
        IapCatalog  = Resources.Load<IapProductCatalog>("IapProductCatalog"), // optional
        UseAppOpen  = true,
    });
}
```

That's it. The kit is now running. Show ads / sell IAP from anywhere:

```csharp
// Rewarded
var result = await AdManager.ShowRewardedAsync("double_coins");
if (result.WasRewarded) GiveCoins(20);

// Interstitial (returns immediately if gated by frequency caps)
await AdManager.ShowInterstitialAsync("level_complete");

// Banner
AdManager.LoadBanner(BannerAnchor.Bottom, BannerSize.Adaptive);
AdManager.ShowBanner();

// IAP
var purchase = await IapManager.PurchaseAsync("coins_100");
if (purchase.Successful) GiveCoins(100);

// Restore (Apple required for any non-consumable / subscription)
await IapManager.RestorePurchasesAsync();
```

---

## Run the demo (zero setup)

**Rinval → Mobile Ads & IAP Kit → One-Click Demo Setup** builds a full demo scene with every API wired to a button, then opens it. Press Play and tap around - banners, interstitials, rewarded, MREC, all simulated in Editor with full result-code coverage. Use this scene to record your store-listing video.

---

## Pick your mediator

1. Open `AdManagerConfig` in the inspector.
2. Pick **Mediator**: AppLovin MAX, Google AdMob, or Unity LevelPlay.
3. Paste your **Ad Unit IDs** for each platform.
4. **Rinval → Mobile Ads & IAP Kit → Ad Manager Dashboard** → click **Use AppLovin / AdMob / LevelPlay** to set the matching scripting define.
5. Install your chosen mediator's SDK separately (we don't bundle them):

| Mediator | How to install |
|-|-|
| AppLovin MAX | AppLovin dashboard → MAX Integration Manager |
| Google AdMob | Package Manager → `com.google.ads.mobile` |
| Unity LevelPlay | Package Manager → `com.unity.services.levelplay` |

The matching adapter compiles only when both the SDK is installed and the `AD_USE_*` define is set.

---

## Track revenue

```csharp
AdManager.RevenuePaid += info => {
    Debug.Log($"{info.NetworkName} {info.Format} {info.Placement}: ${info.Amount}");
    // Forward to your analytics SDK of choice
};
```

> Built-in analytics integrations (Firebase Analytics, GameAnalytics, Adjust, AppsFlyer) ship in the **Pro** edition.

---

## Consent (GDPR / ATT / CCPA)

The pre-built popup handles the standard flow. Drag the prefab into your scene and either:

```csharp
// Manual - show on first launch
FindObjectOfType<ConsentPopupController>().ShowIfNeeded();
```

Or just check **Show Consent On First Launch** on `MobileKitBootstrap`. The popup writes to `ConsentManager` automatically.

---

## Components reference

Add via **Add Component → Mobile Ads & IAP Kit → ...**

| Component | Drop on | What it does |
|-|-|-|
| MobileKitBootstrap | empty GameObject | Initializes the kit; assign config + catalog in inspector |
| IapBuyButton | UI Button | Buys product on click; UnityEvents for outcome |
| RestorePurchasesButton | UI Button | Apple-required restore |
| RewardedAdButton | UI Button | Watch-ad-for-reward |
| InterstitialAdButton | UI Button | Show interstitial then continue |
| BannerToggleButton | UI Button | Show / Hide / Toggle banner |
| RateGameButton | UI Button | StoreKit / Play Review prompt |
| ConsentManageButton | UI Button | Re-open consent popup |
| CrossPromoButton | UI Button | Shows one of your other apps |
| AdEventListener | any GameObject | UnityEvent slots for every ad/IAP/connectivity event |

---

## Troubleshooting

| Symptom | Fix |
|-|-|
| Compile errors after switching mediator | Restart Unity; ensure only one `AD_USE_*` define is active |
| Ads disabled at runtime, no error | Check `TestLabDetector.IsTestLab()` and `AdsEnabled` in config |
| `AdManager not initialized` errors | Use `MobileKitBootstrap` in your first scene |
| Editor shows simulated ads instead of real | Expected. Real mediator only runs on device |
| "Why didn't my ad show?" | Read `AdManager.LastShowVerdict.Reason` - explicit reason for the last gate |

---

## Cheat sheet

**Rinval → Mobile Ads & IAP Kit → Cheat Sheet** opens an in-editor reference window with the most common calls.

---

## Next

- `API_Reference.md` - full type & method index
- `MediatorSetup.md` - per-mediator dashboard + scripting define setup
