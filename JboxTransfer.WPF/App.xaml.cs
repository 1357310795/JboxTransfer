using JboxTransfer.Core;
using JboxTransfer.Helpers;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using JboxTransfer.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace JboxTransfer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitIoC();
            GlobalCookie.Read();
            GlobalSyncInfoService.Read();
            NetService.Init();
            GlobalSettings.Read();
            ThemeHelper.ApplyBase(GlobalSettings.Model.ThemeMode == 1);
            ThemeHelper.ChangeHue(GlobalSettings.Model.ThemeColor);
            base.OnStartup(e);
        }

        public void InitIoC()
        {
            IServiceCollection ioc = new ServiceCollection();
            ioc.AddSingleton<INavigationService, NavigationService>();
            ioc.AddSingleton<IPageService, PageService>();
            ioc.AddSingleton<ISnackBarService, SnackBarService>();

            ioc.AddSingleton<HomePage>();
            ioc.AddTransient<LoginPage>();
            ioc.AddSingleton<DebugPage>();
            ioc.AddSingleton<StartPage>();
            ioc.AddSingleton<ListPage>();
            ioc.AddSingleton<SettingsPage>();
            ioc.AddSingleton<AboutPage>();

            var serviceProvider = ioc.BuildServiceProvider();

            Services.ServiceProvider.Current = serviceProvider;
        }
    }

}
