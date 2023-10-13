using JboxTransfer.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// RichTextDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RichTextDialog : UserControl
    {
        public RichTextDialog(MemoryStream ms)
        {
            InitializeComponent();
            rich.Selection.Load(ms, DataFormats.Rtf);
            rich.Selection.Select(rich.Document.ContentStart, rich.Document.ContentStart);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close(DialogService.DialogIdentifier, new CommonResult(false, ""));
        }
    }
}
