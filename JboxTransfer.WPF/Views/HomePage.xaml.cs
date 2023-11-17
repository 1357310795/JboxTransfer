using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ServiceProvider = JboxTransfer.Services.ServiceProvider;

namespace JboxTransfer.Views
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class HomePage : UserControl, IRecipient<PageChangedMessage>
    {
        [ObservableProperty]
        private ImageSource avatarImage;

        [ObservableProperty]
        private string nickName;

        [ObservableProperty]
        private string selectedPage;

        INavigationService navigationService;

        public HomePage()
        {
            InitializeComponent();
            this.DataContext = this;
            navigationService = ServiceProvider.Current.GetService<INavigationService>();
            navigationService.Frame = this.MainFrame;
            WeakReferenceMessenger.Default.Register<PageChangedMessage>(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DbService.Init(UserInfoService.entity.AccountNo.Replace(".", "_"));
            DbService.db.Execute("UPDATE SyncTaskDbModel SET state = 0 WHERE state = 1;");
            Task.Run(GetAvatar);
            navigationService.NavigateTo(nameof(StartPage));
        }

        private void GetAvatar()
        {
            var res = TboxService.GetUserInfo();
            if (!res.Success)
            {
                //Todo:log
                return;
            }
            this.Dispatcher.Invoke(() => {
                try
                {
                    NickName = res.Result.First().OrgUser.Nickname;
                    if (res.Result.First().OrgUser.Avatar == null || res.Result.First().OrgUser.Avatar == "")
                        AvatarImage = (DrawingImage)App.Current.Resources["userDrawingImage"];
                    else
                    {
                        var avatarurl = "https:" + res.Result.First().OrgUser.Avatar;
                        Uri.TryCreate(avatarurl, UriKind.RelativeOrAbsolute, out var uri);
                        AvatarImage = new BitmapImage(uri);
                    }
                }
                catch (Exception ex)
                {
                    //Todo:log
                }
            });
            
        }

        partial void OnSelectedPageChanged(string value)
        {

        }

        public void Receive(PageChangedMessage message)
        {
            SelectedPage = message.Value;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var tag = (string)((FrameworkElement)sender).Tag;
            switch(tag)
            {
                case "Start":
                    navigationService.NavigateTo(nameof(StartPage));
                    break;
                case "List":
                    navigationService.NavigateTo(nameof(ListPage));
                    break;
                case "Settings":
                    navigationService.NavigateTo(nameof(SettingsPage));
                    break;
                case "Debug":
                    navigationService.NavigateTo(nameof(DebugPage));
                    break;
            }
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new UserLogoutMessage(0));
            var res = GlobalCookie.Default.Clear();
            if (!res.success)
            {
                MessageBox.Show("cookie.json 删除失败，您需要手动删除本程序所在文件夹内的 cookie.json 文件，然后重启程序");
                App.Current.Shutdown();
                return;
            }
            TboxAccessTokenKeeper.UnRegister();
            NetService.Init();
            this.Dispatcher.Invoke(() =>
            {
                var lw = new LoginWindow();
                lw.Show();
                App.Current.MainWindow.Close();
                App.Current.MainWindow = lw;
            });
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            navigationService.NavigateTo(nameof(AboutPage));
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = $"https://chat.sjtu.edu.cn/jboxtransfer",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void ButtonNoti_Click(object sender, RoutedEventArgs e)
        {
            navigationService.NavigateTo(nameof(NotiPage));
        }
    }
}
