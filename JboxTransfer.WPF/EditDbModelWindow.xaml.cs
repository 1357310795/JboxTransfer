using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Models;
using JboxTransfer.Services;
using Newtonsoft.Json;
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
using System.Windows.Shapes;

namespace JboxTransfer
{
    /// <summary>
    /// EditDbModelWindow.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class EditDbModelWindow : Window
    {
        SyncTaskDbModel dbModel;

        [ObservableProperty]
        private string text;

        public EditDbModelWindow(SyncTaskDbModel dbModel)
        {
            InitializeComponent();
            this.dbModel = dbModel;
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Text = JsonConvert.SerializeObject(this.dbModel);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.dbModel = DbService.db.Get<SyncTaskDbModel>(this.dbModel.Id);
            Text = JsonConvert.SerializeObject(this.dbModel);
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            DoUpdate();
        }

        private bool DoUpdate()
        {
            try
            {
                var newModel = JsonConvert.DeserializeObject<SyncTaskDbModel>(Text);
                if (this.dbModel.Id != newModel.Id)
                {
                    MessageBox.Show($"不允许更改主键Id！");
                    return false;
                }
                DbService.db.Update(newModel);
                MessageBox.Show("操作成功！");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生错误：{ex}");
                return false;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var res = DoUpdate();
            if (res) this.Close();
        }
    }
}
