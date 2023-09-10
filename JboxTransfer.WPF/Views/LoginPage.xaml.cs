using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Helpers;
using JboxTransfer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZXing.QrCode;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Diagnostics;
using JboxTransfer.Services.Contracts;

namespace JboxTransfer.Views
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class LoginPage : Page
    {
        INavigationService navigationService;
        public LoginPage(INavigationService navigationService)
        {
            InitializeComponent();
            this.DataContext = this;
            this.navigationService = navigationService;
        }

        JacFastLoginHelper helper;

        [ObservableProperty]
        private BitmapSource imageSource;

        [ObservableProperty]
        private string message = "请稍后";

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ImageSource = CreateQRCode("Loading...");
            helper = new JacFastLoginHelper(GlobalCookie.CookieContainer, "https://my.sjtu.edu.cn/ui/appmyinfo");
            helper.Prepared += Helper_Prepared;
            helper.LoginFail += Helper_LoginFail;
            helper.LoginSuccess += Helper_LoginSuccess;
            helper.Start();
        }

        private void Helper_LoginSuccess()
        {
            this.Dispatcher.Invoke(() =>
            {
                navigationService.NavigateTo(nameof(HomePage));
            });
            GlobalCookie.Save();
            //Debug.WriteLine(GlobalCookie.HasJacCookie());
        }

        private void Helper_LoginFail(string message)
        {
            this.Dispatcher.Invoke(() => {
                MaskBorder.Visibility = Visibility.Visible;
                Message = $"登录失败，点击以重试\n{message}";
            });
        }

        private void Helper_Prepared()
        {
            this.Dispatcher.Invoke(() => {
                MaskBorder.Visibility = Visibility.Hidden;
                ImageSource = CreateQRCode(helper.GetQrcodeStr());
            });
        }

        public static BitmapSource CreateQRCode(string sContent, int width = 300, int height = 300)
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new QrCodeEncodingOptions()
            {
                CharacterSet = "UTF-8",
                Margin = 1,
                DisableECI = true,
                Height = height,
                Width = width,
            };

            var bitmap = writer.Write(sContent);

            return bitmap.ToBitmapSource();
        }

        private void MaskBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (helper.Failed)
            {
                helper.Refresh();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            helper.Dispose();
        }
    }
}
