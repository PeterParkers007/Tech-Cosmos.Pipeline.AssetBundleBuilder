# AssetBundle Builder

智能可配置的AssetBundle打包工具，告别硬编码，拥抱灵活配置！

## 特性

### 智能命名系统
- 自适应规则：基于路径关键词自动分类
- 多种命名模式：扁平化，分层，完整路径，自定义
- 优先级控制：规则按优先级顺序应用
- 实时预览：构建前预览命名结果

### 可视化配置
- ScriptableObject配置：无需修改代码即可调整规则
- 灵活规则设置：支持路径匹配，自定义模式
- 批量操作：一键处理多个资源文件夹

### 专业功能
- 构建报告：详细的打包报告和依赖分析
- 多平台支持：一键切换不同目标平台
- 压缩选项：支持多种压缩格式

## 安装

### 通过 Unity Package Manager
1. 打开 Unity Package Manager
2. 点击 "+" 按钮 → "Add package from git URL"
3. 输入：`https://github.com/your-username/smart-bundle-builder.git`

### 手动安装
1. 下载最新release
2. 将Tech-Cosmos文件夹拖入项目的Assets目录

## 快速开始

### 1. 打开工具窗口
在Unity编辑器中：
Tech-Cosmos - AssetBundle Builder

### 2. 创建配置文件
- 点击工具窗口中的"创建新配置"按钮
- 或者在Project窗口右键：Create - Tech-Cosmos - AssetBundle Config

### 3. 配置命名规则
示例配置UI资源：
Path Keyword: "UI"
Naming Pattern: TwoLevelFolders
Priority: 10
结果：Assets/Art/UI/Icons/health.png - ui/icons

### 4. 构建AssetBundles
- 选择输出路径和目标平台
- 点击"构建AssetBundles"按钮
- 查看生成的构建报告

## 配置示例

### 基础配置
UI资源规则：
Path Keyword: "UI"
Naming Pattern: TwoLevelFolders
Priority: 10

角色资源规则：
Path Keyword: "Character"
Naming Pattern: TwoLevelFolders
Priority: 10

特效资源规则：
Path Keyword: "Effects"
Naming Pattern: TwoLevelFolders
Priority: 10

### 自定义命名模式
使用占位符创建个性化命名：

模式: "ab_{1}_{0}"
输入: Assets/Art/UI/Icons/health.png
输出: ab_ui_icons

可用占位符:
{0} - 当前文件夹名
{1} - 父文件夹名
{2} - 祖父文件夹名
{filename} - 文件名（不含扩展名）
{parent} - 直接父文件夹名

### 扁平化结构配置
在配置中启用Use Flat Structure选项，将分层路径转换为下划线连接：
原始：ui/icons
扁平化：ui_icons

## 目录结构

Assets/
-Tech-Cosmos/
--Editor/
---SmartBundleBuilder.cs      主工具窗口
---PreviewWindow.cs           命名预览窗口
--ScriptableObjects/
---AssetBundleConfig.cs       主配置类
---BundleNamingRule.cs        命名规则数据
---Enums/
----NamingPattern.cs          枚举定义
--Samples~/
---BasicConfigs/              基础配置示例
---AdvancedConfigs/           高级配置示例

## 常见问题

### 为什么AssetBundle名称杂乱无章？
确保在配置中设置了统一的命名规则，避免混合使用不同的命名模式。

### 如何预览命名结果？
点击工具窗口中的"预览命名结果"按钮，查看资源如何被分配到各个AssetBundle。

### 构建报告在哪里？
构建完成后，在输出目录中查看build_report.txt文件。

## 技术支持

- 邮箱：your-email@example.com
- Issue提交：https://github.com/your-username/smart-bundle-builder/issues
- 文档：https://github.com/your-username/smart-bundle-builder/wiki

## 许可证

MIT License - 详见LICENSE文件

---

让AssetBundle打包变得简单而强大！