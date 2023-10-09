using JboxTransfer.Services;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JboxTransfer
{
    /// <summary>
    /// WebviewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WebviewWindow : Window
    {
        public WebviewWindow()
        {
            InitializeComponent();
        }

        int t = 0;

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri == "https://my.sjtu.edu.cn/ui/appmyinfo")
                t++;
            if (e.Uri == ("https://my.sjtu.edu.cn/ui/appmyinfo") && t == 2)
            {
                this.Dispatcher.Invoke(async() =>
                {
                    try
                    {
                        var cookieObj = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://jaccount.sjtu.edu.cn");
                        if (cookieObj != null)
                        {
                            var ja = cookieObj.FirstOrDefault(x => x.Name == "JAAuthCookie");
                            if (ja != null)
                            {
                                GlobalCookie.CookieContainer.Add(ja.ToSystemNetCookie());
                                this.DialogResult = true;
                                this.Close();
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    this.DialogResult = false;
                    this.Close();
                });
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async(await CoreWebView2Environment.CreateAsync());
                
            }
            catch (WebView2RuntimeNotFoundException)
            {
                MessageBox.Show($"未找到Webview2 runtime，请先安装Webview2 runtime");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化Webview2失败：{ex.Message}");
            }
            try
            {
                webView.CoreWebView2.CookieManager.DeleteAllCookies();
                webView.CoreWebView2.NavigationStarting += WebView_NavigationStarting;
                webView.CoreWebView2.Navigate("https://my.sjtu.edu.cn/ui/appmyinfo");
            }
            catch(Exception ex)
            {

            }
        }
    }
}
