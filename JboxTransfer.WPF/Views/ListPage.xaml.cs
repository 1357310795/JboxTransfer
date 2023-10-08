using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JboxTransfer.Helpers;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using JboxTransfer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Teru.Code.Services;

namespace JboxTransfer.Views
{
    /// <summary>
    /// ListPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class ListPage : Page
    {
        [ObservableProperty]
        ObservableCollection<SyncTaskViewModel> listCurrent;

        [ObservableProperty]
        ObservableCollection<SyncTaskViewModel> listCompleted;

        [ObservableProperty]
        ObservableCollection<SyncTaskViewModel> listError;

        [ObservableProperty]
        List<SyncTaskViewModel> listPending;

        [ObservableProperty]
        private int index;

        [ObservableProperty]
        private bool isBusy;

        private ISnackBarService snackBarService;

        public ListPage(ISnackBarService snackBarService)
        {
            InitializeComponent();
            this.DataContext = this;

            this.snackBarService = snackBarService;

            ListCompleted = new ObservableCollection<SyncTaskViewModel>();
            ListCurrent = new ObservableCollection<SyncTaskViewModel>();
            ListError = new ObservableCollection<SyncTaskViewModel>();
            ListPending = new List<SyncTaskViewModel>();

            checker = new LoopWorker();
            checker.Interval = 1000;
            checker.CanRun += () => true;
            checker.OnGoAnimation += () => { };
            checker.Go += Checker_Go;
            checker.StartRun();
        }

        LoopWorker checker;


        private TaskState Checker_Go()
        {
            UpdateInfo();
            UpdateList();
            if (IsBusy)
                UpdateNewTask();
            UpdatePending();
            return TaskState.Started;
        }

        private void UpdateNewTask()
        {
            while (ListCurrent.Count < 8)
            {
                SyncTaskDbModel item = null;
                DbService.db.RunInTransaction(() =>
                {
                    item = DbService.db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).Where(x => x.State == 0).FirstOrDefault(); //.Where(x => x.State == 0)
                    if (item == null)
                        return;
                    item.State = 1;
                    DbService.db.Update(item);
                });

                if (item == null)
                    break;

                IBaseTask task2;
                if (item.Type == 0)
                {
                    task2 = new FileSyncTask(item);
                    task2.Start();
                }
                else
                {
                    task2 = new FolderSyncTask(item);
                    task2.Start();
                }

                this.Dispatcher.Invoke(() =>
                {
                    SyncTaskViewModel vm;
                    vm = new SyncTaskViewModel()
                    {
                        FileName = task2.GetName(),
                        ParentPath = task2.GetParentPath(),
                        Task = task2
                    };
                    vm.Icon = IconHelper.FindIconForFilename(vm.FileName, true);
                    ListCurrent.Add(vm);
                });
            }
        }

        private void UpdateList()
        {
            this.Dispatcher.Invoke(() =>
            {
                for (int i = ListCurrent.Count - 1; i >= 0; i--)
                {
                    var task = ListCurrent[i];
                    if (task.State == Models.SyncTaskState.Complete)
                    {
                        ListCurrent.Remove(task);
                        ListCompleted.Add(task);
                        task.Task = null;
                    }
                    else if (task.State == Models.SyncTaskState.Error)
                    {
                        ListCurrent.Remove(task);
                        ListError.Add(task);
                    }
                }
            });
        }

        private void UpdatePending()
        {
            var items = DbService.db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).Where(x => x.State == 0).Take(20).ToList();
            var list = new List<SyncTaskViewModel>();
            foreach(var item in items)
            {
                SyncTaskViewModel vm;
                vm = new SyncTaskViewModel()
                {
                    FileName = item.FileName,
                    ParentPath = item.FilePath,
                };
                this.Dispatcher.Invoke(() =>
                {
                    vm.Icon = item.Type == 0 ?
                                IconHelper.FindIconForFilename(vm.FileName, true) :
                                IconHelper.FindIconForFolder(true);
                });
                list.Add(vm);
            }
            ListPending = list;
        }

        private void UpdateInfo()
        {
            foreach(var task in ListCurrent)
            {
                task.Progress = task.Task.Progress;
                task.State = task.Task.State;
                if (task.State == SyncTaskState.Error)
                    task.ProgressStr = task.Task.Message;
                else
                    task.ProgressStr = task.Task.GetProgressStr();
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            foreach (var task in ListCurrent)
            {
                task.Task.Resume();
            }
            //checker.StartRun();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            //checker.StopRun();
            IsBusy = false;
            foreach (var task in ListCurrent)
            {
                task.Task.Pause();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonErrorRestart1_Click(object sender, RoutedEventArgs e)
        {
            for (int i = ListError.Count - 1; i >= 0; i--)
            {
                var item = ListError[i];
                item.Task.Recover(true);
                ListError.Remove(item);
            }
        }

        private void ButtonErrorRestart2_Click(object sender, RoutedEventArgs e)
        {
            for (int i = ListError.Count - 1; i >= 0; i--)
            {
                var item = ListError[i];
                item.Task.Recover(false);
                ListError.Remove(item);
            }
        }

        private void ButtonErrorCancel_Click(object sender, RoutedEventArgs e)
        {
            foreach(var item in ListError)
            {
                item.Task.Cancel();
            }
            ListError = new ObservableCollection<SyncTaskViewModel>();
        }

        private void ButtonRefreshPending_Click(object sender, RoutedEventArgs e)
        {
            UpdatePending();
        }

        [RelayCommand]
        private void ToggleA(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            if (vm.Task.State == SyncTaskState.Running)
                vm.Task.Pause();
            else if (vm.Task.State == SyncTaskState.Pause)
                vm.Task.Resume();
        }

        [RelayCommand]
        private void CancelA(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
        }

        [RelayCommand]
        private void OpenInTbox(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
        }

        [RelayCommand]
        private void OpenInJbox(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
        }

        [RelayCommand]
        private void CopyPath(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            Clipboard.SetText(vm.Task.GetPath());
            snackBarService.MessageQueue.Enqueue("已复制到剪切板");
        }
    }
}
