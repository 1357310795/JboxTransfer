using JboxTransfer.Services.Contracts;
using JboxTransfer.Views;

namespace JboxTransfer.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    private static PageService _default;
    public static PageService Default
    {
        get
        {
            if (_default == null)
                _default = new PageService();
            return _default;
        }
    }

    public PageService()
    {
        _pages.Add(nameof(LoginPage), typeof(LoginPage));
        _pages.Add(nameof(HomePage), typeof(HomePage));
        _pages.Add(nameof(DebugPage), typeof(DebugPage));
        _pages.Add(nameof(StartPage), typeof(StartPage));
        _pages.Add(nameof(ListPage), typeof(ListPage));
        _pages.Add(nameof(SettingsPage), typeof(SettingsPage));
    }

    public Type GetPageType(string key)
    {
        Type pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }
}
