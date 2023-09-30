using JboxTransfer.Core.Helpers;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using JboxTransfer.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NavigationService = JboxTransfer.Services.NavigationService;
using ServiceProvider = JboxTransfer.Services.ServiceProvider;

namespace JboxTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        INavigationService navigationService;
        ISnackBarService snackBarService;
        public MainWindow()
        {
            InitializeComponent();
            //navigationService = ServiceProvider.Current.GetService<INavigationService>();
            //navigationService.Frame = this.MainFrame;
            var snackBarService = ServiceProvider.Current.GetService<ISnackBarService>() as SnackBarService;
            snackBarService.MessageQueue = new SnackbarMessageQueue();
            SnackbarOne.MessageQueue = (SnackbarMessageQueue)snackBarService.MessageQueue;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //navigationService.NavigateTo(nameof(HomePage));
            //if (GlobalCookie.HasJacCookie())
            //    navigationService.NavigateTo(nameof(DebugPage));
            //else
            //    navigationService.NavigateTo(nameof(LoginPage));
        }
    }
}