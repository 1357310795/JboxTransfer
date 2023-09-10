using JboxTransfer.Core;
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
            NetService.Init();
            base.OnStartup(e);
        }

        public void InitIoC()
        {
            IServiceCollection ioc = new ServiceCollection();
            ioc.AddSingleton<INavigationService, NavigationService>();
            ioc.AddSingleton<IPageService, PageService>();

            ioc.AddSingleton<HomePage>();
            ioc.AddSingleton<LoginPage>();
            ioc.AddSingleton<DebugPage>();

            var serviceProvider = ioc.BuildServiceProvider();

            Services.ServiceProvider.Current = serviceProvider;
        }
    }

}
