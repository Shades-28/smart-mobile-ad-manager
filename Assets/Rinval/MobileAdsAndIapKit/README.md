# Mobile Ads & IAP Kit

Production-grade ads, IAP, and mobile-game essentials for Unity. One package, one API.

> **This package wraps AppLovin MAX, Google AdMob, and Unity LevelPlay. The SDKs are not bundled - install them separately from each vendor.** Use of those SDKs is governed by their own terms (see `Third_Party_Notices.txt`).

## What's inside

**Ads** - single-mediator architecture across AppLovin MAX, Google AdMob, and Unity LevelPlay. Banner, interstitial, rewarded, rewarded interstitial, MREC, app open, and native formats. Exponential-backoff retry, auto-preload after show, per-placement frequency caps, network-connectivity guard, Test Lab auto-detection, server-side rewarded verification, editor simulator with every result code.

**In-App Purchases** - Unity IAP V5 wrapper. Consumables, non-consumables, subscriptions. Local-currency prices, restore purchases, receipt-validation hook, refund detection, subscription activate / deactivate events.

**Remove Ads** - first-class API wired to a `remove_ads` non-consumable. All interruption formats auto-skip when active.

**Privacy & Compliance** - GDPR consent, Google UMP SDK integration (optional define), App Tracking Transparency for iOS, drop-in consent popup prefab generator.

**Mobile essentials** - local + push notifications (FCM reference adapter), Android in-app update, rate-game prompt, internet-connectivity watcher, cross-promotion module.

**A/B Testing** - sticky-bucketed placement variants with per-variant cap overrides.

**Designer-friendly** - every behavior is a ScriptableObject. 12 drop-on-button no-code components.

**Editor tooling** - pre-flight setup wizard, live diagnostics, health check, placement graph, recipe builder, cheat sheet, marketing screenshot generator, one-click demo scene.

## Quick start

**Path A - no code (recommended for indie / hyper-casual):**

1. Add an empty GameObject. Add Component → Mobile Ads & IAP Kit → **Mobile Kit Bootstrap**.
2. Drag your `AdManagerConfig` (and optional `IapProductCatalog`) into the inspector.
3. Rinval → Mobile Ads & IAP Kit → **Create Consent Popup Prefab**, drop it into the scene.
4. On any UI Button, add a kit component (e.g. `IapBuyButton`, `RewardedAdButton`), set the ID, wire UnityEvents.
5. Press Play.

**Path B - code:**

```csharp
using Rinval.MobileAdsAndIapKit;

await MobileKit.InitializeAsync(new KitConfig {
    AdConfig    = myAdManagerConfig,
    IapCatalog  = myIapCatalog,        // optional
    UseAppOpen  = true,
});

var result = await AdManager.ShowRewardedAsync("double_coins");
if (result.WasRewarded) GiveCoins(20);

var purchase = await IapManager.PurchaseAsync("coins_100");
if (purchase.Successful) GiveCoins(100);
```

Full setup: see `Documentation/QuickStart.md`. In-editor reference: **Rinval → Mobile Ads & IAP Kit → Cheat Sheet**.

## Mediator setup

The kit ships zero SDK binaries. Pick one mediator, install its SDK separately, and the matching adapter compiles automatically:

| Mediator | Install via |
|-|-|
| AppLovin MAX | AppLovin dashboard → MAX Integration Manager |
| Google AdMob | Package Manager → `com.google.ads.mobile` |
| Unity LevelPlay | Package Manager → `com.unity.services.levelplay` |

Then **Rinval → Mobile Ads & IAP Kit → Ad Manager Dashboard** → click **Use AppLovin / AdMob / LevelPlay** to set the matching scripting define.

For per-mediator setup details see `Documentation/MediatorSetup.md`.

## Compatibility

- Unity 2021.3 LTS or newer (also tested on Unity 6)
- Built-in, URP, HDRP
- Android + iOS
- IL2CPP + Mono

## Privacy & data

The kit itself collects, stores, and transmits **no user data**. The wrapped SDKs (AdMob, AppLovin, LevelPlay, Firebase, Google UMP, Unity IAP) handle their own data collection under each vendor's privacy terms - see `Third_Party_Notices.txt`.

You (the publisher) are responsible for:

- Surfacing a Google-certified Consent Management Platform to EEA / UK users (the kit's UMP integration handles this; install via the `RINVAL_UMP` scripting define + Google Mobile Ads Unity plugin).
- Providing your own privacy policy URL on your store listing covering the SDKs you ship.
- Configuring App Tracking Transparency on iOS.

## Support

- Cheat Sheet (in-editor): **Rinval → Mobile Ads & IAP Kit → Cheat Sheet**
- Health Check: **Rinval → Mobile Ads & IAP Kit → Run Health Check**
- Live Diagnostics: **Rinval → Mobile Ads & IAP Kit → Live Diagnostics**

## License

See `LICENSE.md`.

---

Mobile Ads & IAP Kit by **Rinval Games**.
