using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Helpers;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using JboxTransfer.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Teru.Code.Extensions;
using Teru.Code.Services;

namespace JboxTransfer.Views
{
    /// <summary>
    /// ListPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class ListPage : Page, IRecipient<UserLogoutMessage>, IRecipient<SetTopMessage>
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

        [ObservableProperty]
        private string errorNum;

        private ISnackBarService snackBarService;
        private object addTaskLock = new object();
        //private int maxThread = 8;

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
            checker.Go += Checker_Go;
            checker.StartRun();

            checker2 = new LoopWorker();
            checker2.Interval = 1000;
            checker2.CanRun += () => true;
            checker2.Go += Checker2_Go;
            //checker2.StartRun();
            WeakReferenceMessenger.Default.Register<UserLogoutMessage>(this);
            WeakReferenceMessenger.Default.Register<SetTopMessage>(this);
            Task.Run(LoadErrorFromDb);
        }

        LoopWorker checker;
        LoopWorker checker2;


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateErrorNum()
        {
            if (ListError.Count == 0)
                ErrorNum = null;
            else if (ListError.Count > 99)
            {
                ErrorNum = "99+";
            }
            else
                ErrorNum = ListError.Count.ToString();
        }

        private void CheckTooManyErrors()
        {
            if (ListError.Count > 99)
            {
                ErrorNum = "99+";
                IsBusy = false;
                snackBarService.MessageQueue.Enqueue("错误过多，队列被迫终止。请先处理出错的项目。");
            }
        }

        private void LoadErrorFromDb()
        {
            try
            {
                var res = DbService.db.Table<SyncTaskDbModel>().Where(x => x.State == 2);
                if (res.Count() == 0)
                    return;
                foreach (var item in res)
                {
                    this.Dispatcher.Invoke(() =>
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

                        SyncTaskViewModel vm;
                        vm = new SyncTaskViewModel()
                        {
                            FileName = item.FileName,
                            ParentPath = item.FilePath.GetParentPath(),
                            ProgressStr = item.Message,
                            Task = task2
                        };
                        vm.Icon = item.Type == 0 ?
                                        IconHelper.FindIconForFilename(vm.FileName, true) :
                                        IconHelper.FindIconForFolder(true);
                        ListError.Add(vm);
                        UpdateErrorNum();
                    });
                }
            }
            catch (Exception ex)
            {
                //log
                Debug.WriteLine(ex);
            }
        }
        private TaskState Checker_Go(CancellationTokenSource cts)
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

        private TaskState Checker2_Go(CancellationTokenSource cts)
        {
            return TaskState.Started;
        }

        private void UpdateStartNew()
        {
            while (ListCurrent.Count(
                x => x.Task.State == SyncTaskState.Running ||
                x.Task.State == SyncTaskState.Error ||
                x.Task.State == SyncTaskState.Complete
                ) < GlobalSettings.Model.WorkThreads)
            {
                Debug.WriteLine("begin start new");
                var x = ListCurrent.FirstOrDefault(x => (x.Task.State == SyncTaskState.Wait || x.Task.State == SyncTaskState.Pause) && (x.IsUserPause == false));
                if (x == null)
                    return;
                if (x.Task.State == SyncTaskState.Wait)
                    x.Task.Start();
                else
                    x.Task.Resume();
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
                        ListCompleted.Insert(0, task);
                        task.Task = null;
                    }
                    else if (task.State == Models.SyncTaskState.Error)
                    {
                        ListCurrent.Remove(task);
                        ListError.Insert(0, task);
                        UpdateErrorNum();
                        CheckTooManyErrors();
                    }
                }
            });
        }

        private void UpdateAddTask()
        {
            Monitor.Enter(addTaskLock);
            try
            {
                if (ListCurrent.Count < 99)
                {
                    List<SyncTaskDbModel> items = null;
                    //此处应该不需要transaction，因为只有这里会AddTask
                    items = DbService.db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).Where(x => x.State == 0).Take(99 - ListCurrent.Count).ToList(); //.Where(x => x.State == 0)
                    if (items == null || items.Count == 0)
                        return;
                    foreach (var item in items)
                    {
                        item.State = 1;
                        DbService.db.Update(item);
                    }

                    if (items == null || items.Count == 0)
                        return;

                    foreach (var item in items)
                    {
                        AddToCurrentInternal(item, false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Monitor.Exit(addTaskLock);
            }
        }

        private void AddToCurrentInternal(SyncTaskDbModel item, bool setTop)
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
                if (setTop)
                    ListCurrent.Insert(0, vm);
                else
                    ListCurrent.Add(vm);
            });
        }

        private void UpdateInfo()
        {
            foreach(var task in ListCurrent)
            {
                task.Progress = task.Task.Progress;
                task.State = task.Task.State;
                if (task.State == SyncTaskState.Error)
                {
                    task.ProgressStr = task.Task.Message;
                    task.ProgressTextTooltip = null;
                }
                else
                {
                    task.ProgressStr = task.Task.GetProgressStr();
                    task.ProgressTextTooltip = task.Task.GetProgressTextTooltip();
                }
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (ListError.Count > 99)
            {
                MessageBox.Show("错误过多，无法启动队列。请先前往“已停止”选项卡处理出错的项目。", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsBusy = true;
            var t = GlobalSettings.Model.WorkThreads;
            foreach (var vm in ListCurrent)
            {
                vm.IsUserPause = false;
            }
            foreach (var vm in ListCurrent)
            {
                t--;
                if (vm.Task.State == SyncTaskState.Running)
                    continue;
                vm.Task.Resume();
                if (t == 0) 
                    break;
            }
            //checker.StartRun();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            //checker.StopRun();
            IsBusy = false;
            foreach (var vm in ListCurrent)
            {
                vm.Task.Pause();
                vm.IsUserPause = true;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("此操作将删除所有传输中、待传输和出错的项目，是否确定？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                IsBusy = false;
                foreach (var vm in ListCurrent)
                {
                    vm.Task.Cancel();
                }
                DbService.db.Execute("DELETE FROM SyncTaskDbModel;");
                ListCurrent.Clear();
            }
        }

        private void ButtonErrorRestart1_Click(object sender, RoutedEventArgs e)
        {
            for (int i = ListError.Count - 1; i >= 0; i--)
            {
                var item = ListError[i];
                item.Task.Recover(true);
                ListError.Remove(item);
                UpdateErrorNum();
            }
        }

        private void ButtonErrorRestart2_Click(object sender, RoutedEventArgs e)
        {
            for (int i = ListError.Count - 1; i >= 0; i--)
            {
                var item = ListError[i];
                item.Task.Recover(false);
                ListError.Remove(item);
                UpdateErrorNum();
            }
        }

        private void ButtonErrorCancel_Click(object sender, RoutedEventArgs e)
        {
            foreach(var vm in ListError)
            {
                vm.Task.Cancel();
            }
            ListError = new ObservableCollection<SyncTaskViewModel>();
            UpdateErrorNum();
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
            {
                vm.Task.Pause();
                vm.IsUserPause = true;
            }
            else if (vm.Task.State == SyncTaskState.Pause)
            {
                vm.Task.Resume();
                vm.IsUserPause = false;
            }
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

            LaunchHelper.OpenURL($"https://jbox.sjtu.edu.cn/v/list/self/{res.Result.Neid}");
        }

        [RelayCommand]
        private void OpenInTbox(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;

            var path = vm.ParentPath;
            path = path.Substring(1, path.Length - 1).UrlEncodeByParts();

            LaunchHelper.OpenURL($"https://pan.sjtu.edu.cn/web/desktop/personalSpace?path={path}");
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
            UpdateErrorNum();
        }

        [RelayCommand]
        private void RetryFromBegin(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            vm.Task.Recover(false);
            ListError.Remove(vm);
            UpdateErrorNum();
        }

        [RelayCommand]
        private void CancelC(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            vm.Task.Cancel();
            ListError.Remove(vm);
            UpdateErrorNum();
        }

        [RelayCommand]
        private void SetTop(object sender)
        {
            SyncTaskViewModel vm = sender as SyncTaskViewModel;
            if (vm == null)
                return;
            ListCurrent.Remove(vm);
            ListCurrent.Insert(0, vm);
        }

        public void Receive(UserLogoutMessage message)
        {
            ButtonPause_Click(null, null);
        }

        public void Receive(SetTopMessage message)
        {
            var dbModel = message.DbModel;
            try
            {
                Monitor.Enter(addTaskLock);
                dbModel = DbService.db.Get<SyncTaskDbModel>(dbModel.Id);
                if (dbModel.State != 0)
                {
                    MessageBox.Show("状态错误！传输任务可能已经在队列中");
                    return;
                }
                dbModel.State = 1;
                DbService.db.Update(dbModel);
                AddToCurrentInternal(dbModel, true);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Monitor.Exit(addTaskLock);
            }
        }

        private void ButtonStopHelp_Click(object sender, RoutedEventArgs e)
        {
            LaunchHelper.OpenURL("https://pan.sjtu.edu.cn/jboxtransfer/error-dealing.html");
        }
    }
}
