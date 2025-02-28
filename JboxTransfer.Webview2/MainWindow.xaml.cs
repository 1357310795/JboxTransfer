using JboxTransfer.Core.Helpers;
using JboxTransfer.Server.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace JboxTransfer.Webview2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IHostApplicationLifetime _lifetime;
        public MainWindow(IHostApplicationLifetime lifetime)
        {
            InitializeComponent();
            _lifetime = lifetime;
        }

        private void webView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
             
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取程序集版本（示例用方法3）
            var assembly = typeof(MainWindow).Assembly;
            var versionAttribute = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            string displayVersion = versionAttribute != null
                ? $"v{versionAttribute.Version}"
                : "v1.0.0"; // 默认值

            // 设置窗口标题
            this.Title = $"JboxTransfer 桌面端 - {displayVersion}";

            var webView2Environment = await CoreWebView2Environment.CreateAsync(null, PathHelper.AppDataPath, new CoreWebView2EnvironmentOptions()
            {
                AdditionalBrowserArguments = ""
            });
            await webView.EnsureCoreWebView2Async(webView2Environment);

            _lifetime.ApplicationStarted.Register(() =>
            {
                var local_url = $"http://localhost:{GlobalConfigService.Config.ServerConfig.Port}";
                webView.Source = new Uri(local_url);
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var res = MessageBox.Show("确定要退出吗？如果您有任务正在传输，任务将被停止。", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                e.Cancel = false;
                App.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}