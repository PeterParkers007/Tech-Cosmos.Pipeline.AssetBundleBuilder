using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TechCosmos.AssetBundleBuilder.SO;
using TechCosmos.AssetBundleBuilder.Data; 
namespace TechCosmos.AssetBundleBuilder.Editor
{
    public class BundleBuilder : EditorWindow
    {
        // 配置引用
        private AssetBundleConfig config;
        private List<string> sourceFolders = new List<string>() { "Assets/Art" };
        private string outputPath = "AssetBundles";
        private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        private Vector2 scrollPosition;

        [MenuItem("Tech-Cosmos/AssetBundle Builder")]
        public static void ShowWindow()
        {
            GetWindow<BundleBuilder>("AB Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 标题
            EditorGUILayout.Space();
            GUILayout.Label("🎯 AssetBundle 打包工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // === 配置文件 ===
            GUILayout.Label("⚙️ 配置文件", EditorStyles.boldLabel);
            config = (AssetBundleConfig)EditorGUILayout.ObjectField("配置文件", config, typeof(AssetBundleConfig), false);

            if (config == null)
            {
                EditorGUILayout.HelpBox("请创建或指定配置文件", MessageType.Warning);
                if (GUILayout.Button("📝 创建新配置"))
                {
                    CreateNewConfig();
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"已加载配置: {config.name}", MessageType.Info);
            }

            // === 基础配置 ===
            GUILayout.Label("📁 扫描文件夹", EditorStyles.boldLabel);
            DrawFolderList();

            GUILayout.Label("📤 输出配置", EditorStyles.boldLabel);
            DrawOutputConfig();

            // === 构建按钮 ===
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 构建 AssetBundles", GUILayout.Height(40)))
            {
                BuildAllAssetBundles();
            }
            GUI.backgroundColor = Color.white;

            // === 工具按钮 ===
            EditorGUILayout.Space();
            if (GUILayout.Button("🔍 预览命名结果"))
            {
                PreviewNamingResults();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFolderList()
        {
            int itemToRemove = -1;

            // 第一遍：只标记，不执行删除
            for (int i = 0; i < sourceFolders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                sourceFolders[i] = EditorGUILayout.TextField($"文件夹 {i + 1}:", sourceFolders[i]);
                if (GUILayout.Button("×", GUILayout.Width(30)))
                {
                    itemToRemove = i; // 只标记，不删除
                                      // 不要 break，继续完成当前布局
                }
                EditorGUILayout.EndHorizontal();
            }

            // 第二遍：在GUI布局之外执行删除
            if (itemToRemove >= 0)
            {
                sourceFolders.RemoveAt(itemToRemove);
                // 可选：强制刷新GUI
                Repaint();
            }

            // 添加文件夹的按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 添加文件夹"))
            {
                sourceFolders.Add("Assets/");
            }
            if (GUILayout.Button("📁 选择文件夹"))
            {
                var path = EditorUtility.OpenFolderPanel("选择资源文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        sourceFolders.Add("Assets" + path.Substring(Application.dataPath.Length));
                    }
                    else
                    {
                        sourceFolders.Add(path);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutputConfig()
        {
            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("输出路径:", outputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                var path = EditorUtility.SaveFolderPanel("选择输出目录", outputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台:", buildTarget);
        }

        private void CreateNewConfig()
        {
            var newConfig = CreateInstance<AssetBundleConfig>();

            // 添加一些默认规则
            newConfig.namingRules = new List<BundleNamingRule>
        {
            new BundleNamingRule
            {
                pathKeyword = "UI",
                namingPattern = NamingPattern.TwoLevelFolders,
                priority = 10
            },
            new BundleNamingRule
            {
                pathKeyword = "Character",
                namingPattern = NamingPattern.TwoLevelFolders,
                priority = 10
            },
            new BundleNamingRule
            {
                pathKeyword = "Effect",
                namingPattern = NamingPattern.TwoLevelFolders,
                priority = 10
            }
        };

            var path = EditorUtility.SaveFilePanelInProject("保存配置", "AssetBundleConfig", "asset", "保存配置文件");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                config = newConfig;
            }
        }

        public string GenerateBundleName(string assetPath)
        {
            if (config == null)
            {
                // 无配置时的默认行为
                return Path.GetFileName(Path.GetDirectoryName(assetPath))?.ToLower();
            }

            // 按优先级排序规则
            var sortedRules = new List<BundleNamingRule>(config.namingRules);
            sortedRules.Sort((a, b) => b.priority.CompareTo(a.priority));

            // 应用匹配的规则
            foreach (var rule in sortedRules)
            {
                if (!string.IsNullOrEmpty(rule.pathKeyword) &&
                    assetPath.Contains(rule.pathKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    return ApplyNamingPattern(assetPath, rule);
                }
            }

            // 应用默认规则
            return ApplyDefaultNamingPattern(assetPath);
        }

        private string ApplyNamingPattern(string assetPath, BundleNamingRule rule)
        {
            var directories = assetPath.Split('/');

            switch (rule.namingPattern)
            {
                case NamingPattern.ParentFolder:
                    return Path.GetFileName(Path.GetDirectoryName(assetPath))?.ToLower();

                case NamingPattern.TwoLevelFolders:
                    if (directories.Length >= 3)
                    {
                        string parent = directories[^2].ToLower();
                        string current = directories[^1].ToLower();
                        return config.useFlatStructure ?
                            $"{parent}_{current}" :
                            $"{parent}{config.separator}{current}";
                    }
                    break;

                case NamingPattern.FullPath:
                    // 移除 Assets/ 前缀，转换路径
                    var cleanPath = assetPath.StartsWith("Assets/") ? assetPath.Substring(7) : assetPath;
                    cleanPath = Path.ChangeExtension(cleanPath, null); // 移除扩展名
                    return cleanPath.ToLower().Replace('/', config.separator[0]);

                case NamingPattern.Custom:
                    if (!string.IsNullOrEmpty(rule.customPattern))
                    {
                        return ApplyCustomPattern(assetPath, rule.customPattern);
                    }
                    break;
            }

            return ApplyDefaultNamingPattern(assetPath);
        }

        private string ApplyCustomPattern(string assetPath, string pattern)
        {
            var directories = assetPath.Split('/');

            // 支持 {0} {1} {2} 等占位符
            string result = pattern;

            for (int i = 0; i < Math.Min(directories.Length, config.maxFolderDepth); i++)
            {
                string placeholder = "{" + i + "}";
                if (result.Contains(placeholder))
                {
                    result = result.Replace(placeholder, directories[directories.Length - 1 - i].ToLower());
                }
            }

            // 支持特殊占位符
            result = result.Replace("{filename}", Path.GetFileNameWithoutExtension(assetPath).ToLower());
            result = result.Replace("{parent}", Path.GetFileName(Path.GetDirectoryName(assetPath))?.ToLower());

            return result;
        }

        private string ApplyDefaultNamingPattern(string assetPath)
        {
            if (config == null) return Path.GetFileName(Path.GetDirectoryName(assetPath))?.ToLower();

            return ApplyNamingPattern(assetPath, new BundleNamingRule
            {
                namingPattern = config.defaultRule.pattern,
                customPattern = config.defaultRule.customPattern
            });
        }

        private void PreviewNamingResults()
        {
            if (sourceFolders.Count == 0) return;

            var previewWindow = CreateInstance<PreviewWindow>();
            previewWindow.ShowPreview(this, sourceFolders[0]);
        }
        public void BuildAllAssetBundles()
        {
            try
            {
                // 验证配置
                if (sourceFolders.Count == 0)
                {
                    EditorUtility.DisplayDialog("错误", "请至少配置一个扫描文件夹", "确定");
                    return;
                }

                // 检查文件夹是否存在
                foreach (var folder in sourceFolders)
                {
                    if (!Directory.Exists(folder))
                    {
                        EditorUtility.DisplayDialog("错误", $"文件夹不存在: {folder}", "确定");
                        return;
                    }
                }

                Debug.Log("🚀 开始构建 AssetBundles...");

                // 设置AssetBundle名称
                AutoSetAssetBundleNames();

                // 创建输出目录
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                // 执行构建
                BuildPipeline.BuildAssetBundles(outputPath,
                                                BuildAssetBundleOptions.ChunkBasedCompression,
                                                buildTarget);

                // 生成报告
                GenerateBuildReport();

                Debug.Log("✅ AssetBundle 构建成功！");
                EditorUtility.DisplayDialog("构建完成", "AssetBundle 构建成功！", "确定");
                EditorUtility.RevealInFinder(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ AssetBundle 构建失败: {e.Message}");
                EditorUtility.DisplayDialog("构建失败", $"构建过程中出现错误: {e.Message}", "确定");
            }
            finally
            {
                AssetDatabase.RemoveUnusedAssetBundleNames();
                AssetDatabase.Refresh();
            }
        }

        private void AutoSetAssetBundleNames()
        {
            // 清除所有旧的设置
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (var name in allAssetBundleNames)
            {
                AssetDatabase.RemoveAssetBundleName(name, true);
            }

            // 遍历所有配置的源文件夹
            foreach (var folder in sourceFolders)
            {
                if (!Directory.Exists(folder)) continue;

                // 获取文件夹下所有资源文件的GUID
                var guids = AssetDatabase.FindAssets("", new[] { folder });

                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    // 跳过meta文件和不支持的文件
                    if (assetPath.EndsWith(".meta")) continue;

                    // 根据路径自动生成AssetBundle名称
                    var bundleName = GenerateBundleName(assetPath);
                    if (string.IsNullOrEmpty(bundleName)) continue;

                    // 设置AssetBundle名称
                    var importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        importer.assetBundleName = bundleName.ToLower();
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
        private void GenerateBuildReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("🎯 AssetBundle 构建报告");
            report.AppendLine($"⏰ 生成时间: {DateTime.Now}");
            report.AppendLine($"🎮 目标平台: {buildTarget}");
            report.AppendLine($"📁 输出路径: {outputPath}");
            report.AppendLine("=================================");

            var bundles = AssetDatabase.GetAllAssetBundleNames();
            foreach (var bundleName in bundles)
            {
                report.AppendLine($"\n📦 {bundleName}");

                // 获取该bundle中的所有资源
                var assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                foreach (var asset in assets)
                {
                    report.AppendLine($"   └─ {asset}");
                }
            }

            // 将报告写入文件
            var reportPath = Path.Combine(outputPath, "build_report.txt");
            File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"📊 构建报告已生成: {reportPath}");
        }
    }
}
