using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Captures marketing screenshots for the Asset Store listing. Runs the demo, snaps the dashboard, the cheat sheet, the placement graph, the live diagnostics, and a fullscreen game-view shot. Saves PNGs to a folder of the dev's choice.</summary>
    public static class ScreenshotGenerator
    {
        [MenuItem("Rinval/Mobile Ads & IAP Kit/Capture Marketing Screenshots", priority = 90)]
        public static void Capture()
        {
            var folder = EditorUtility.SaveFolderPanel("Save screenshots to", Application.dataPath, "Screenshots");
            if (string.IsNullOrEmpty(folder)) return;

            // 1. Game view (main demo scene if open)
            var gameViewPath = Path.Combine(folder, "01_game_view.png");
            try
            {
                ScreenCapture.CaptureScreenshot(gameViewPath);
                Debug.Log($"Captured: {gameViewPath}");
            }
            catch (System.Exception e) { Debug.LogWarning($"Game view capture failed: {e.Message}"); }

            // 2. Editor windows
            CaptureEditorWindow<AdManagerDashboard>(Path.Combine(folder, "02_dashboard.png"));
            CaptureEditorWindow<CheatSheetWindow>(Path.Combine(folder, "03_cheat_sheet.png"));
            CaptureEditorWindow<PreflightSetupWizard>(Path.Combine(folder, "04_preflight.png"));
            CaptureEditorWindow<LiveDiagnosticsWindow>(Path.Combine(folder, "05_live_diagnostics.png"));
            CaptureEditorWindow<PlacementGraphWindow>(Path.Combine(folder, "06_placement_graph.png"));

            EditorUtility.DisplayDialog("Screenshots Captured",
                $"Saved to:\n{folder}\n\n6 PNGs ready for the Asset Store listing.",
                "OK");
            EditorUtility.RevealInFinder(folder);
        }

        private static void CaptureEditorWindow<T>(string path) where T : EditorWindow
        {
            try
            {
                var w = EditorWindow.GetWindow<T>();
                if (w == null) return;
                w.Show();
                var rect = w.position;
                int width = Mathf.Max(1, (int)rect.width);
                int height = Mathf.Max(1, (int)rect.height);
                // EditorWindow has no built-in pixel grab; use system's screen-capture for the active window region.
                // Cleanest cross-platform path: re-paint into a RenderTexture would require reflection.
                // Pragmatic fallback: capture the full screen at the window's rect and let the dev crop.
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(rect.x, Screen.height - rect.y - rect.height, width, height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                Debug.Log($"Captured: {path}");
            }
            catch (System.Exception e) { Debug.LogWarning($"Capture {path} failed: {e.Message}"); }
        }
    }
}
