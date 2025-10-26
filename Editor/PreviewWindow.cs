using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TechCosmos.AssetBundleBuilder.Editor
{
    public class PreviewWindow : EditorWindow
    {
        private BundleBuilder builder;
        private List<string> previewFolders; // 🎯 改为列表存储所有文件夹
        private Vector2 scrollPos;
        private Dictionary<string, List<string>> folderResults; // 🎯 按文件夹分组显示
        private bool[] folderFoldouts; // 🎯 每个文件夹的折叠状态

        public void ShowPreview(BundleBuilder abBuilder, List<string> folders)
        {
            builder = abBuilder;
            previewFolders = folders;

            // 🎯 初始化折叠状态
            folderFoldouts = new bool[folders.Count];
            for (int i = 0; i < folderFoldouts.Length; i++)
            {
                folderFoldouts[i] = true; // 默认展开
            }

            GeneratePreviewData();
            ShowUtility();
            titleContent = new GUIContent("🔍 命名预览 - 所有文件夹");
            minSize = new Vector2(600, 400);
        }

        private void GeneratePreviewData()
        {
            folderResults = new Dictionary<string, List<string>>();

            foreach (var folder in previewFolders)
            {
                if (!Directory.Exists(folder)) continue;

                var guids = AssetDatabase.FindAssets("", new[] { folder });
                var results = new List<string>();

                foreach (var guid in guids.Take(100)) // 限制每个文件夹的预览数量
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (ShouldSkipAssetForPreview(assetPath)) continue;

                    var bundleName = builder.GenerateBundleName(assetPath);
                    results.Add($"{Path.GetFileName(assetPath)} → {bundleName}");
                }

                folderResults[folder] = results;
            }
        }
        private bool ShouldSkipAssetForPreview(string assetPath)
        {
            return AssetBundleFilter.ShouldSkipAsset(assetPath);
        }
        private void OnGUI()
        {
            if (builder == null || previewFolders == null || previewFolders.Count == 0)
            {
                Close();
                return;
            }

            GUILayout.Label("🔍 AssetBundle 命名预览 - 所有扫描文件夹", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"共扫描 {previewFolders.Count} 个文件夹", MessageType.Info);

            // 🎯 刷新按钮
            if (GUILayout.Button("🔄 刷新预览", GUILayout.Height(30)))
            {
                GeneratePreviewData();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 🎯 按文件夹分组显示
            for (int i = 0; i < previewFolders.Count; i++)
            {
                var folder = previewFolders[i];

                EditorGUILayout.Space();

                // 🎯 文件夹折叠标题
                string folderName = Path.GetFileName(folder);
                if (string.IsNullOrEmpty(folderName)) folderName = "根目录";

                folderFoldouts[i] = EditorGUILayout.Foldout(folderFoldouts[i],
                    $"📁 {folderName} ({folder})", true, EditorStyles.foldoutHeader);

                if (folderFoldouts[i])
                {
                    if (folderResults != null && folderResults.ContainsKey(folder))
                    {
                        var results = folderResults[folder];

                        EditorGUI.indentLevel++;

                        if (results.Count == 0)
                        {
                            EditorGUILayout.HelpBox("该文件夹中没有找到资源文件", MessageType.Info);
                        }
                        else
                        {
                            // 🎯 显示该文件夹下的所有资源
                            foreach (var result in results)
                            {
                                EditorGUILayout.BeginHorizontal();
                                var parts = result.Split(new[] { " → " }, System.StringSplitOptions.None);
                                if (parts.Length == 2)
                                {
                                    EditorGUILayout.LabelField(parts[0], EditorStyles.miniLabel, GUILayout.Width(200));
                                    EditorGUILayout.LabelField("→", GUILayout.Width(20));
                                    EditorGUILayout.TextField(parts[1]);
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(result);
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            // 🎯 显示数量统计
                            EditorGUILayout.HelpBox($"共 {results.Count} 个资源文件", MessageType.None);
                        }

                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("文件夹不存在或无法访问", MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            // 🎯 显示总统计信息
            EditorGUILayout.Space();
            int totalFiles = folderResults?.Values.Sum(list => list.Count) ?? 0;
            EditorGUILayout.HelpBox($"总计: {previewFolders.Count} 个文件夹, {totalFiles} 个资源文件", MessageType.Info);
        }
    }
}