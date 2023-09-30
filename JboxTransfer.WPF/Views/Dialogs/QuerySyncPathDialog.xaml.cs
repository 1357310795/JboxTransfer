using JboxTransfer.Services;
using MaterialDesignThemes.Wpf;
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
using Teru.Code.Models;

namespace JboxTransfer.Views.Dialogs
{
    /// <summary>
    /// QuerySyncPathDialog.xaml 的交互逻辑
    /// </summary>
    public partial class QuerySyncPathDialog : UserControl
    {
        public QuerySyncPathDialog()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close(DialogService.DialogIdentifier, new CommonResult<string>(true, "", Text1.Text));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close(DialogService.DialogIdentifier, new CommonResult<string>(false, "已取消"));
        }
    }
}
