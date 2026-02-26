using System.Windows;
using System.Windows.Documents;
using Markdig.Wpf;

namespace MarkdownReader.Controls;

/// <summary>
/// 自定义MarkdownViewer，支持Ctrl+点击链接功能
/// </summary>
public class CustomMarkdownViewer : MarkdownViewer
{
    private MainWindow? _mainWindow;

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        if (_mainWindow == null)
        {
            _mainWindow = Window.GetWindow(this) as MainWindow;
            if (_mainWindow != null)
            {
                SubscribeToHyperlinks();
            }
        }
    }

    private void SubscribeToHyperlinks()
    {
        // 使用路由事件监听超链接点击
        AddHandler(Hyperlink.RequestNavigateEvent, new RoutedEventHandler(OnHyperlinkClick));
    }

    private void OnHyperlinkClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Hyperlink hyperlink && hyperlink.NavigateUri != null)
        {
            var url = hyperlink.NavigateUri.ToString();
            _mainWindow?.HandleHyperlinkClick(url);
            e.Handled = true;
        }
    }
}
