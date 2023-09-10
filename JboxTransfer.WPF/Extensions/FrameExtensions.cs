
using System.Windows.Controls;

namespace JboxTransfer.Extensions;

public static class FrameExtensions
{
    public static object GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty("DataContext")?.GetValue(frame.Content, null);
}
