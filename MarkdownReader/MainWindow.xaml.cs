using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Markdig;
using Markdig.Syntax;
using Microsoft.Win32;
using MarkdownReader.Models;
using System.Windows.Input;

namespace MarkdownReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string? _currentFilePath;
    private string? _currentDirectory;

    public ObservableCollection<HeadingItem> Headings { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Markdown文件 (*.md;*.markdown)|*.md;*.markdown|所有文件 (*.*)|*.*",
            Title = "打开Markdown文件",
            CheckFileExists = true,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            LoadMarkdownFile(dialog.FileName);
        }
    }

    private void LoadMarkdownFile(string filePath)
    {
        try
        {
            var markdown = File.ReadAllText(filePath);
            var directory = Path.GetDirectoryName(filePath);
            
            // 设置基础目录（用于解析相对路径图片）
            MarkdownViewer.BaseDirectory = directory;
            MarkdownViewer.Markdown = markdown;
            MarkdownViewer.LinkClickCommand = HandleHyperlinkClick;
            
            _currentFilePath = filePath;
            _currentDirectory = directory;

            // 解析标题
            ParseHeadings(markdown);

            // 显示按钮
            RefreshButton.Visibility = Visibility.Visible;
            ToggleSidebarButton.Visibility = Visibility.Visible;

            // 更新状态栏
            var fileInfo = new FileInfo(filePath);
            StatusText.Text = "已加载";
            FileNameText.Text = $"{fileInfo.Name} | {FormatFileSize(fileInfo.Length)}";

            // 更新窗口标题
            Title = $"Markdown Reader - {fileInfo.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开文件: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ParseHeadings(string markdown)
    {
        Headings.Clear();

        try
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .Build();
            var document = Markdig.Markdown.Parse(markdown, pipeline);

            foreach (var block in document.Descendants<HeadingBlock>())
            {
                var heading = new HeadingItem
                {
                    Level = block.Level,
                    Text = block.Inline?.FirstChild?.ToString() ?? ""
                };
                Headings.Add(heading);
            }
        }
        catch
        {
            // 解析失败时忽略
        }
    }

    private void Heading_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is HeadingItem heading)
        {
            // 使用标题文本进行跳转
            MarkdownViewer.ScrollToHeadingText(heading.Text);
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:0.##} {suffixes[i]}";
    }

    private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
        {
            LoadMarkdownFile(_currentFilePath);
            StatusText.Text = "已刷新";
        }
        else
        {
            MessageBox.Show("请先打开一个Markdown文件", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        if (SidebarColumn.Width.Value > 0)
        {
            SidebarColumn.Width = new GridLength(0);
            ToggleSidebarButton.Content = "◀";
        }
        else
        {
            SidebarColumn.Width = new GridLength(220);
            ToggleSidebarButton.Content = "▶";
        }
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            @"Markdown Reader v1.0

一个简洁的Markdown文件阅读器

使用技术:
- WPF (.NET 10)
- Markdig
- WebView2",
            "关于",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ScrollToTop_Click(object sender, RoutedEventArgs e)
    {
        MarkdownViewer.ScrollToTop();
    }

    private void ScrollToEnd_Click(object sender, RoutedEventArgs e)
    {
        MarkdownViewer.ScrollToEnd();
    }

    /// <summary>
    /// 处理超链接点击
    /// </summary>
    private void HandleHyperlinkClick(string url)
    {
        try
        {
            // 判断是否为Markdown文件链接
            string? filePath = null;

            // 处理虚拟主机路径 (http://local.markdown/...)
            if (url.StartsWith("http://local.markdown/", StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = url.Substring("http://local.markdown/".Length);
                if (_currentDirectory != null)
                {
                    filePath = Path.GetFullPath(Path.Combine(_currentDirectory, relativePath));
                }
            }
            // 处理相对路径
            else if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // 相对路径，基于当前文件目录解析
                if (_currentDirectory != null)
                {
                    filePath = Path.GetFullPath(Path.Combine(_currentDirectory, url));
                }
            }
            else if (uri.IsFile)
            {
                filePath = uri.LocalPath;
            }
            else if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                // 网页链接，打开浏览器
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return;
            }

            // 检查是否为Markdown文件
            if (filePath != null && File.Exists(filePath))
            {
                var ext = Path.GetExtension(filePath).ToLowerInvariant();
                if (ext == ".md" || ext == ".markdown")
                {
                    LoadMarkdownFile(filePath);
                    return;
                }
                else
                {
                    // 其他文件类型，使用系统默认程序打开
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                    return;
                }
            }

            // 其他情况，尝试作为URL打开
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开链接: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
