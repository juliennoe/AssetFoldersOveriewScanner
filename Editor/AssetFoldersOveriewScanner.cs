using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace JulienNoe.Tools.AssetFoldersOveriewScanner
{
    /// <summary>
    /// Editor window that scans the Assets/ folder and displays usage statistics per folder.
    /// </summary>
    public class AssetFoldersOveriewScanner : EditorWindow
    {
        // Maps each folder path to a breakdown of asset type counts
        private Dictionary<string, Dictionary<string, int>> folderUsage = new();
        private List<string> sortedFolders = new();   // Folders sorted for display
        private List<string> naturalFolders = new();  // Discovery order list
        private Vector2 scrollPos = Vector2.zero;     // Scroll position for UI
        private int minElementCount = 10;             // Minimum assets to display a folder

        private string[] sortOrderOptions = new[] { "Natural", "By type", "By asset count" };
        private int sortOrderIndex = 2;               // Default sort by asset count

        private GUIStyle _clickableBoxStyle;
        private GUIStyle ClickableBoxStyle
        {
            get
            {
                if (_clickableBoxStyle == null)
                {
                    _clickableBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        alignment = TextAnchor.UpperLeft,
                        richText = true,
                        wordWrap = true
                    };
                }
                return _clickableBoxStyle;
            }
        }

        private bool showHelp = false; // Toggles help section in UI

        [MenuItem("Tools/Julien Noe/Asset Folders Overview Scanner")]
        public static void ShowWindow()
        {
            // Opens the window or focuses it if already open
            GetWindow<AssetFoldersOveriewScanner>("Asset Folders Overview Scanner");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            // Help dropdown
            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            if (showHelp)
            {
                EditorGUILayout.HelpBox("This tool analyzes the 'Assets/' folder and displays folder usage based on asset types.\n\n" +
                                        "- Choose how to sort folders (natural, by type, or by count).\n" +
                                        "- Only folders with more than the threshold are shown.\n" +
                                        "- Click a folder to highlight it in the Project window.",
                                        MessageType.Info);
                GUILayout.Space(5);
            }

            // Input for minimum asset threshold
            int newMin = EditorGUILayout.IntField("Min elements per folder", minElementCount);
            if (newMin != minElementCount)
            {
                minElementCount = newMin;
                if (folderUsage.Count > 0)
                    UpdateSortedFolders();
            }

            // Sorting mode dropdown
            int newSort = EditorGUILayout.Popup("Sort folders by", sortOrderIndex, sortOrderOptions);
            if (newSort != sortOrderIndex)
            {
                sortOrderIndex = newSort;
                if (folderUsage.Count > 0)
                    UpdateSortedFolders();
            }

            GUILayout.Space(5);

            // Green 'Analyze' button
            Color prevCol = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Analyze Assets Folder", GUILayout.Height(30)))
            {
                ScanAssetsFolder();
            }
            GUI.backgroundColor = prevCol;

            GUILayout.Space(10);

            // Display results in a scroll view
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (sortedFolders.Count == 0)
            {
                EditorGUILayout.LabelField("No folder to display. Click 'Analyze Assets Folder'.");
            }
            else
            {
                foreach (string folder in sortedFolders)
                {
                    var assetCounts = folderUsage[folder];
                    int total = assetCounts.Values.Sum();
                    var majority = assetCounts.OrderByDescending(kvp => kvp.Value).First();
                    Color btnColor = GetColorForCategory(majority.Key);

                    Color old = GUI.backgroundColor;
                    GUI.backgroundColor = btnColor;

                    // Build the detail label
                    var parts = assetCounts.Select(kvp => $"{kvp.Value} {kvp.Key}");
                    string detail = string.Join(", ", parts);
                    string label = $"<b>Folder:</b> {folder}\n<b>Total assets:</b> {total}\n<b>Breakdown:</b> {detail}";

                    if (GUILayout.Button(label, ClickableBoxStyle))
                    {
                        PingFolder(folder);
                        Focus();
                    }

                    GUI.backgroundColor = old;
                    GUILayout.Space(5);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Scans the Assets folder and updates folderUsage.
        /// </summary>
        private void ScanAssetsFolder()
        {
            var paths = AssetDatabase.GetAllAssetPaths()
                                     .Where(p => p.StartsWith("Assets/"))
                                     .ToArray();

            folderUsage.Clear();
            naturalFolders.Clear();

            foreach (string path in paths)
            {
                string folder = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(folder)) continue;

                if (!folderUsage.ContainsKey(folder))
                {
                    naturalFolders.Add(folder);
                    folderUsage[folder] = new Dictionary<string, int>();
                }

                string ext = Path.GetExtension(path).ToLower();
                string category = ext switch
                {
                    ".cs" => "Scripts",
                    ".shader" => "Shaders",
                    ".png" => ".png",
                    ".jpg" or ".jpeg" => ".jpg",
                    ".prefab" => "Prefabs",
                    ".mat" => "Materials",
                    ".mp3" or ".wav" => "Audio",
                    ".asset" => AssetDatabase.LoadAssetAtPath<Object>(path) is ScriptableObject ? "ScriptableObject" : ".asset",
                    _ => ext
                };

                if (!folderUsage[folder].ContainsKey(category))
                    folderUsage[folder][category] = 0;

                folderUsage[folder][category]++;
            }

            UpdateSortedFolders();
        }

        /// <summary>
        /// Refreshes the sortedFolders list based on the selected criteria.
        /// </summary>
        private void UpdateSortedFolders()
        {
            var filtered = folderUsage.Keys
                                      .Where(f => folderUsage[f].Values.Sum() >= minElementCount);

            switch (sortOrderIndex)
            {
                case 0:
                    sortedFolders = naturalFolders.Where(f => filtered.Contains(f)).ToList();
                    break;
                case 1:
                    sortedFolders = filtered
                        .OrderBy(f =>
                        {
                            var maj = folderUsage[f].OrderByDescending(kvp => kvp.Value).First();
                            bool gray = GetColorForCategory(maj.Key) == Color.gray;
                            return gray ? 1 : 0;
                        })
                        .ThenBy(f => folderUsage[f].OrderByDescending(kvp => kvp.Value).First().Key)
                        .ThenBy(f => f)
                        .ToList();
                    break;
                default:
                    sortedFolders = filtered.OrderByDescending(f => folderUsage[f].Values.Sum()).ToList();
                    break;
            }
        }

        /// <summary>
        /// Returns a color associated with the asset type category.
        /// </summary>
        private Color GetColorForCategory(string typeCategory)
        {
            return typeCategory switch
            {
                ".png" => Color.green,
                ".jpg" or ".jpeg" => Color.red,
                "Scripts" => Color.yellow,
                "Audio" => Color.blue,
                "Shaders" => Color.magenta,
                "Prefabs" => Color.cyan,
                "Materials" => Color.white,
                "ScriptableObject" => new Color(0.7f, 1f, 0.7f),
                _ => Color.gray
            };
        }

        /// <summary>
        /// Selects and pings the specified folder in the Project window.
        /// </summary>
        private void PingFolder(string folder)
        {
            var folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
            if (folderAsset != null)
            {
                Selection.activeObject = folderAsset;
                EditorGUIUtility.PingObject(folderAsset);
            }
            else
            {
                Debug.LogWarning($"Could not load folder: {folder}");
            }
        }
    }
}
