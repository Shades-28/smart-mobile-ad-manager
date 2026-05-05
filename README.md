# Mobile Ads & IAP Kit

> Production-grade ads, IAP, and mobile-game essentials for Unity. One package, one API. Wraps AppLovin MAX, Google AdMob, and Unity LevelPlay.

> *Source available for portfolio / code-review purposes. Commercial licensing via Unity Asset Store (publication pending).*

> **Note:** The vendor SDKs (AppLovin MAX, AdMob, LevelPlay) are NOT bundled with this package. Install them separately from each vendor. Use of those SDKs is governed by their own terms (see `Third_Party_Notices.txt` inside the package).

## Features

**Ads** — single-mediator architecture across AppLovin MAX, Google AdMob, and Unity LevelPlay
- Banner, interstitial, rewarded, rewarded interstitial, MREC, app open, native
- Exponential-backoff retry
- Auto-preload after show
- Per-placement frequency caps
- Network-connectivity guard
- Test Lab auto-detection
- Server-side rewarded verification
- Editor simulator with every result code

**IAP** — single API for consumables, non-consumables, subscriptions; receipt validation; restore purchases.

## Tech stack

- Unity 2020.3+
- TextMeshPro
- Vendor SDKs (separate install): AppLovin MAX / Google AdMob / Unity LevelPlay / Unity IAP

## License

See `LICENSE-UNITY.md` for the Asset Store EULA scope. The contents of this GitHub repository are made available for portfolio and code-review purposes; all rights are reserved by Rinval Games.
