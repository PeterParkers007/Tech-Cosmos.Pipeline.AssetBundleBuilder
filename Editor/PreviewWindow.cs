// 预览窗口类
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
namespace TechCosmos.AssetBundleBuilder.Editor
{
    public class PreviewWindow : EditorWindow
    {
        private BundleBuilder builder;
        private string previewFolder;
        private Vector2 scrollPos;

        public void ShowPreview(BundleBuilder abBuilder, string folder)
        {
            builder = abBuilder;
            previewFolder = folder;
            ShowUtility();
            titleContent = new GUIContent("🔍 命名预览");
        }

        private void OnGUI()
        {
            if (builder == null || string.IsNullOrEmpty(previewFolder))
            {
                Close();
                return;
            }

            GUILayout.Label("🔍 AssetBundle 命名预览", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"预览文件夹: {previewFolder}", MessageType.Info);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var guids = AssetDatabase.FindAssets("", new[] { previewFolder });
            foreach (var guid in guids.Take(50)) // 限制预览数量
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".meta")) continue;

                var bundleName = builder.GenerateBundleName(assetPath);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(Path.GetFileName(assetPath), EditorStyles.miniLabel, GUILayout.Width(150));
                EditorGUILayout.LabelField("→", GUILayout.Width(20));
                EditorGUILayout.TextField(bundleName);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (guids.Length > 50)
            {
                EditorGUILayout.HelpBox($"只显示前50个资源，总共 {guids.Length} 个资源", MessageType.Info);
            }
        }
    }
}
