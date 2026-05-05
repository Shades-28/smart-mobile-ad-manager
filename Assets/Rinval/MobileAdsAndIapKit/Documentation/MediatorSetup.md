# Mediator Setup

The kit ships zero SDK binaries. Pick **one** mediator, install its SDK separately, set the matching scripting define, and the corresponding adapter compiles automatically.

> Use the **Pre-flight Setup Wizard** (Rinval → Mobile Ads & IAP Kit → Pre-flight Setup Wizard) to validate every step below in one click after setup.

---

## AppLovin MAX

**Dashboard**

1. Create an account at https://applovin.com.
2. Create your app, grab the **SDK Key**.
3. Create ad units: Banner / MREC / Interstitial / Rewarded / App Open.
4. Paste each ad-unit ID into your `AdManagerConfig` (Android + iOS rows).

**Unity**

1. Download the MAX Integration Manager `.unitypackage` from the AppLovin dashboard and import.
2. Open **AppLovin → Integration Manager**, install the networks you want (Meta, Mintegral, Pangle, etc.).
3. Paste your SDK Key into AppLovin's settings asset.
4. **Rinval → Mobile Ads & IAP Kit → Ad Manager Dashboard → Use AppLovin** (sets `AD_USE_APPLOVIN`).

**Build**

- Android: Gradle build; MAX handles its own dependencies.
- iOS: CocoaPods runs automatically. Ensure `Info.plist` has `NSUserTrackingUsageDescription` for ATT.

---

## Google AdMob

**Dashboard**

1. Create an app at https://admob.google.com.
2. Add ad units (Banner / Interstitial / Rewarded / Rewarded Interstitial / MREC / App Open / Native).
3. Paste the ad-unit IDs into your `AdManagerConfig`.
4. Add the **Application ID** to Google Mobile Ads settings.

**Unity**

1. Package Manager → install `com.google.ads.mobile` (Google Mobile Ads Unity Plugin 9.x+).
2. Configure mediation networks via **Assets → Google Mobile Ads → Settings**.
3. **Rinval → Mobile Ads & IAP Kit → Ad Manager Dashboard → Use AdMob** (sets `AD_USE_ADMOB`).

**Build**

- AndroidManifest needs `<meta-data android:name="com.google.android.gms.ads.APPLICATION_ID" .../>` - the **Pre-flight Setup Wizard** adds this for you.
- iOS Info.plist needs `GADApplicationIdentifier` and SKAdNetwork items - the kit's iOS post-build hook patches this automatically.

---

## Unity LevelPlay (formerly IronSource)

**Dashboard**

1. Create an app at https://platform.ironsrc.com.
2. Configure your mediation waterfall in the LevelPlay dashboard.
3. Note the **App Key** (Android + iOS).

**Unity**

1. Package Manager → install `com.unity.services.levelplay`.
2. Open **LevelPlay → Integration Manager**, paste your App Key.
3. **Rinval → Mobile Ads & IAP Kit → Ad Manager Dashboard → Use LevelPlay** (sets `AD_USE_LEVELPLAY`).

**Build**

- Android: gradle templates auto-merge LevelPlay dependencies.
- iOS: CocoaPods auto-installs. Ensure the ATT prompt is wired up.

---

## Switching mediators

Pick a different mediator at any time via **Ad Manager Dashboard**. Only one `AD_USE_*` define is active at a time; the kit recompiles cleanly with just the new mediator's adapter.

## Troubleshooting

If something doesn't work after setup:

- **Rinval → Mobile Ads & IAP Kit → Run Health Check** - pass / fail report on every subsystem.
- **Rinval → Mobile Ads & IAP Kit → Live Diagnostics** - real-time event timeline in Play mode.
- `AdManager.LastShowVerdict` - explicit reason the most recent show attempt was blocked.
