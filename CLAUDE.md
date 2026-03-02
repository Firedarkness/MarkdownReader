# Markdown Reader 项目文档

## 项目概述

Markdown Reader 是一个基于 WPF (.NET 10) 开发的 Markdown 文件阅读器，提供简洁的阅读体验和实用的功能。

## 技术栈

| 技术 | 版本/说明 |
|------|----------|
| .NET | 10.0-windows |
| WPF | 桌面应用框架 |
| Markdig | 0.40.0 - Markdown 解析库 |
| WebView2 | 1.0.2792.45 - HTML 渲染引擎 |

## 项目结构

```
MarkdownReader/
├── MarkdownReader/
│   ├── App.xaml / App.xaml.cs          # 应用程序入口
│   ├── MainWindow.xaml                 # 主窗口 UI
│   ├── MainWindow.xaml.cs              # 主窗口逻辑
│   ├── MarkdownReader.csproj           # 项目配置文件
│   └── Models/
│       └── HeadingItem.cs              # 标题项数据模型
├── MarkdownHtmlRenderer/
│   ├── MarkdownHtmlControl.xaml        # 自定义 HTML 渲染控件
│   ├── MarkdownHtmlControl.xaml.cs     # 控件逻辑
│   └── MarkdownHtmlRenderer.csproj     # 类库项目配置
└── MarkdownReader.slnx                 # 解决方案文件
```

## 功能特性

### 1. 文件操作
- **打开文件**: 菜单 `文件 → 打开` 或快捷键 `Ctrl+O`
- **文件关联**: 双击 .md 文件可直接打开程序预览
- **刷新文件**: 状态栏刷新按钮 🔄
- 支持的文件格式: `.md`, `.markdown`

### 2. 侧边栏目录
- 打开文件后自动解析 Markdown 标题（H1-H6）
- 以树形缩进显示标题层级
- 点击标题可跳转到对应位置
- 支持显示/隐藏侧边栏（状态栏目录按钮 ◀▶）
- 可拖拽调整侧边栏宽度

### 3. 链接处理
- 点击链接自动处理：
  - 网页链接 → 打开默认浏览器
  - Markdown 文件链接 → 在阅读器中打开
  - 其他文件 → 使用系统默认程序打开
  - 支持相对路径解析（基于当前文件目录）
- 支持本地图片显示（包括中文文件名）

### 4. 视图控制
- 滚动到顶部/底部（菜单 `视图 → 滚动到顶部/底部` 或状态栏按钮）

### 5. 渲染特性
- 基于 WebView2 的 HTML 渲染，支持完整的 Markdown 语法
- 自动标题锚点生成

## UI 设计

### 颜色主题
- 主色调: `#2B579A` (深蓝色) - 用于菜单栏、状态栏、侧边栏标题
- 悬停色: `#3B679A` (浅蓝色) - 按钮悬停效果
- 背景色: `#F5F5F5` (浅灰色) - 窗口背景
- 内容背景: `#FFFFFF` (白色) - 预览区域、侧边栏

### 界面布局
```
┌─────────────────────────────────────────┐
│ 菜单栏 (文件、视图、帮助)                 │
├─────────────────────────────────────────┤
│                                         │
│  ┌─────────┐  ┌───────────────────────┐ │
│  │ 目录    │  │                       │ │
│  │ 侧边栏  │  │   Markdown 预览区域   │ │
│  │         │  │   (WebView2)         │ │
│  └─────────┘  └───────────────────────┘ │
│                                         │
├─────────────────────────────────────────┤
│ 状态栏 | ◀🔄就绪        文件名          │
└─────────────────────────────────────────┘
```

## 核心组件

### MarkdownHtmlControl (MarkdownHtmlRenderer 项目)
自定义的 Markdown 渲染控件，基于 WebView2：
- 使用 Markdig 将 Markdown 转换为 HTML
- WebView2 渲染 HTML，提供更好的显示效果
- 虚拟主机映射支持本地资源加载
- 图片中文文件名 URL 解码处理
- 链接点击事件回调

```csharp
// 渲染 Markdown
public async Task RenderMarkdownAsync(string markdown)
{
    var html = Markdig.Markdown.ToHtml(markdown, _pipeline);
    html = ProcessLocalUrls(html);  // 处理本地路径
    var fullHtml = WrapHtml(html);
    await RenderHtmlAsync(fullHtml);
}
```

### HeadingItem.cs
标题项数据模型，包含：
- `Level`: 标题级别 (1-6)
- `Text`: 标题文本
- `Anchor`: 锚点 ID (保留但当前未使用)
- `IndentedText`: 带缩进的显示文本

### 标题解析
使用 Markdig 解析 Markdown 文档提取标题：
```csharp
var document = Markdig.Markdown.Parse(markdown, pipeline);
foreach (var block in document.Descendants<HeadingBlock>())
{
    // 提取标题信息
}
```

### 命令行参数支持
程序启动时检查命令行参数，自动加载传入的 Markdown 文件：
```csharp
private void LoadStartupFile()
{
    var args = Environment.GetCommandLineArgs();
    if (args.Length > 1)
    {
        var filePath = args[1];
        if (File.Exists(filePath))
        {
            LoadMarkdownFile(filePath);
        }
    }
}
```

## 开发说明

### 运行项目
```bash
cd MarkdownReader
dotnet restore
dotnet run
```

### 编译发布
```bash
# 编译 Release 版本
dotnet build -c Release

# 发布为独立应用
dotnet publish -c Release -r win-x64 --self-contained
```

### 依赖项
项目使用 NuGet 包管理，主要依赖：
- `Markdig` - Markdown 解析
- `Microsoft.Web.WebView2` - HTML 渲染

### 文件关联设置
要在 Windows 系统中关联 .md 文件：
1. 右键任意 .md 文件 → 打开方式 → 选择其他应用
2. 选择 MarkdownReader.exe，勾选"始终使用此应用打开"
3. 或通过注册表进行系统级关联

## 后续可扩展功能

- [ ] 深色模式支持
- [ ] 自定义主题支持
- [ ] 字体大小调整
- [ ] 文件编码自动检测
- [ ] 最近打开文件列表
- [ ] 书签功能
- [ ] 导出 PDF
- [ ] 全屏模式
- [ ] 搜索功能
- [ ] 多标签页支持
