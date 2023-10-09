using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JboxTransfer.Helpers;
using MaterialDesignColors;
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

namespace JboxTransfer.Views
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        [ObservableProperty]
        private ObservableCollection<Color> themeColors;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeColors = new ObservableCollection<Color>();
            var swatches = SwatchHelper.Swatches.ToList().SelectMany(t => t.Hues);
            foreach (var item in swatches) ThemeColors.Add(item);
        }

        [RelayCommand]
        private void ChangeHue(object sender)
        {
            Color c = (Color)sender;
            ThemeHelper.ChangeHue(c);
        }
    }
}
