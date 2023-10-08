using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
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
using ServiceProvider = JboxTransfer.Services.ServiceProvider;

namespace JboxTransfer.Views
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class HomePage : UserControl
    {
        [ObservableProperty]
        private ImageSource avatarImage;

        [ObservableProperty]
        private string nickName;

        INavigationService navigationService;

        public HomePage()
        {
            InitializeComponent();
            this.DataContext = this;
            navigationService = ServiceProvider.Current.GetService<INavigationService>();
            navigationService.Frame = this.MainFrame;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DbService.Init();
            DbService.db.Execute("UPDATE SyncTaskDbModel SET state = 0 WHERE state = 1;");
            Task.Run(GetAvatar);
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
                var avatarurl = "https:" + res.Result.First().OrgUser.Avatar;
                if (!Uri.TryCreate(avatarurl, UriKind.RelativeOrAbsolute, out var uri))
                    AvatarImage = (DrawingImage)App.Current.Resources["userDrawingImage"];
                else
                    AvatarImage = new BitmapImage(uri);
                NickName = res.Result.First().OrgUser.Nickname;
            });
            
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
            var res = GlobalCookie.Clear();
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
    }
}
