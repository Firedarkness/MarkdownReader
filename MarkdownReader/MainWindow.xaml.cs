using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Markdig;
using Markdig.Syntax;
using Microsoft.Win32;
using MarkdownReader.Models;

namespace MarkdownReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string? _currentFilePath;
    private string? _currentDirectory;
    private bool _isCtrlPressed;

    public ObservableCollection<HeadingItem> Headings { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        
        // 监听键盘事件
        KeyDown += MainWindow_KeyDown;
        KeyUp += MainWindow_KeyUp;
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlPressed = true;
            StatusText.Text = "按住Ctrl并点击链接可打开";
        }
    }

    private void MainWindow_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlPressed = false;
            StatusText.Text = string.IsNullOrEmpty(_currentFilePath) ? "就绪" : "已加载";
        }
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
            MarkdownViewer.Markdown = markdown;
            _currentFilePath = filePath;
            _currentDirectory = Path.GetDirectoryName(filePath);

            // 解析标题
            ParseHeadings(markdown);

            // 显示侧边栏
            SidebarColumn.Width = new GridLength(0);
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
            var pipeline = new MarkdownPipelineBuilder().UseAutoLinks().Build();
            var document = Markdig.Markdown.Parse(markdown, pipeline);

            foreach (var block in document.Descendants<HeadingBlock>())
            {
                var heading = new HeadingItem
                {
                    Level = block.Level,
                    Text = block.Inline?.FirstChild?.ToString() ?? "",
                    Anchor = $"heading-{Headings.Count}"
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
            // 滚动到对应标题位置
            ScrollToHeading(heading.Text);
        }
    }

    private void ScrollToHeading(string headingText)
    {
        if (MarkdownViewer.Document == null) return;

        // 在FlowDocument中查找包含标题文本的段落
        var scrollViewer = FindScrollViewer(MarkdownViewer);
        if (scrollViewer == null) return;

        // 遍历文档内容查找标题
        foreach (var block in MarkdownViewer.Document.Blocks)
        {
            if (block is Section section)
            {
                foreach (var sectionBlock in section.Blocks)
                {
                    if (sectionBlock is Paragraph para)
                    {
                        var text = new TextRange(para.ContentStart, para.ContentEnd).Text;
                        if (text.Contains(headingText))
                        {
                            sectionBlock.BringIntoView();
                            return;
                        }
                    }
                }
            }
            else if (block is Paragraph paragraph)
            {
                var text = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;
                if (text.Contains(headingText))
                {
                    block.BringIntoView();
                    return;
                }
            }
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
- Markdig.Wpf",
            "关于",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ScrollToTop_Click(object sender, RoutedEventArgs e)
    {
        if (MarkdownViewer.Document != null)
        {
            var scrollViewer = FindScrollViewer(MarkdownViewer);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
            }
        }
    }

    private void ScrollToEnd_Click(object sender, RoutedEventArgs e)
    {
        if (MarkdownViewer.Document != null)
        {
            var scrollViewer = FindScrollViewer(MarkdownViewer);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        if (parent is ScrollViewer scrollViewer)
            return scrollViewer;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindScrollViewer(child);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// 处理MarkdownViewer中的超链接点击
    /// </summary>
    public void HandleHyperlinkClick(string url)
    {
        if (!_isCtrlPressed)
            return;

        try
        {
            // 判断是否为Markdown文件链接
            string? filePath = null;

            // 处理相对路径
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
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
