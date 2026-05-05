using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    public static class OneClickDemoSetup
    {
        private const string DemoFolder = "Assets/Rinval/MobileAdsAndIapKit/Demo";
        private const string ScenePath = DemoFolder + "/RinvalAdsDemo.unity";
        private const string ConfigPath = DemoFolder + "/AdManagerConfig.asset";

        // iPhone X / 11 / 12 / 13 / 14 portrait - modern reference resolution.
        private static readonly Vector2 ReferenceResolution = new Vector2(1125f, 2436f);
        private const float SafeTop = 120f;
        private const float SafeBottom = 90f;
        private const float SidePad = 36f;

        // Brand palette
        private static readonly Color BgTop = new Color(0.08f, 0.10f, 0.16f);
        private static readonly Color BgPanel = new Color(0.13f, 0.16f, 0.22f, 0.95f);
        private static readonly Color BgPanelDark = new Color(0.06f, 0.08f, 0.12f, 0.95f);
        private static readonly Color Accent = new Color(0.20f, 0.55f, 0.95f);
        private static readonly Color AccentMuted = new Color(0.30f, 0.35f, 0.45f);
        private static readonly Color TextWhite = new Color(0.95f, 0.97f, 1f);
        private static readonly Color TextDim = new Color(0.65f, 0.72f, 0.85f);

        [MenuItem("Rinval/Mobile Ads & IAP Kit/One-Click Demo Setup", priority = 1)]
        public static void Run()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode",
                    "Exit Play Mode before running One-Click Demo Setup.", "OK");
                return;
            }

            Directory.CreateDirectory(DemoFolder);

            var config = EnsureConfig();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var canvas = BuildCanvas();
            BuildUi(canvas, config);
            EnsureEventSystem();

            var cam = Camera.main;
            if (cam != null) cam.backgroundColor = BgTop;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            AdManagerDashboard.Open();

            Debug.Log("[Rinval.MobileAdsIap] Demo scene built at " + ScenePath);
            EditorUtility.DisplayDialog("Demo ready",
                "Demo scene built and opened at:\n" + ScenePath +
                "\n\nPress Play. Use the on-screen buttons or the Ad Manager Dashboard to test every code path.",
                "Got it");
        }

        // - Config -
        private static AdManagerConfig EnsureConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AdManagerConfig>(ConfigPath);
            if (existing != null) return existing;
            var cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            cfg.SetTestMode(true);
            cfg.SetVerbose(true);
            AssetDatabase.CreateAsset(cfg, ConfigPath);
            AssetDatabase.SaveAssets();
            return cfg;
        }

        // - Canvas -
        private static Canvas BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // match width - keeps button width predictable on tablets

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // - UI -
        private static void BuildUi(Canvas canvas, AdManagerConfig config)
        {
            // Backdrop fills the canvas.
            var backdrop = NewImage(canvas.transform, "Backdrop", BgTop);
            Stretch(backdrop);

            // Root vertical layout aligned within safe area.
            // Values calibrated from the user's hand-tweaked RinvalAdsDemo.unity scene:
            //   Root spacing 78, anchoredPos.y -15.
            var root = NewObject(canvas.transform, "Root");
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = new Vector2(SidePad, SafeBottom);
            rootRt.offsetMax = new Vector2(-SidePad, -SafeTop);
            rootRt.anchoredPosition = new Vector2(0f, -15f);
            var vroot = root.AddComponent<VerticalLayoutGroup>();
            vroot.padding = new RectOffset(0, 0, 0, 0);
            vroot.spacing = 78f;
            vroot.childAlignment = TextAnchor.UpperCenter;
            vroot.childControlWidth = true;
            vroot.childControlHeight = false;
            vroot.childForceExpandWidth = true;
            vroot.childForceExpandHeight = false;

            // Header
            var header = BuildHeader(root.transform);
            SetLayoutHeight(header, 120);
            ApplyTopAnchor(header);

            // Status row (Status | Counters) - two columns
            var status = BuildStatusRow(root.transform, out var statusText, out var countersText);
            SetLayoutHeight(status, 200);
            ApplyTopAnchor(status);

            // Button columns row (Banners | Interstitial+Rewarded | MREC+Consent+Lifecycle)
            var btnRow = BuildButtonRow(root.transform, out var btnRefs);
            SetLayoutHeight(btnRow, 1180);
            ApplyTopAnchor(btnRow);

            // Log
            var log = BuildLogPanel(root.transform, out var logText);
            SetLayoutHeight(log, 480);
            ApplyTopAnchor(log);

            // Wire DemoController
            var ctrl = NewObject(canvas.transform.parent, "DemoController");
            var dc = ctrl.AddComponent<DemoController>();
            dc.config = config;
            dc.bannerLoadTopBtn = btnRefs.bannerLoadTop;
            dc.bannerLoadBottomBtn = btnRefs.bannerLoadBot;
            dc.bannerShowBtn = btnRefs.bannerShow;
            dc.bannerHideBtn = btnRefs.bannerHide;
            dc.bannerDestroyBtn = btnRefs.bannerDestroy;
            dc.interstitialLoadBtn = btnRefs.interLoad;
            dc.interstitialShowClosedBtn = btnRefs.interShowClosed;
            dc.interstitialShowFailBtn = btnRefs.interShowFail;
            dc.rewardedLoadBtn = btnRefs.rewardLoad;
            dc.rewardedShowRewardBtn = btnRefs.rewardShowR;
            dc.rewardedShowCancelBtn = btnRefs.rewardShowC;
            dc.mrecLoadBtn = btnRefs.mrecLoad;
            dc.mrecShowBtn = btnRefs.mrecShow;
            dc.mrecHideBtn = btnRefs.mrecHide;
            dc.consentGrantBtn = btnRefs.consentGrant;
            dc.consentDenyBtn = btnRefs.consentDeny;
            dc.initBtn = btnRefs.init;
            dc.shutdownBtn = btnRefs.shutdown;
            dc.statusText = statusText;
            dc.logText = logText;
        }

        private static GameObject BuildHeader(Transform parent)
        {
            var header = NewImage(parent, "Header", BgPanel).gameObject;
            AddRoundedLook(header);
            var v = header.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(24, 24, 16, 16);
            v.spacing = 4;
            v.childAlignment = TextAnchor.MiddleCenter;
            v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;

            var title = NewText(header.transform, "Title",
                "Mobile Ads & IAP Kit", 50, FontStyle.Bold, TextAnchor.MiddleCenter, TextWhite);
            SetLayoutHeight(title.gameObject, 60);
            var subtitle = NewText(header.transform, "Subtitle",
                "by Rinval Games  ·  Demo Scene", 26, FontStyle.Normal, TextAnchor.MiddleCenter, TextDim);
            SetLayoutHeight(subtitle.gameObject, 32);
            return header;
        }

        private static GameObject BuildStatusRow(Transform parent, out Text statusText, out Text countersText)
        {
            var row = NewObject(parent, "StatusRow");
            row.AddComponent<RectTransform>();
            var h = row.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 18f;
            h.childAlignment = TextAnchor.UpperLeft;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            // Note: the user's saved scene reordered children visually; we still emit
            // [Status, Counters] left-to-right, which the layout group renders correctly.

            var statusBg = NewImage(row.transform, "Status", BgPanel).gameObject;
            AddRoundedLook(statusBg);
            var sv = statusBg.AddComponent<VerticalLayoutGroup>();
            sv.padding = new RectOffset(20, 20, 14, 14);
            sv.spacing = 6;
            sv.childAlignment = TextAnchor.UpperLeft;
            sv.childControlWidth = true; sv.childControlHeight = false;
            sv.childForceExpandWidth = true; sv.childForceExpandHeight = false;
            NewText(statusBg.transform, "StatusLabel", "STATUS", 22, FontStyle.Bold, TextAnchor.UpperLeft, Accent);
            statusText = NewText(statusBg.transform, "StatusText", "...", 24, FontStyle.Normal, TextAnchor.UpperLeft, TextWhite);
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Truncate;

            var countersBg = NewImage(row.transform, "Counters", BgPanel).gameObject;
            AddRoundedLook(countersBg);
            var cv = countersBg.AddComponent<VerticalLayoutGroup>();
            cv.padding = new RectOffset(20, 20, 14, 14);
            cv.spacing = 6;
            cv.childAlignment = TextAnchor.UpperLeft;
            cv.childControlWidth = true; cv.childControlHeight = false;
            cv.childForceExpandWidth = true; cv.childForceExpandHeight = false;
            NewText(countersBg.transform, "CountersLabel", "LIVE", 22, FontStyle.Bold, TextAnchor.UpperLeft, Accent);
            countersText = NewText(countersBg.transform, "CountersText",
                "Press an action button →", 22, FontStyle.Normal, TextAnchor.UpperLeft, TextDim);

            return row;
        }

        private static GameObject BuildButtonRow(Transform parent, out ButtonRefs refs)
        {
            refs = new ButtonRefs();

            var row = NewObject(parent, "ButtonRow");
            row.AddComponent<RectTransform>();
            var h = row.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 18;
            h.childAlignment = TextAnchor.UpperCenter;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = true; h.childForceExpandHeight = true;

            // Column 1 - Banner
            var col1 = MakeColumn(row.transform, "Banners", "BANNER");
            refs.bannerLoadTop = AddBtn(col1, "Load Top", Accent);
            refs.bannerLoadBot = AddBtn(col1, "Load Bottom", Accent);
            refs.bannerShow = AddBtn(col1, "Show", Accent);
            refs.bannerHide = AddBtn(col1, "Hide", AccentMuted);
            refs.bannerDestroy = AddBtn(col1, "Destroy", AccentMuted);
            AddSpacer(col1, 12);
            AddSectionHeader(col1, "MREC");
            refs.mrecLoad = AddBtn(col1, "Load", Accent);
            refs.mrecShow = AddBtn(col1, "Show", Accent);
            refs.mrecHide = AddBtn(col1, "Hide", AccentMuted);

            // Column 2 - Fullscreen
            var col2 = MakeColumn(row.transform, "Fullscreen", "INTERSTITIAL");
            refs.interLoad = AddBtn(col2, "Load", Accent);
            refs.interShowClosed = AddBtn(col2, "Show (Closed)", Accent);
            refs.interShowFail = AddBtn(col2, "Show (Failed)", AccentMuted);
            AddSpacer(col2, 12);
            AddSectionHeader(col2, "REWARDED");
            refs.rewardLoad = AddBtn(col2, "Load", Accent);
            refs.rewardShowR = AddBtn(col2, "Show (Rewarded)", Accent);
            refs.rewardShowC = AddBtn(col2, "Show (Cancelled)", AccentMuted);

            // Column 3 - Lifecycle / Consent
            var col3 = MakeColumn(row.transform, "Lifecycle", "CONSENT");
            refs.consentGrant = AddBtn(col3, "Grant", Accent);
            refs.consentDeny = AddBtn(col3, "Deny", AccentMuted);
            AddSpacer(col3, 12);
            AddSectionHeader(col3, "LIFECYCLE");
            refs.init = AddBtn(col3, "Initialize", Accent);
            refs.shutdown = AddBtn(col3, "Shutdown", AccentMuted);

            return row;
        }

        private static GameObject MakeColumn(Transform parent, string name, string headerText)
        {
            var col = NewImage(parent, name, BgPanelDark).gameObject;
            AddRoundedLook(col);
            var v = col.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(16, 16, 16, 16);
            v.spacing = 10;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            AddSectionHeader(col, headerText);
            return col;
        }

        private static void AddSectionHeader(GameObject col, string headerText)
        {
            var hdr = NewText(col.transform, "Section_" + headerText, headerText,
                22, FontStyle.Bold, TextAnchor.MiddleCenter, Accent);
            SetLayoutHeight(hdr.gameObject, 30);
        }

        private static void AddSpacer(GameObject col, float h)
        {
            var sp = NewObject(col.transform, "Spacer");
            sp.AddComponent<RectTransform>();
            SetLayoutHeight(sp, h);
        }

        private static Button AddBtn(GameObject column, string label, Color color)
        {
            var go = NewObject(column.transform, "Btn_" + label);
            var img = go.AddComponent<Image>();
            img.color = color;
            AddRoundedLook(go);
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.95f, 1f);
            colors.pressedColor = new Color(0.7f, 0.8f, 0.95f);
            btn.colors = colors;

            var txtGo = NewObject(go.transform, "Label");
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = TextWhite;

            SetLayoutHeight(go, 78);
            return btn;
        }

        private static GameObject BuildLogPanel(Transform parent, out Text logText)
        {
            var bg = NewImage(parent, "LogPanel", BgPanelDark).gameObject;
            AddRoundedLook(bg);
            var v = bg.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(20, 20, 14, 14);
            v.spacing = 6;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            NewText(bg.transform, "LogLabel", "LOG", 22, FontStyle.Bold, TextAnchor.UpperLeft, Accent);
            logText = NewText(bg.transform, "LogText", "", 22, FontStyle.Normal, TextAnchor.UpperLeft, TextDim);
            logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            logText.verticalOverflow = VerticalWrapMode.Truncate;
            SetLayoutHeight(logText.gameObject, 420);
            return bg;
        }

        // - Helpers -
        private static GameObject NewObject(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image NewImage(Transform parent, string name, Color color)
        {
            var go = NewObject(parent, name);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static Text NewText(Transform parent, string name, string content,
            int fontSize, FontStyle style, TextAnchor align, Color color)
        {
            var go = NewObject(parent, name);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        private static void Stretch(Image img)
        {
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void AddRoundedLook(GameObject go)
        {
            // Built-in Unity sprite "UISprite" is the default rounded panel shape; assigning it
            // to an Image gives a cleaner look than the flat default.
            var img = go.GetComponent<Image>();
            if (img == null) return;
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Sliced;
        }

        private static void SetLayoutHeight(GameObject go, float h)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = h;
            le.preferredHeight = h;
            le.flexibleHeight = 0;
        }

        // Calibrated from the user's saved RinvalAdsDemo.unity. Each section under Root has
        // anchor top-left and pivot (0.5, 0.5); the layout group computes width from parent.
        private static void ApplyTopAnchor(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        private struct ButtonRefs
        {
            public Button bannerLoadTop, bannerLoadBot, bannerShow, bannerHide, bannerDestroy;
            public Button interLoad, interShowClosed, interShowFail;
            public Button rewardLoad, rewardShowR, rewardShowC;
            public Button mrecLoad, mrecShow, mrecHide;
            public Button consentGrant, consentDeny;
            public Button init, shutdown;
        }
    }
}
