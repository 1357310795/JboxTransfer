using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JboxTransfer.Helpers;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using JboxTransfer.ViewModels;
using Newtonsoft.Json.Linq;
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
using Teru.Code.Extensions;

namespace JboxTransfer.Views
{
    /// <summary>
    /// DbOpPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class DbOpPage : Page
    {
        [ObservableProperty]
        private List<QueryTypeViewModel> queryTypes;

        [ObservableProperty]
        private QueryTypeViewModel selectedQueryType;

        [ObservableProperty]
        private string hint;

        [ObservableProperty]
        private string queryText;

        [ObservableProperty]
        private ObservableCollection<SyncTaskQueryViewModel> listResult;

        static string[] HintList = { "请输入要查询的项目的路径（支持模糊搜索）", "请输入要查询的项目的路径（支持模糊搜索）", "请输入完整的SQL语句进行查询（Select * From SyncTaskDbModel Where ……）" };

        ISnackBarService snackBarService;

        public DbOpPage(ISnackBarService snackBarService)
        {
            InitializeComponent();
            QueryTypes = new List<QueryTypeViewModel>();
            QueryTypes.Add(new QueryTypeViewModel(QueryType.QueryWait, "查询待传输项目"));
            QueryTypes.Add(new QueryTypeViewModel(QueryType.QueryCompleted, "查询已完成项目"));
            QueryTypes.Add(new QueryTypeViewModel(QueryType.QuerySql, "使用SQL语句查询"));

            SelectedQueryType = QueryTypes[0];
            this.snackBarService = snackBarService;
            this.DataContext = this;
        }

        partial void OnSelectedQueryTypeChanged(QueryTypeViewModel value)
        {
            switch(value.Type)
            {
                case QueryType.QueryWait:
                    Hint = HintList[0];
                    QueryText = "";
                    break;
                case QueryType.QueryCompleted:
                    Hint = HintList[1];
                    QueryText = "";
                    break;
                case QueryType.QuerySql:
                    Hint = HintList[2];
                    QueryText = "Select * From SyncTaskDbModel Where FilePath like '%校园%'";
                    break;
            }
        }

        private void ButtonQuery_Click(object sender, RoutedEventArgs e)
        {
            switch (SelectedQueryType.Type)
            {
                case QueryType.QueryWait:
                    //你说什么？你看见了sql字符串拼接？
                    //对上暗号，确定你是学网安的了
                    //但是teru也是学网安的
                    //为什么会犯这么“低级的错误”呢
                    //为什么呢？
                    DoQuery($"Select * From SyncTaskDbModel Where FilePath like '%{QueryText}%' And State = 0");
                    break;
                case QueryType.QueryCompleted:
                    DoQuery($"Select * From SyncTaskDbModel Where FilePath like '%{QueryText}%' And State = 3");
                    break;
                case QueryType.QuerySql:
                    DoQuery(QueryText);
                    break;
            }
        }

        private void DoQuery(string sql)
        {
            try
            {
                var res = DbService.db.Query<SyncTaskDbModel>(sql);
                if (res == null) {
                    snackBarService.MessageQueue.Enqueue($"查询结果为空");
                    return;
                }
                if (res.Count == 0) {
                    snackBarService.MessageQueue.Enqueue($"操作执行成功，但是查询结果为空");
                    return;
                }
                ListResult = new ObservableCollection<SyncTaskQueryViewModel>();
                foreach(var item in res)
                {
                    ListResult.Add(new SyncTaskQueryViewModel(item) { queryType = SelectedQueryType.Type });
                    if (ListResult.Count >= 1000)
                        break;
                }
            }
            catch(Exception ex)
            {
                snackBarService.MessageQueue.Enqueue($"查询失败：{ex.Message}");
            }
        }

        [RelayCommand]
        private void OpenInJbox(object sender)
        {
            SyncTaskQueryViewModel vm = sender as SyncTaskQueryViewModel;
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
            SyncTaskQueryViewModel vm = sender as SyncTaskQueryViewModel;
            if (vm == null)
                return;

            var path = vm.ParentPath;
            path = path.Substring(1, path.Length - 1).UrlEncodeByParts();

            LaunchHelper.OpenURL($"https://pan.sjtu.edu.cn/web/desktop/personalSpace?path={path}");
        }

        [RelayCommand]
        private void CopyPath(object sender)
        {
            SyncTaskQueryViewModel vm = sender as SyncTaskQueryViewModel;
            if (vm == null)
                return;
            Clipboard.SetText(vm.dbModel.FilePath);
            snackBarService.MessageQueue.Enqueue("已复制到剪切板");
        }

        [RelayCommand]
        private void ViewJson(object sender)
        {
            SyncTaskQueryViewModel vm = sender as SyncTaskQueryViewModel;
            if (vm == null)
                return;
            EditDbModelWindow w = new EditDbModelWindow(vm.dbModel);
            w.Show();
        }

        [RelayCommand]
        private void SetTop(object sender)
        {
            SyncTaskQueryViewModel vm = sender as SyncTaskQueryViewModel;
            if (vm == null)
                return;
            vm.dbModel.Order = DbService.GetMinOrder() - 1;
            DbService.db.Update(vm.dbModel);
            SetTopMessage message = new SetTopMessage(vm.dbModel);
            WeakReferenceMessenger.Default.Send(message);
            ListResult.Remove(vm);
        }

        [RelayCommand]
        private void Cancel(object sender)
        {
            SyncTaskQueryViewModel vm = sender as SyncTaskQueryViewModel;
            if (vm == null)
                return;
            vm.dbModel.State = 4;
            DbService.db.Update(vm.dbModel);
            ListResult.Remove(vm);
        }
    }

    public class QueryTypeViewModel
    {
        public QueryTypeViewModel(QueryType type, string name)
        {
            Type = type;
            Name = name;
        }

        public QueryType Type { get; set; }
        public string Name { get; set; }
    }

    public enum QueryType
    {
        QueryWait,
        QueryCompleted,
        QuerySql,
    }
}
