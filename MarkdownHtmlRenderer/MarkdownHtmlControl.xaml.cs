using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using Markdig;

namespace MarkdownHtmlRenderer;

/// <summary>
/// 基于 WebView2 的 Markdown 渲染控件
/// </summary>
public partial class MarkdownHtmlControl : UserControl
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoLinks()
        .UseTaskLists()
        .UseAutoIdentifiers()
        .Build();

    private bool _isInitialized;
    private string? _pendingMarkdown;
    private string? _currentDirectory;

    #region 依赖属性

    public static readonly DependencyProperty MarkdownProperty =
        DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownHtmlControl),
            new PropertyMetadata(null, OnMarkdownChanged));

    public static readonly DependencyProperty BaseDirectoryProperty =
        DependencyProperty.Register(nameof(BaseDirectory), typeof(string), typeof(MarkdownHtmlControl),
            new PropertyMetadata(null, OnBaseDirectoryChanged));

    public static readonly DependencyProperty LinkClickCommandProperty =
        DependencyProperty.Register(nameof(LinkClickCommand), typeof(Action<string>), typeof(MarkdownHtmlControl),
            new PropertyMetadata(null));

    #endregion

    #region 属性

    /// <summary>
    /// 要渲染的 Markdown 文本
    /// </summary>
    public string? Markdown
    {
        get => (string?)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    /// <summary>
    /// 资源基础目录（用于解析相对路径的图片和链接）
    /// </summary>
    public string? BaseDirectory
    {
        get => (string?)GetValue(BaseDirectoryProperty);
        set => SetValue(BaseDirectoryProperty, value);
    }

    /// <summary>
    /// 链接点击回调
    /// </summary>
    public Action<string>? LinkClickCommand
    {
        get => (Action<string>?)GetValue(LinkClickCommandProperty);
        set => SetValue(LinkClickCommandProperty, value);
    }

    #endregion

    public MarkdownHtmlControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await InitializeWebView();
    }

    private async Task InitializeWebView()
    {
        if (_isInitialized) return;

        try
        {
            await WebView.EnsureCoreWebView2Async(null);
            
            // 注册 JavaScript 消息处理
            WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            
            // 注入 JavaScript 以捕获链接点击
            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                    document.addEventListener('click', function(e) {
                        var target = e.target;
                        while (target && target.tagName !== 'A') {
                            target = target.parentElement;
                        }
                        if (target && target.tagName === 'A') {
                            var href = target.getAttribute('href');
                            if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                                e.preventDefault();
                                window.chrome.webview.postMessage({type: 'linkClick', url: href});
                            }
                        }
                    }, true);
                ");

            _isInitialized = true;

            // 渲染等待中的 Markdown
            if (_pendingMarkdown != null)
            {
                await RenderMarkdownAsync(_pendingMarkdown);
                _pendingMarkdown = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebView2 初始化失败: {ex.Message}");
        }
    }

    private void OnWebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var message = e.WebMessageAsJson;
            if (message.Contains("linkClick"))
            {
                var url = ExtractUrlFromMessage(message);
                if (!string.IsNullOrEmpty(url))
                {
                    LinkClickCommand?.Invoke(url);
                }
            }
        }
        catch { }
    }

    private static string? ExtractUrlFromMessage(string message)
    {
        // 简单解析 JSON 获取 URL
        var urlStart = message.IndexOf("\"url\":\"");
        if (urlStart < 0) return null;
        
        urlStart += 7;
        var urlEnd = message.IndexOf("\"", urlStart);
        if (urlEnd < 0) return null;
        
        var url = message.Substring(urlStart, urlEnd - urlStart);
        // 处理转义字符
        return url.Replace("\\/", "/").Replace("\\\"", "\"").Replace("\\\\", "\\");
    }

    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownHtmlControl control)
        {
            control.UpdateContent();
        }
    }

    private static void OnBaseDirectoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownHtmlControl control)
        {
            control._currentDirectory = e.NewValue as string;
        }
    }

    private async void UpdateContent()
    {
        if (!_isInitialized)
        {
            _pendingMarkdown = Markdown;
            return;
        }

        if (Markdown != null)
        {
            await RenderMarkdownAsync(Markdown);
        }
        else
        {
            await RenderHtmlAsync(GetEmptyHtml());
        }
    }

    public async Task RenderMarkdownAsync(string markdown)
    {
        var html = Markdig.Markdown.ToHtml(markdown, _pipeline);
        
        // 将相对路径的图片和链接转换为虚拟主机路径
        // 并对 URL 编码的中文路径进行解码
        html = ProcessLocalUrls(html);
        
        var fullHtml = WrapHtml(html);
        await RenderHtmlAsync(fullHtml);
    }

    /// <summary>
    /// 处理 HTML 中的本地 URL，将相对路径转换为虚拟主机路径，并解码 URL 编码的字符
    /// </summary>
    private string ProcessLocalUrls(string html)
    {
        if (string.IsNullOrEmpty(_currentDirectory))
            return html;

        // 处理图片 src 属性
        html = Regex.Replace(html, @"(<img[^>]+src=[""'])([^""']+)([""'])", match =>
        {
            var prefix = match.Groups[1].Value;
            var src = match.Groups[2].Value;
            var suffix = match.Groups[3].Value;

            // 解码 URL 编码的字符（如中文）
            src = HttpUtility.UrlDecode(src);

            // 如果是相对路径，转换为虚拟主机路径
            if (!IsAbsoluteUrl(src))
            {
                src = $"http://local.markdown/{src}";
            }

            return $"{prefix}{src}{suffix}";
        });

        // 处理链接 href 属性
        html = Regex.Replace(html, @"(<a[^>]+href=[""'])([^""']+)([""'])", match =>
        {
            var prefix = match.Groups[1].Value;
            var href = match.Groups[2].Value;
            var suffix = match.Groups[3].Value;

            // 解码 URL 编码的字符
            href = HttpUtility.UrlDecode(href);

            // 如果是相对路径且不是锚点，转换为虚拟主机路径
            if (!IsAbsoluteUrl(href) && !href.StartsWith("#"))
            {
                href = $"http://local.markdown/{href}";
            }

            return $"{prefix}{href}{suffix}";
        });

        return html;
    }

    /// <summary>
    /// 检查是否为绝对 URL
    /// </summary>
    private static bool IsAbsoluteUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("file://", StringComparison.OrdinalIgnoreCase);
    }

    private async Task RenderHtmlAsync(string html)
    {
        if (!_isInitialized || WebView.CoreWebView2 == null) return;

        _currentDirectory = BaseDirectory;
        
        // 设置虚拟主机目录以支持本地文件访问
        if (!string.IsNullOrEmpty(_currentDirectory) && Directory.Exists(_currentDirectory))
        {
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "local.markdown",
                _currentDirectory,
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
        }

        WebView.NavigateToString(html);
    }

    private void WebView_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        // 导航完成后的处理
    }

    private static string GetEmptyHtml()
    {
        return WrapHtml("<div style='color: #999; text-align: center; padding: 50px;'>打开 Markdown 文件开始阅读</div>");
    }

    private static string WrapHtml(string contentHtml)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        :root {{
            --bg-color: #ffffff;
            --text-color: #24292f;
            --code-bg: #f6f8fa;
            --border-color: #d0d7de;
            --link-color: #0969da;
            --quote-color: #57606a;
        }}
        
        @media (prefers-color-scheme: dark) {{
            :root {{
                --bg-color: #0d1117;
                --text-color: #c9d1d9;
                --code-bg: #161b22;
                --border-color: #30363d;
                --link-color: #58a6ff;
                --quote-color: #8b949e;
            }}
        }}
        
        * {{
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
            font-size: 16px;
            line-height: 1.6;
            color: var(--text-color);
            background-color: var(--bg-color);
            margin: 0;
            padding: 20px;
            max-width: 900px;
            margin: 0 auto;
        }}
        
        h1, h2, h3, h4, h5, h6 {{
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
        }}
        
        h1 {{ font-size: 2em; border-bottom: 1px solid var(--border-color); padding-bottom: 0.3em; }}
        h2 {{ font-size: 1.5em; border-bottom: 1px solid var(--border-color); padding-bottom: 0.3em; }}
        h3 {{ font-size: 1.25em; }}
        h4 {{ font-size: 1em; }}
        
        p {{
            margin-top: 0;
            margin-bottom: 16px;
        }}
        
        a {{
            color: var(--link-color);
            text-decoration: none;
        }}
        
        a:hover {{
            text-decoration: underline;
        }}
        
        code {{
            padding: 0.2em 0.4em;
            margin: 0;
            font-size: 85%;
            background-color: var(--code-bg);
            border-radius: 6px;
            font-family: ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, monospace;
        }}
        
        pre {{
            padding: 16px;
            overflow: auto;
            font-size: 85%;
            line-height: 1.45;
            background-color: var(--code-bg);
            border-radius: 6px;
            margin-top: 0;
            margin-bottom: 16px;
        }}
        
        pre code {{
            background-color: transparent;
            padding: 0;
        }}
        
        blockquote {{
            margin: 0 0 16px;
            padding: 0 1em;
            color: var(--quote-color);
            border-left: 0.25em solid var(--border-color);
        }}
        
        ul, ol {{
            margin-top: 0;
            margin-bottom: 16px;
            padding-left: 2em;
        }}
        
        li {{
            margin-top: 0.25em;
        }}
        
        table {{
            border-spacing: 0;
            border-collapse: collapse;
            margin-bottom: 16px;
            width: 100%;
        }}
        
        table th, table td {{
            padding: 6px 13px;
            border: 1px solid var(--border-color);
        }}
        
        table th {{
            font-weight: 600;
            background-color: var(--code-bg);
        }}
        
        table tr:nth-child(2n) {{
            background-color: var(--code-bg);
        }}
        
        img {{
            max-width: 100%;
            height: auto;
            border-radius: 6px;
        }}
        
        hr {{
            height: 0.25em;
            padding: 0;
            margin: 24px 0;
            background-color: var(--border-color);
            border: 0;
        }}
        
        /* 任务列表 */
        .task-list-item {{
            list-style-type: none;
        }}
        
        input[type=""checkbox""] {{
            margin-right: 0.5em;
        }}
    </style>
</head>
<body>
{contentHtml}
</body>
</html>";
    }

    /// <summary>
    /// 滚动到顶部
    /// </summary>
    public async void ScrollToTop()
    {
        if (_isInitialized && WebView.CoreWebView2 != null)
        {
            await WebView.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);");
        }
    }

    /// <summary>
    /// 滚动到底部
    /// </summary>
    public async void ScrollToEnd()
    {
        if (_isInitialized && WebView.CoreWebView2 != null)
        {
            await WebView.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, document.body.scrollHeight);");
        }
    }

    /// <summary>
    /// 滚动到指定锚点
    /// </summary>
    public async void ScrollToAnchor(string anchor)
    {
        if (_isInitialized && WebView.CoreWebView2 != null && !string.IsNullOrEmpty(anchor))
        {
            // 先尝试通过 ID 查找
            var script = $@"
                    (function() {{
                        var element = document.getElementById('{anchor}');
                        if (element) {{
                            element.scrollIntoView({{ behavior: 'smooth', block: 'start' }});
                            return true;
                        }}
                        // 如果找不到 ID，尝试通过文本查找标题
                        var headings = document.querySelectorAll('h1, h2, h3, h4, h5, h6');
                        for (var h of headings) {{
                            if (h.textContent.trim() === decodeURIComponent('{Uri.EscapeDataString(anchor)}') ||
                                h.id.includes('{anchor}')) {{
                                h.scrollIntoView({{ behavior: 'smooth', block: 'start' }});
                                return true;
                            }}
                        }}
                        return false;
                    }})();";
            await WebView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }

    /// <summary>
    /// 通过标题文本滚动到对应位置
    /// </summary>
    public async void ScrollToHeadingText(string headingText)
    {
        if (_isInitialized && WebView.CoreWebView2 != null && !string.IsNullOrEmpty(headingText))
        {
            var escapedText = headingText.Replace("\\", "\\\\").Replace("'", "\\'");
            var script = $@"
                    var headings = document.querySelectorAll('h1, h2, h3, h4, h5, h6');
                    for (var h of headings) {{
                        if (h.textContent.trim() === '{escapedText}') {{
                            h.scrollIntoView({{ behavior: 'smooth', block: 'start' }});
                            break;
                        }}
                    }}";
            await WebView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
