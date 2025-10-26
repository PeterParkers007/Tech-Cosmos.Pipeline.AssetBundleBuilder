// 新建一个静态工具类
using System.IO;
using System.Linq;
namespace TechCosmos.AssetBundleBuilder.Editor
{
    public static class AssetBundleFilter
    {
        // 共享的资源过滤逻辑
        public static bool ShouldSkipAsset(string assetPath)
        {
            // 跳过meta文件
            if (assetPath.EndsWith(".meta"))
                return true;

            string extension = Path.GetExtension(assetPath)?.ToLower();

            // 如果无法获取扩展名，跳过
            if (string.IsNullOrEmpty(extension))
                return true;

            // 明确跳过不需要打包的文件类型
            string[] skipExtensions = {
            // 脚本和代码文件
            ".cs", ".js", 
            // 编辑器相关文件
            ".asmdef", ".md", ".editor",
            // 插件和程序集文件
            ".dll", ".so", ".bundle", ".a",
            // 压缩包和文档
            ".zip", ".rar", ".7z", ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            // Unity特定不需要打包的文件
            ".lighting", ".gradle", ".props", ".template", ".rsp",
            // 其他
            ".db", ".tmp", ".bak"
        };

            // 需要打包的资源类型
            string[] bundleExtensions = {
            // 纹理资源
            ".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff", ".bmp", ".psd", ".exr", ".hdr",
            // 3D模型
            ".fbx", ".obj", ".blend", ".max", ".ma", ".mb", ".3ds", ".dae", ".dxf",
            // 材质和着色器
            ".mat", ".shader", ".shadergraph", ".compute",
            // 音频资源
            ".wav", ".mp3", ".ogg", ".aiff", ".aif", ".mod", ".it", ".s3m", ".xm",
            // 动画资源
            ".anim", ".controller", ".overridecontroller", ".mask", ".playable",
            // 预制体和场景
            ".prefab", ".unity",
            // UI资源
            ".spriteatlas", ".guiskin", ".font", ".ttf", ".otf",
            // 配置文件
            ".asset", ".json", ".xml", ".txt",
            // 视频资源
            ".mp4", ".mov", ".avi", ".webm", ".ogv",
            // 物理材质
            ".physicmaterial", ".physicsmaterial2d",
            // 渲染相关
            ".rendertexture", ".cubemap", ".flare", ".terrainlayer"
        };

            // 如果在跳过列表中，直接跳过
            if (skipExtensions.Contains(extension))
            {
                return true;
            }

            // 如果不在打包列表中，也跳过
            if (!bundleExtensions.Contains(extension))
            {
                return true;
            }

            // 特殊文件检查
            var fileName = Path.GetFileName(assetPath).ToLower();

            // 跳过以 ~ 开头的临时文件
            if (fileName.StartsWith("~") || fileName.StartsWith("."))
                return true;

            // 跳过包含"editor"路径的文件（编辑器专用资源）
            if (assetPath.ToLower().Contains("/editor/"))
                return true;

            return false;
        }
    }
}
