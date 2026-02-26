# Markdown Reader

一个简洁优雅的 Markdown 文件阅读器，基于 WPF (.NET 10) 开发。

![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## 功能特性

### 📂 文件操作
- 支持打开 `.md` 和 `.markdown` 文件
- 快捷键 `Ctrl+O` 快速打开文件
- 一键刷新当前文件内容

### 📑 目录导航
- 自动解析文档标题结构（H1-H6）
- 侧边栏显示文档目录大纲
- 点击标题快速跳转到对应位置
- 支持隐藏/显示侧边栏

### 🔗 智能链接
- 按住 `Ctrl` 键点击链接：
  - **网页链接** → 自动打开浏览器
  - **Markdown 文件** → 在阅读器中打开
  - **其他文件** → 使用系统默认程序打开
- 支持相对路径解析

### 🎨 简洁界面
- 现代化蓝色主题设计
- 清晰的视觉层次
- 状态栏快捷操作按钮

## 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+O` | 打开文件 |
| `Ctrl+B` | 切换侧边栏 |
| `F5` | 刷新文件 |

## 系统要求

- Windows 10/11
- .NET 10.0 Runtime

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

## 技术栈

- **[WPF](https://docs.microsoft.com/dotnet/desktop/wpf/)** - Windows 桌面应用框架
- **[Markdig](https://github.com/xoofx/markdig)** - 快速、强大的 Markdown 解析库
- **[Markdig.Wpf](https://github.com/xoofx/markdig.wpf)** - Markdig 的 WPF 渲染扩展

## 项目结构

```
MarkdownReader/
├── MainWindow.xaml          # 主窗口界面
├── MainWindow.xaml.cs       # 主窗口逻辑
├── Models/
│   └── HeadingItem.cs       # 标题数据模型
├── Controls/
│   └── CustomMarkdownViewer.cs  # 自定义 Markdown 控件
└── App.xaml                 # 应用程序入口
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
│  就绪                        🔄📑 README.md  │
└──────────────────────────────────────────────┘
```

## 许可证

本项目采用 [MIT License](LICENSE) 开源协议。

## 贡献

欢迎提交 Issue 和 Pull Request！
