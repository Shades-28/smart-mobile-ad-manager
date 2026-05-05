# API Reference

All public types live in namespace `Rinval.MobileAdsAndIapKit`.

## `AdManager` (static)

| Member | Description |
|-|-|
| `Initialize(AdManagerConfig, IConfigSource = null)` | Boots the system. Call once at startup |
| `Shutdown()` | Tears everything down; resets all state |
| `IsInitialized` | True when an adapter is wired and ready |
| `ActiveMediator` / `ActiveMediatorName` | What's currently active |
| `Config` / `ConfigSource` | The config in use |
| `SetCurrentStage(int)` | Pass your game's level/stage for stage-gating interstitials |
| `LoadBanner(BannerAnchor)` / `ShowBanner()` / `HideBanner()` / `DestroyBanner()` | Banner lifecycle |
| `LoadInterstitial()` / `IsInterstitialReady()` / `ShowInterstitial(placement, callback)` | Interstitial lifecycle |
| `CanShowInterstitial(out reason)` | Manual gate check (interval, stage, window cap) |
| `LoadRewarded()` / `IsRewardedReady()` / `ShowRewarded(placement, callback)` | Rewarded lifecycle |
| `LoadMrec()` / `IsMrecReady()` / `ShowMrec()` / `HideMrec()` | MREC lifecycle |
| `RegisterAdapter(Func<INetworkAdapter>)` | Mediator adapters call this in `[RuntimeInitializeOnLoadMethod]` - you don't usually call it |
| `event RevenuePaid` | Fires for every paid impression with structured `AdRevenueInfo` |
| `event InterstitialClosed` / `RewardedClosed` | Fires when an ad closes, with the result code |

## `AppOpenAdManager` (static)

| `Initialize(config, source, adapter)` / `Shutdown()` |
| `Load()` / `IsReady()` / `TryShow(callback)` |
| `CanShow(out reason)` |
| `event AppOpenClosed` |

## `ConsentManager` (static)

| `RequestAll(privacyUrl, onComplete)` |
| `Grant()` / `Deny()` / `Reset()` |
| `GdprStatus` / `AttStatus` / `AdsAllowed` |
| `event ConsentChanged` |

## Enums

- `AdResultCode` - None, Rewarded, Shown, Closed, Failed, Cancelled, LoadFailed, TimedOut, NotReady, Disabled
- `AdFormat` - Banner, Interstitial, Rewarded, MediumRectangle, AppOpen
- `BannerAnchor` - Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight
- `MediatorKind` - None, Editor, AppLovinMax, GoogleAdMob, UnityLevelPlay
- `ConsentStatus` - Unknown, NotRequired, Required, Obtained, Denied
- `AttStatus` - NotDetermined, Restricted, Denied, Authorized, Unsupported

## Structs

- `AdRevenueInfo` - Format, AdUnitId, Placement, NetworkName, Currency, Amount
- `AdLoadFailure` - Format, NetworkName, ErrorCode, Message

## Logging

`AdLogger` provides verbose-gated logging with per-tag colors. Call `AdLogger.SetVerbose(false)` to silence everything except errors.
