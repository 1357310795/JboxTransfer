﻿using JboxTransfer.Core;
using JboxTransfer.Core.Services;
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
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            InitIoC();
            GlobalCookie.Default.Read();
            GlobalSyncInfoService.Default.Read();
            NetService.Init();
            GlobalSettings.Default.Read();
            ThemeHelper.ApplyBase(GlobalSettings.Default.Model.ThemeMode == 1);
            ThemeHelper.ChangeHue(GlobalSettings.Default.Model.ThemeColor);
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"出现未经处理的异常，为保护您的数据，程序将自动关闭。\n{e.Exception}");
            this.Shutdown();
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
            ioc.AddSingleton<DbOpPage>();
            ioc.AddSingleton<NotiPage>();

            var serviceProvider = ioc.BuildServiceProvider();

            Services.ServiceProvider.Current = serviceProvider;
        }
    }

}
