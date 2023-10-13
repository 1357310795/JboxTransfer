using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Helpers;
using JboxTransfer.Services;
using JboxTransfer.Services.Contracts;
using MaterialDesignColors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        INavigationService navigationService;
        public SettingsPage(INavigationService navigationService)
        {
            InitializeComponent();
            ThemeModes = new List<string>() { "浅色模式", "深色模式" };
            selectedThemeMode = ThemeModes[GlobalSettings.Model.ThemeMode];
            selectedWorkThreads = GlobalSettings.Model.WorkThreads;
            WorkThreads = new List<int>();
            for (int i = 1; i <= 12; i++)
                WorkThreads.Add(i);
            this.navigationService = navigationService;
            this.DataContext = this;
        }

        [ObservableProperty]
        private ObservableCollection<Color> themeColors;

        [ObservableProperty]
        private List<int> workThreads;

        [ObservableProperty]
        private int selectedWorkThreads;

        [ObservableProperty]
        private List<string> themeModes;

        [ObservableProperty]
        private string selectedThemeMode;

        public string DataPath => PathHelper.AppDataPath;

        partial void OnSelectedWorkThreadsChanged(int value)
        {
            GlobalSettings.Model.WorkThreads = value;
        }

        partial void OnSelectedThemeModeChanged(string value)
        {
            ThemeHelper.ApplyBase(value == "深色模式");
            GlobalSettings.Model.ThemeMode = value == "深色模式" ? 1 : 0;
        }

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
            GlobalSettings.Model.ThemeColor = c.ColorToHex();
        }

        private void ButtonOpenDataPath_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
            psi.Arguments = DataPath;
            Process.Start(psi);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Save();
        }

        private void ButtonDbOperation_Click(object sender, RoutedEventArgs e)
        {
            navigationService.NavigateTo(nameof(DbOpPage));
        }
    }
}
