using System.Windows.Controls;
using System.Windows.Navigation;

namespace JboxTransfer.Services.Contracts;

public interface INavigationService
{
    event NavigatedEventHandler Navigated;

    bool CanGoBack
    {
        get;
    }

    Frame Frame
    {
        get; set;
    }

    bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);

    bool GoBack();
}
