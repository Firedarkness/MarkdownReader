# Markdown Reader

一个简洁优雅的 Markdown 文件阅读器，基于 WPF (.NET 10) 开发。

![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## 功能特性

### 📂 文件操作
- 支持打开 `.md` 和 `.markdown` 文件
- 快捷键 `Ctrl+O` 快速打开文件
- 文件关联：双击 .md 文件直接打开
- 一键刷新当前文件内容

### 📑 目录导航
- 自动解析文档标题结构（H1-H6）
- 侧边栏显示文档目录大纲
- 点击标题快速跳转到对应位置
- 支持隐藏/显示侧边栏

### 🔗 智能链接
- 点击链接自动处理：
  - **网页链接** → 自动打开浏览器
  - **Markdown 文件** → 在阅读器中打开
  - **其他文件** → 使用系统默认程序打开
- 支持相对路径解析

### 🖼️ 完美渲染
- 基于 WebView2 的 HTML 渲染引擎
- 支持所有标准 Markdown 语法
- 支持本地图片显示（包括中文文件名）
- 自动暗色模式（跟随系统主题）

### 🎨 简洁界面
- 现代化蓝色主题设计
- 清晰的视觉层次
- 状态栏快捷操作按钮

## 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+O` | 打开文件 |
| `Ctrl+B` | 切换侧边栏 |

## 系统要求

- Windows 10/11
- .NET 10.0 Runtime
- WebView2 Runtime (Windows 10/11 自带)

## 安装与运行

### 从源码运行

```bash
# 克隆仓库
git clone <repository-url>
cd MarkdownReader

# 还原依赖
dotnet restore

# 运行项目
dotnet run
```

### 编译发布

```bash
# 编译 Release 版本
dotnet build -c Release

# 发布为独立应用
dotnet publish -c Release -r win-x64 --self-contained
```

### 文件关联

要在 Windows 中将 .md 文件关联到此程序：

**方法一：图形界面**
1. 右键任意 .md 文件 → "打开方式" → "选择其他应用"
2. 点击"更多应用" → 找到 MarkdownReader.exe
3. 勾选"始终使用此应用打开 .md 文件"
4. 点击"确定"

**方法二：命令行（管理员权限）**
```cmd
assoc .md=MarkdownFile
ftype MarkdownFile="C:\path\to\MarkdownReader.exe" "%1"
```

## 技术栈

- **[WPF](https://docs.microsoft.com/dotnet/desktop/wpf/)** - Windows 桌面应用框架
- **[Markdig](https://github.com/xoofx/markdig)** - 快速、强大的 Markdown 解析库
- **[WebView2](https://docs.microsoft.com/microsoft-edge/webview2/)** - 微软 Edge WebView2 控件

## 项目结构

```
MarkdownReader/
├── MarkdownReader/
│   ├── App.xaml / App.xaml.cs       # 应用程序入口
│   ├── MainWindow.xaml             # 主窗口界面
│   ├── MainWindow.xaml.cs          # 主窗口逻辑
│   └── Models/
│       └── HeadingItem.cs           # 标题数据模型
├── MarkdownHtmlRenderer/
│   ├── MarkdownHtmlControl.xaml     # 自定义 Markdown 渲染控件
│   ├── MarkdownHtmlControl.xaml.cs  # 控件逻辑
│   └── MarkdownHtmlRenderer.csproj  # 类库项目配置
└── MarkdownReader.slnx              # 解决方案文件
```

## 截图预览

```
┌──────────────────────────────────────────────┐
│  文件  视图  帮助                             │
├──────────────────────────────────────────────┤
│  ┌─────────┐  ┌────────────────────────────┐ │
│  │ 📑 目录 │  │                            │ │
│  │         │  │    Markdown 预览内容       │ │
│  │  标题1  │  │                            │ │
│  │    标题2│  │    支持：                  │ │
│  │  标题3  │  │    - 标题样式              │ │
│  │         │  │    - 列表                  │ │
│  │         │  │    - 代码块                │ │
│  └─────────┘  │    - 表格 等...            │ │
│               └────────────────────────────┘ │
├──────────────────────────────────────────────┤
│  就绪                        ◀🔄 README.md  │
└──────────────────────────────────────────────┘
```

## 核心功能说明

### WebView2 渲染
使用 WebView2 控件渲染 HTML，相比传统 WPF 控件：
- 更好的 CSS 样式支持
- 支持暗色模式
- 更准确的 Markdown 渲染
- 支持复杂表格、代码高亮等

### 虚拟主机映射
通过 WebView2 的虚拟主机功能，安全地加载本地资源：
- 图片支持相对路径
- 中文文件名自动解码
- 链接智能识别和处理

## 许可证

本项目采用 [MIT License](LICENSE) 开源协议。

## 贡献

欢迎提交 Issue 和 Pull Request！

### 开发指南
1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request
