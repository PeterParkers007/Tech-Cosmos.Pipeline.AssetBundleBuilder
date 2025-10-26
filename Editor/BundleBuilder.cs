using System;
using System.Linq;
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
            // 🎯 添加配置保护
            if (config != null && !AssetDatabase.Contains(config))
            {
                Debug.LogWarning("配置引用已失效，正在重置...");
                config = null;
            }

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

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
            }
        }

        private void DrawFolderList()
        {
            int itemToRemove = -1;

            // 第一遍：只标记，不执行删除
            for (int i = 0; i < sourceFolders.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    sourceFolders[i] = EditorGUILayout.TextField($"文件夹 {i + 1}:", sourceFolders[i]);
                    if (GUILayout.Button("×", GUILayout.Width(30)))
                    {
                        itemToRemove = i;
                    }
                }
            }

            // 第二遍：在GUI布局之外执行删除
            if (itemToRemove >= 0)
            {
                sourceFolders.RemoveAt(itemToRemove);
                Repaint();
            }

            // 添加文件夹的按钮
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ 添加文件夹"))
                {
                    sourceFolders.Add("Assets/Art/"); // 🎯 默认改为子目录
                }
                if (GUILayout.Button("📁 选择文件夹"))
                {
                    var path = EditorUtility.OpenFolderPanel("选择资源文件夹", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.StartsWith(Application.dataPath))
                        {
                            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                            // 🎯 避免重复添加
                            if (!sourceFolders.Contains(relativePath))
                                sourceFolders.Add(relativePath);
                        }
                        else
                        {
                            if (!sourceFolders.Contains(path))
                                sourceFolders.Add(path);
                        }
                    }
                }
            }

            // 🎯 显示根目录警告
            if (sourceFolders.Any(f => f == "Assets" || f == "Assets/"))
            {
                EditorGUILayout.HelpBox("⚠️ 扫描根目录 'Assets/' 可能导致问题，建议使用子目录", MessageType.Warning);
            }
        }

        private void DrawOutputConfig()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                outputPath = EditorGUILayout.TextField("输出路径:", outputPath);
                if (GUILayout.Button("浏览", GUILayout.Width(60)))
                {
                    var path = EditorUtility.SaveFolderPanel("选择输出目录", outputPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        outputPath = path;
                    }
                }
            }

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台:", buildTarget);
        }

        private void CreateNewConfig()
        {
            try
            {
                var path = EditorUtility.SaveFilePanelInProject("保存配置", "AssetBundleConfig", "asset", "保存配置文件");
                if (string.IsNullOrEmpty(path)) return;

                var newConfig = CreateInstance<AssetBundleConfig>();

                // 添加一些默认规则
                newConfig.namingRules = new List<BundleNamingRule>
                {
                    new BundleNamingRule
                    {
                        pathKeyword = "UI",
                        namingPattern = NamingPattern.TwoLevelFolders,
                        priority = 10,
                        customPattern = ""
                    },
                    new BundleNamingRule
                    {
                        pathKeyword = "Character",
                        namingPattern = NamingPattern.TwoLevelFolders,
                        priority = 10,
                        customPattern = ""
                    },
                    new BundleNamingRule
                    {
                        pathKeyword = "Effect",
                        namingPattern = NamingPattern.TwoLevelFolders,
                        priority = 10,
                        customPattern = ""
                    }
                };

                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();

                // 🎯 延迟加载避免引用问题
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                    config = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(path);
                    Repaint();
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"创建配置失败: {e.Message}");
            }
        }

        public string GenerateBundleName(string assetPath)
        {
            // 🎯 输入验证
            if (string.IsNullOrEmpty(assetPath))
                return "invalid_path";

            if (config == null)
            {
                var dirName = Path.GetFileName(Path.GetDirectoryName(assetPath));
                return string.IsNullOrEmpty(dirName) ? "default" : dirName.ToLower();
            }

            try
            {
                // 按优先级排序规则
                var sortedRules = new List<BundleNamingRule>(config.namingRules ?? new List<BundleNamingRule>());
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
            catch (Exception ex)
            {
                Debug.LogError($"生成Bundle名称时出错: {ex.Message}");
                return "error";
            }
        }

        private string ApplyNamingPattern(string assetPath, BundleNamingRule rule)
        {
            // 根目录检测和保护
            if (assetPath == "Assets" || assetPath == "Assets/")
                return "invalid_root_path";

            var directories = assetPath.Split('/').Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (directories.Length == 0) return "invalid_path";

            // 根目录文件特殊处理
            if (directories.Length == 2 && directories[0] == "Assets")
            {
                string fileName = Path.GetFileNameWithoutExtension(directories[1])?.ToLower() ?? "unknown";
                return $"root/{fileName}";
            }

            switch (rule.namingPattern)
            {
                case NamingPattern.ParentFolder:
                    var dirName = Path.GetFileName(Path.GetDirectoryName(assetPath));
                    return string.IsNullOrEmpty(dirName) ? "unknown" : dirName.ToLower();

                case NamingPattern.TwoLevelFolders:
                    return HandleTwoLevelFolders(directories);

                case NamingPattern.FullPath:
                    var cleanPath = assetPath.StartsWith("Assets/") ? assetPath.Substring(7) : assetPath;
                    cleanPath = Path.ChangeExtension(cleanPath, null);
                    // 🎯 安全的separator访问
                    char separator = (config?.separator?.Length > 0) ? config.separator[0] : '/';
                    return cleanPath.ToLower().Replace('/', separator);

                case NamingPattern.Custom:
                    if (!string.IsNullOrEmpty(rule.customPattern))
                    {
                        return ApplyCustomPattern(assetPath, rule.customPattern);
                    }
                    break;
            }

            return ApplyDefaultNamingPattern(assetPath);
        }

        private string HandleTwoLevelFolders(string[] directories)
        {
            if (directories.Length >= 3)
            {
                string parent = directories[^2]?.ToLower() ?? "unknown";
                string current = Path.GetFileNameWithoutExtension(directories[^1])?.ToLower() ?? "unknown";
                return $"{parent}/{current}";
            }
            else if (directories.Length == 2)
            {
                string fileName = Path.GetFileNameWithoutExtension(directories[1])?.ToLower() ?? "unknown";
                return $"root/{fileName}";
            }
            else
            {
                return "invalid_path";
            }
        }

        private string ApplyCustomPattern(string assetPath, string pattern)
        {
            try
            {
                var directories = assetPath.Split('/').Where(d => !string.IsNullOrEmpty(d)).ToArray();
                string result = pattern;

                // 🎯 安全的maxFolderDepth访问
                int maxDepth = (config?.maxFolderDepth ?? 3);
                maxDepth = Math.Max(1, Math.Min(maxDepth, directories.Length));

                for (int i = 0; i < maxDepth; i++)
                {
                    string placeholder = "{" + i + "}";
                    if (result.Contains(placeholder))
                    {
                        int index = directories.Length - 1 - i;
                        if (index >= 0 && index < directories.Length)
                        {
                            string folderName = directories[index]?.ToLower() ?? "unknown";
                            result = result.Replace(placeholder, folderName);
                        }
                    }
                }

                // 🎯 安全的占位符替换
                string fileName = Path.GetFileNameWithoutExtension(assetPath)?.ToLower() ?? "unknown";
                string parentDir = Path.GetFileName(Path.GetDirectoryName(assetPath))?.ToLower() ?? "unknown";

                result = result.Replace("{filename}", fileName);
                result = result.Replace("{parent}", parentDir);

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"应用自定义模式失败: {ex.Message}");
                return "custom_error";
            }
        }

        private string ApplyDefaultNamingPattern(string assetPath)
        {
            if (config == null || config.defaultRule == null)
            {
                var dirName = Path.GetFileName(Path.GetDirectoryName(assetPath));
                return string.IsNullOrEmpty(dirName) ? "default" : dirName.ToLower();
            }

            return ApplyNamingPattern(assetPath, new BundleNamingRule
            {
                namingPattern = config.defaultRule.pattern,
                customPattern = config.defaultRule.customPattern ?? ""
            });
        }

        private void PreviewNamingResults()
        {
            if (sourceFolders.Count == 0) return;

            // 🎯 配置保护
            if (config != null && !AssetDatabase.Contains(config))
            {
                Debug.LogError("配置资产引用已失效！");
                config = null;
                return;
            }

            try
            {
                var previewWindow = CreateInstance<PreviewWindow>();
                // 🎯 传递所有文件夹，而不仅仅是第一个
                previewWindow.ShowPreview(this, sourceFolders);
            }
            catch (Exception e)
            {
                Debug.LogError($"打开预览窗口失败: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"预览功能失败: {e.Message}", "确定");
            }
        }
        private bool ShouldSkipAsset(string assetPath)
        {
            return AssetBundleFilter.ShouldSkipAsset(assetPath);
        }
        public void BuildAllAssetBundles()
        {
            try
            {
                // 🎯 根目录警告
                var rootPaths = sourceFolders.Where(f => f == "Assets" || f == "Assets/").ToList();
                if (rootPaths.Count > 0)
                {
                    bool proceed = EditorUtility.DisplayDialog("警告",
                        "检测到可能问题的扫描路径（Assets/）。这可能导致闪退。是否继续？",
                        "继续", "取消");
                    if (!proceed) return;
                }

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

            // 使用HashSet避免重复处理
            var processedAssets = new HashSet<string>();

            // 遍历所有配置的源文件夹
            foreach (var folder in sourceFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Debug.LogWarning($"文件夹不存在，已跳过: {folder}");
                    continue;
                }

                // 获取文件夹下所有资源文件的GUID
                var guids = AssetDatabase.FindAssets("", new[] { folder });

                Debug.Log($"📁 扫描文件夹: {folder}，找到 {guids.Length} 个资源");

                int processedCount = 0;
                int skippedCount = 0;

                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    // 跳过已处理的资源
                    if (processedAssets.Contains(assetPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    processedAssets.Add(assetPath);

                    // 🎯 使用智能过滤跳过不需要打包的文件
                    if (ShouldSkipAsset(assetPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    // 根据路径自动生成AssetBundle名称
                    var bundleName = GenerateBundleName(assetPath);
                    if (string.IsNullOrEmpty(bundleName))
                    {
                        Debug.LogWarning($"无法生成Bundle名称，已跳过: {assetPath}");
                        skippedCount++;
                        continue;
                    }

                    // 设置AssetBundle名称
                    var importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        importer.assetBundleName = bundleName.ToLower();
                        processedCount++;

                        // 在详细日志模式下显示每个资源的处理结果
                        if (Debug.isDebugBuild)
                        {
                            Debug.Log($"✅ 设置Bundle: {Path.GetFileName(assetPath)} → {bundleName}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"无法获取AssetImporter: {assetPath}");
                        skippedCount++;
                    }
                }

                Debug.Log($"📊 文件夹 {folder} 处理完成: {processedCount} 个资源已设置Bundle, {skippedCount} 个资源已跳过");
            }

            AssetDatabase.SaveAssets();

            // 显示最终统计
            var finalBundles = AssetDatabase.GetAllAssetBundleNames();
            Debug.Log($"🎉 AssetBundle名称设置完成! 总共生成 {finalBundles.Length} 个Bundle");
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