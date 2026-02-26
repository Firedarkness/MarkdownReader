namespace MarkdownReader.Models;

/// <summary>
/// 表示Markdown文档中的一个标题项
/// </summary>
public class HeadingItem
{
    /// <summary>
    /// 标题级别 (1-6)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 标题文本内容
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 标题的锚点ID (用于跳转)
    /// </summary>
    public string Anchor { get; set; } = string.Empty;

    /// <summary>
    /// 用于显示的缩进文本
    /// </summary>
    public string IndentedText => new string(' ', (Level - 1) * 2) + Text;
}
