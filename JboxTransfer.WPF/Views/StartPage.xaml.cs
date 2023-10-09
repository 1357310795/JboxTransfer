using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JboxTransfer.Views
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class StartPage : Page
    {
        [ObservableProperty]
        private bool isProgressShow;

        ISnackBarService snackBarService;

        public StartPage(ISnackBarService snackBarService)
        {
            InitializeComponent();
            this.DataContext = this;
            this.snackBarService = snackBarService;
        }

        public void AddSyncItem(string path)
        {
            var res = JboxService.GetJboxFileInfo(path);
            if (!res.Success)
            {
                snackBarService.MessageQueue.Enqueue($"获取文件信息失败：{res.Message}");
                return;
            }

            var order = DbService.GetMinOrder() - 1;
            DbService.db.Insert(new SyncTaskDbModel(res.Result.IsDir ? 1 : 0, res.Result.Path, res.Result.Bytes, order) { MD5_Ori = res.Result.Hash });

            snackBarService.MessageQueue.Enqueue("添加任务成功！");
            //Todo:开始任务

            //IsProgressShow = true;
        }

        private async void ButtonFull_Click(object sender, RoutedEventArgs e)
        {
            var res = await DialogService.SelectSyncPath();
            if (!res.Success)
                return;
            //this.Dispatcher.Invoke(() => {
            //    MessageBox.Show(res.Result);
            //});
            AddSyncItem(res.Result);
        }

        private void ButtonIncre_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
