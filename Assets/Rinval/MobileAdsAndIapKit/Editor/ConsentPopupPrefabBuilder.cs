using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Generates a pre-wired Consent popup prefab so devs don't have to lay one out by hand. Rinval → Mobile Ads & IAP Kit → Create Consent Popup Prefab.</summary>
    public static class ConsentPopupPrefabBuilder
    {
        private const string PrefabFolder = "Assets/Rinval/MobileAdsAndIapKit/Prefabs";
        private const string PrefabPath = "Assets/Rinval/MobileAdsAndIapKit/Prefabs/ConsentPopup.prefab";

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Create Consent Popup Prefab", priority = 10)]
        public static void Create()
        {
            if (!Directory.Exists(PrefabFolder)) Directory.CreateDirectory(PrefabFolder);

            // Build the prefab in memory then save.
            var canvas = new GameObject("ConsentPopup",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ConsentPopupController));
            var canvasComp = canvas.GetComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.sortingOrder = 1000;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var dim = MakeImage(canvas.transform, "Backdrop", new Color(0, 0, 0, 0.7f));
            Stretch(dim);

            var panel = MakeImage(canvas.transform, "Panel", new Color(0.15f, 0.15f, 0.18f, 1f));
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(900, 1100);
            panelRt.anchoredPosition = Vector2.zero;

            var title = MakeText(panel.transform, "Title", "Privacy & Personalization", 48, FontStyle.Bold);
            PlaceText(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(800, 80));

            var body = MakeText(panel.transform, "Body",
                "We use ads to keep this game free.\n\nTap Accept to allow personalized ads, or Decline for non-personalized ads only. You can change this anytime in Settings.",
                28, FontStyle.Normal);
            PlaceText(body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -300), new Vector2(820, 600));
            body.alignment = TextAnchor.UpperCenter;

            var acceptBtn = MakeButton(panel.transform, "AcceptButton", "Accept",
                new Color(0.2f, 0.55f, 0.95f, 1f), new Vector2(0.5f, 0f), new Vector2(0, 240), new Vector2(700, 110));
            var denyBtn = MakeButton(panel.transform, "DeclineButton", "Decline",
                new Color(0.4f, 0.4f, 0.4f, 1f), new Vector2(0.5f, 0f), new Vector2(0, 110), new Vector2(700, 110));
            var policyBtn = MakeButton(panel.transform, "PrivacyPolicyButton", "Privacy Policy",
                new Color(0.2f, 0.2f, 0.22f, 1f), new Vector2(0.5f, 0f), new Vector2(0, -10), new Vector2(700, 80));

            // Wire references on the controller, including the per-button label texts so a
            // ConsentSettings asset can override them at runtime.
            var controller = canvas.GetComponent<ConsentPopupController>();
            controller.panel = panel;
            controller.titleText = title;
            controller.bodyText = body;
            controller.acceptButton = acceptBtn.GetComponent<Button>();
            controller.denyButton = denyBtn.GetComponent<Button>();
            controller.privacyPolicyButton = policyBtn.GetComponent<Button>();
            controller.acceptButtonLabel = acceptBtn.GetComponentInChildren<Text>();
            controller.declineButtonLabel = denyBtn.GetComponentInChildren<Text>();
            controller.privacyPolicyButtonLabel = policyBtn.GetComponentInChildren<Text>();

            // Save as prefab and delete the scene instance.
            var prefab = PrefabUtility.SaveAsPrefabAsset(canvas, PrefabPath);
            Object.DestroyImmediate(canvas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = prefab;
            EditorUtility.DisplayDialog("Consent Popup Created",
                $"Prefab saved to:\n{PrefabPath}\n\nDrag it into your first scene and call Show() (or use ShowConsentOnFirstLaunch in MobileKitBootstrap).",
                "OK");
        }

        // - helpers -
        private static GameObject MakeImage(Transform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = c;
            return go;
        }

        private static Text MakeText(Transform parent, string name, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static GameObject MakeButton(Transform parent, string name, string label, Color bg,
            Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bg;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var lbl = MakeText(go.transform, "Label", label, 32, FontStyle.Bold);
            var lblRt = lbl.GetComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            return go;
        }

        private static void PlaceText(Text t, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var rt = t.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        private static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
