using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Visual map of every placement and its rules. Lives in the editor - devs see at a glance what placements exist, what caps each has, and which one was last attempted (with verdict).</summary>
    public class PlacementGraphWindow : EditorWindow
    {
        private PlacementCatalog _placements;
        private AdManagerConfig _config;
        private Vector2 _scroll;

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Placement Graph", priority = 4)]
        public static void Open()
        {
            var w = GetWindow<PlacementGraphWindow>("Placement Graph");
            w.minSize = new Vector2(720, 480);
            w.Show();
        }

        private void OnEnable()
        {
            if (_placements == null)
            {
                var guids = AssetDatabase.FindAssets("t:PlacementCatalog");
                if (guids.Length > 0)
                    _placements = AssetDatabase.LoadAssetAtPath<PlacementCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            if (_config == null)
            {
                var guids = AssetDatabase.FindAssets("t:AdManagerConfig");
                if (guids.Length > 0)
                    _config = AssetDatabase.LoadAssetAtPath<AdManagerConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Placement Graph", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
            _placements = (PlacementCatalog)EditorGUILayout.ObjectField("Placement Catalog", _placements, typeof(PlacementCatalog), false);
            _config = (AdManagerConfig)EditorGUILayout.ObjectField("Ad Config", _config, typeof(AdManagerConfig), false);

            if (_placements == null || _placements.Placements.Count == 0)
            {
                EditorGUILayout.HelpBox("Assign a PlacementCatalog with at least one placement.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(8);
            DrawLastVerdict();
            EditorGUILayout.Space(8);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            int columns = Mathf.Max(1, Mathf.FloorToInt(position.width / 260));
            int col = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var p in _placements.Placements)
            {
                DrawPlacementCard(p);
                col++;
                if (col >= columns) { col = 0; EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            if (Application.isPlaying) Repaint();
        }

        private void DrawLastVerdict()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Last Show Attempt", EditorStyles.boldLabel);
            if (Application.isPlaying)
            {
                var v = AdManager.LastShowVerdict;
                EditorGUILayout.LabelField($"Format: {v.Format}   Placement: {v.Placement}");
                var color = GUI.contentColor;
                GUI.contentColor = v.Allowed ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.5f, 0.5f);
                EditorGUILayout.LabelField(v.Allowed ? "ALLOWED" : "BLOCKED: " + v.Reason);
                GUI.contentColor = color;
            }
            else EditorGUILayout.LabelField("(enter Play mode to see live verdicts)", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawPlacementCard(string placementKey)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(250), GUILayout.MinHeight(160));
            EditorGUILayout.LabelField(placementKey, new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 });

            // Find a matching PlacementRule on the AdManagerConfig
            PlacementRule rule = null;
            if (_config != null)
            {
                foreach (var r in _config.PlacementRules)
                    if (r != null && r.Placement == placementKey) { rule = r; break; }
            }
            if (rule != null)
            {
                EditorGUILayout.LabelField($"Min interval: {rule.MinIntervalSeconds}s");
                EditorGUILayout.LabelField($"Max in window: {rule.MaxPerWindow}");
                EditorGUILayout.LabelField($"Window: {rule.WindowSeconds}s");
            }
            else
            {
                EditorGUILayout.LabelField("(uses global caps)", EditorStyles.miniLabel);
                if (_config != null)
                {
                    EditorGUILayout.LabelField($"Global min interval: {_config.InterstitialMinIntervalSeconds}s");
                    EditorGUILayout.LabelField($"Global max/window: {_config.InterstitialMaxPerWindow}");
                }
            }

            // A/B variant if any
            if (Application.isPlaying && AbTestManager.Catalog != null)
            {
                foreach (var exp in AbTestManager.Catalog.Experiments)
                {
                    if (exp.Placement == placementKey)
                    {
                        var v = AbTestManager.GetVariantKey(exp.Key);
                        EditorGUILayout.LabelField($"A/B: {exp.Key} → {v}", EditorStyles.miniBoldLabel);
                    }
                }
            }

            // Last verdict if matches
            if (Application.isPlaying && AdManager.LastShowVerdict.Placement == placementKey)
            {
                var c = GUI.backgroundColor;
                GUI.backgroundColor = AdManager.LastShowVerdict.Allowed ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.5f, 0.5f);
                EditorGUILayout.LabelField(AdManager.LastShowVerdict.ToString(), EditorStyles.helpBox);
                GUI.backgroundColor = c;
            }
            EditorGUILayout.EndVertical();
        }
    }
}
