using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Helpers;
using JboxTransfer.Services;
using JboxTransfer.ViewModels;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using Teru.Code.Models;

namespace JboxTransfer.Views.Dialogs
{
    /// <summary>
    /// SelectJboxItemDialog.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class SelectJboxItemDialog : UserControl
    {
        public SelectJboxItemDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        [ObservableProperty]
        private List<JboxItemViewModel> items;

        [ObservableProperty]
        private string selectedPath;

        [ObservableProperty]
        private bool isError;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string message;

        private string listPath = "/";

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { LoadDir("/"); });
        }

        private CommonResult LoadDir(string path)
        {
            this.Dispatcher.Invoke(() => { IsBusy = true; IsError = false; });
            listPath = path;
            var res = JboxService.GetJboxFolderInfo(path, 0);
            this.Dispatcher.Invoke(() => { IsBusy = false; });
            if (!res.Success)
            {
                Items = null;
                IsError = true;
                Message = res.Message;
                return new CommonResult(false, res.Message);
            }
            SelectedPath = path;

            this.Dispatcher.Invoke(() =>
            {
                List<JboxItemViewModel> newlist = new List<JboxItemViewModel>();
                foreach (var item in res.Result.Content)
                {
                    newlist.Add(new JboxItemViewModel()
                    {
                        Icon = item.IsDir ? IconHelper.FindIconForFolder(true) :
                                            IconHelper.FindIconForFilename(item.Path.PathToName(), true),
                        Info = item
                    });
                }
                Items = newlist;
            });
            
            return new CommonResult(true, "");
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close(DialogService.DialogIdentifier, new CommonResult<string>(true, "", SelectedPath));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close(DialogService.DialogIdentifier, new CommonResult<string>(false, "已取消"));
        }

        [RelayCommand]
        private void ItemDoubleClick(object sender)
        {
            JboxItemViewModel vm = sender as JboxItemViewModel;
            if (vm == null)
                return;
            if (vm.Info.IsDir)
                Task.Run(() => { LoadDir(vm.Info.Path); });
        }

        [RelayCommand]
        private void ItemClick(object sender)
        {
            JboxItemViewModel vm = sender as JboxItemViewModel;
            if (vm == null)
                return;
            SelectedPath = vm.Info.Path;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;
            JboxItemViewModel vm = e.AddedItems[0] as JboxItemViewModel;
            if (vm == null) 
                return;
            SelectedPath = vm.Info.Path;
        }

        private void ButtonRetry_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { LoadDir(listPath); });
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { LoadDir(listPath.GetParentPath()); });
        }

        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { LoadDir("/"); });
        }
    }
}
