# Changelog

All notable changes to **Mobile Ads & IAP Kit** are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - 2026-04-30

Initial public release.

### Ads
- Single-mediator architecture (AppLovin MAX / Google AdMob / Unity LevelPlay)
- Banner, interstitial, rewarded, rewarded interstitial, MREC, app open, native
- Adaptive banner sizes
- Exponential-backoff retry with auto-preload after consume
- Per-placement and global frequency caps
- Network-connectivity guard
- Test Lab auto-detection
- Test device registry
- Server-side reward verification (SSV) hook
- Editor simulator covering every result code
- Runtime debug overlay (dev builds)

### IAP
- Unity IAP V5 wrapper
- Consumable / non-consumable / subscription support
- Local-currency price display
- Restore purchases
- Receipt-validation hook
- Refund detection
- Subscription activated / deactivated events

### Remove Ads
- First-class API auto-wired to a `remove_ads` non-consumable
- All interruption ad types auto-skip when active

### Privacy & Compliance
- GDPR consent flow
- Google UMP SDK integration (behind `RINVAL_UMP`)
- App Tracking Transparency (ATT) for iOS
- Drop-in consent popup prefab generator

### Notifications
- Local notifications via `NotificationDefinition` ScriptableObject
- Push notifications via pluggable adapter (FCM reference behind `RINVAL_FCM`)

### Mobile essentials
- Cross-promotion module
- Rate-game prompt (StoreKit / Play Review)
- Android In-App Update wrapper
- Connectivity watcher

### A/B Testing
- Sticky-bucketed placement variants with per-variant cap overrides

### Designer-friendly
- 9 ScriptableObject configuration types
- 12 drop-on-button no-code components
- Async API surface (Task-based)

### Editor tooling
- Pre-flight Setup Wizard
- Live Diagnostics Window
- Run Health Check
- Placement Graph Window
- Recipe builder (Hyper-Casual / Match-3 / Idle)
- Cheat Sheet window
- Marketing screenshot generator
- One-click demo scene generator
- Ad Manager Dashboard
- Mediator Setup Wizard
- Consent popup prefab builder
- Auto-config AndroidManifest.xml + iOS Info.plist on build

### Compatibility
- Unity 2021.3 LTS or newer
- Built-in / URP / HDRP
- Android + iOS
- IL2CPP + Mono
