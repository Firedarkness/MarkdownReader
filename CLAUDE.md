# Markdown Reader 项目文档

## 项目概述

Markdown Reader 是一个基于 WPF (.NET 10) 开发的 Markdown 文件阅读器，提供简洁的阅读体验和实用的功能。

## 技术栈

| 技术 | 版本/说明 |
|------|----------|
| .NET | 10.0-windows |
| WPF | 桌面应用框架 |
| Markdig | 0.40.0 - Markdown 解析库 |
| Markdig.Wpf | 0.5.0.1 - WPF Markdown 渲染控件 |

## 项目结构

```
MarkdownReader/
├── MarkdownReader/
│   ├── App.xaml / App.xaml.cs          # 应用程序入口
│   ├── MainWindow.xaml                 # 主窗口 UI
│   ├── MainWindow.xaml.cs              # 主窗口逻辑
│   ├── MarkdownReader.csproj           # 项目配置文件
│   ├── Models/
│   │   └── HeadingItem.cs              # 标题项数据模型
│   └── Controls/
│       └── CustomMarkdownViewer.cs     # 自定义 MarkdownViewer 控件
└── MarkdownReader.slnx                 # 解决方案文件
```

## 功能特性

### 1. 文件操作
- **打开文件**: 菜单 `文件 → 打开` 或快捷键 `Ctrl+O`
- **刷新文件**: 状态栏刷新按钮 🔄 或按 `F5`
- 支持的文件格式: `.md`, `.markdown`

### 2. 侧边栏目录
- 打开文件后自动解析 Markdown 标题（H1-H6）
- 以树形缩进显示标题层级
- 点击标题可跳转到对应位置
- 支持显示/隐藏侧边栏（状态栏目录按钮 📑 或 `Ctrl+B`）
- 可拖拽调整侧边栏宽度

### 3. 链接处理
- **Ctrl + 点击链接**:
  - 网页链接 → 打开默认浏览器
  - Markdown 文件链接 → 在阅读器中打开
  - 其他文件 → 使用系统默认程序打开
  - 支持相对路径解析（基于当前文件目录）
- 按住 Ctrl 键时，状态栏显示提示信息

### 4. 视图控制
- 滚动到顶部/底部（菜单 `视图 → 滚动到顶部/底部`）

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
│  │         │  │                       │ │
│  └─────────┘  └───────────────────────┘ │
│                                         │
├─────────────────────────────────────────┤
│ 状态栏 | 📑🔄就绪        文件名          │
└─────────────────────────────────────────┘
```

## 关键代码说明

### CustomMarkdownViewer.cs
继承自 `Markdig.Wpf.MarkdownViewer`，添加了超链接点击事件处理，实现 Ctrl+点击打开链接功能。

```csharp
// 监听超链接点击事件
AddHandler(Hyperlink.RequestNavigateEvent, new RoutedEventHandler(OnHyperlinkClick));
```

### HeadingItem.cs
标题项数据模型，包含：
- `Level`: 标题级别 (1-6)
- `Text`: 标题文本
- `Anchor`: 锚点 ID
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

## 开发说明

### 运行项目
```bash
cd MarkdownReader
dotnet restore
dotnet run
```

### 依赖项
项目使用 NuGet 包管理，主要依赖：
- `Markdig` - Markdown 解析
- `Markdig.Wpf` - WPF 渲染支持

## 后续可扩展功能

- [ ] 深色模式支持
- [ ] 文件编码自动检测
- [ ] 最近打开文件列表
- [ ] 书签功能
- [ ] 导出 PDF
- [ ] 自定义主题/字体设置
