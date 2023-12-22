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
    public partial class DbQueryResWindow : Window
    {
        [ObservableProperty]
        private string text;

        public DbQueryResWindow(string text)
        {
            InitializeComponent();
            this.Text = text;
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
