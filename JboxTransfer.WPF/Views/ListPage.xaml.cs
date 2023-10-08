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
using System.Diagnostics;
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
using Teru.Code.Extensions;
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
        private int index;

        [ObservableProperty]
        private bool isBusy;

        private ISnackBarService snackBarService;

        private int maxThread = 8;

        public ListPage(ISnackBarService snackBarService)
        {
            InitializeComponent();
            this.DataContext = this;

            this.snackBarService = snackBarService;

            ListCompleted = new ObservableCollection<SyncTaskViewModel>();
            ListCurrent = new ObservableCollection<SyncTaskViewModel>();
            ListError = new ObservableCollection<SyncTaskViewModel>();

            checker = new LoopWorker();
            checker.Interval = 1000;
            checker.CanRun += () => true;
            checker.OnGoAnimation += () => { };
            checker.Go += Checker_Go;
            checker.StartRun();

            checker2 = new LoopWorker();
            checker2.Interval = 1000;
            checker2.CanRun += () => true;
            checker2.OnGoAnimation += () => { };
            checker2.Go += Checker2_Go;
            //checker2.StartRun();
        }

        LoopWorker checker;
        LoopWorker checker2;


        private TaskState Checker_Go()
        {
            try
            {
                UpdateInfo();
                UpdateList();
                if (IsBusy)
                    UpdateStartNew();
                UpdateAddTask();
                return TaskState.Started;
            }
            catch(Exception ex)
            {
                //Todo:log
                return TaskState.Started;
            }
        }

        private TaskState Checker2_Go()
        {
            return TaskState.Started;
        }

        private void UpdateStartNew()
        {
            while (ListCurrent.Count(
                x=>x.Task.State == SyncTaskState.Running || 
                x.Task.State == SyncTaskState.Error ||
                x.Task.State == SyncTaskState.Complete
                ) < maxThread)
            {
                var x = ListCurrent.FirstOrDefault(x => x.Task.State == SyncTaskState.Wait);
                if (x == null)
                    return;
                x.Task.Start();
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

        private void UpdateAddTask()
        {
            if (ListCurrent.Count < 99)
            {
                List<SyncTaskDbModel> items = null;
                DbService.db.RunInTransaction(() =>
                {
                    items = DbService.db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).Where(x => x.State == 0).Take(99 - ListCurrent.Count).ToList(); //.Where(x => x.State == 0)
                    if (items == null || items.Count == 0)
                        return;
                    foreach(var item in items)
                    {
                        item.State = 1;
                        DbService.db.Update(item);
                    }
                });

                if (items == null || items.Count == 0)
                    return;

                foreach(var item in items)
                {
                    IBaseTask task2;
                    if (item.Type == 0)
                    {
                        task2 = new FileSyncTask(item);
                        //task2.Start();
                    }
                    else
                    {
                        task2 = new FolderSyncTask(item);
                        //task2.Start();
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
                        this.Dispatcher.Invoke(() =>
                        {
                            vm.Icon = item.Type == 0 ?
                                        IconHelper.FindIconForFilename(vm.FileName, true) :
                                        IconHelper.FindIconForFolder(true);
                        });
                        ListCurrent.Add(vm);
                    });
                }
            }
            //var items = DbService.db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).Where(x => x.State == 0).Take(99-).ToList();
            //var list = new List<SyncTaskViewModel>();
            //foreach(var item in items)
            //{
            //    SyncTaskViewModel vm;
            //    vm = new SyncTaskViewModel()
            //    {
            //        FileName = item.FileName,
            //        ParentPath = item.FilePath,
            //    };
            //    this.Dispatcher.Invoke(() =>
            //    {
            //        vm.Icon = item.Type == 0 ?
            //                    IconHelper.FindIconForFilename(vm.FileName, true) :
            //                    IconHelper.FindIconForFolder(true);
            //    });
            //    list.Add(vm);
            //}
            //ListPending = list;
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
            var t = maxThread;
            foreach (var task in ListCurrent)
            {
                t--;
                task.Task.Resume();
                if (t == 0) 
                    break;
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
            UpdateAddTask();
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
            else if (vm.Task.State == SyncTaskState.Wait)
                vm.Task.Start();
        }

        [RelayCommand]
        private void CancelA(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            vm.Task.Cancel();
            if (ListCurrent.Contains(vm))
                ListCurrent.Remove(vm);
        }

        [RelayCommand]
        private void OpenInJbox(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;

            var path = vm.ParentPath;
            var res = JboxService.GetJboxFileInfo(path);
            if (!res.Success)
            {
                MessageBox.Show($"获取信息失败：{res.Message}");
                return;
            }
            if (!res.Result.IsDir)
            {
                MessageBox.Show($"找不到父文件夹");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = $"https://jbox.sjtu.edu.cn/v/list/self/{res.Result.Neid}",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        [RelayCommand]
        private void OpenInTbox(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;

            var path = vm.ParentPath;
            path = path.Substring(1, path.Length - 1).UrlEncodeByParts();

            var psi = new ProcessStartInfo
            {
                FileName = $"https://pan.sjtu.edu.cn/web/desktop/personalSpace?path={path}",
                UseShellExecute = true
            };
            Process.Start(psi);
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

        [RelayCommand]
        private void Retry(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            vm.Task.Recover(true);
            ListError.Remove(vm);
        }

        [RelayCommand]
        private void RetryFromBegin(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            vm.Task.Recover(false);
            ListError.Remove(vm);
        }

        [RelayCommand]
        private void CancelC(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            vm.Task.Cancel();
            ListError.Remove(vm);
        }
    }
}
