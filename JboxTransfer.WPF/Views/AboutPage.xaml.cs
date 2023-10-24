using JboxTransfer.Helpers;
using JboxTransfer.Services;
using JboxTransfer.Views.Dialogs;
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

namespace JboxTransfer.Views
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {
            LaunchHelper.OpenURL("https://pan.sjtu.edu.cn/jboxtransfer");
        }

        private void ButtonDocs_Click(object sender, RoutedEventArgs e)
        {
            LaunchHelper.OpenURL("https://pan.sjtu.edu.cn/jboxtransfer");
        }

        private void ButtonGithub_Click(object sender, RoutedEventArgs e)
        {
            LaunchHelper.OpenURL("https://github.com/1357310795/JboxTransfer");
        }

        private void LinkContact_Click(object sender, RoutedEventArgs e)
        {
            LaunchHelper.OpenURL("mailto://jbox@sjtu.edu.cn");
        }

        private async void LinkEULA_Click(object sender, RoutedEventArgs e)
        {
            await DialogService.ShowRichText(new MemoryStream(EmbedResHelper.GetELUA().ToArray()));
        }

        private async void LinkPrivacy_Click(object sender, RoutedEventArgs e)
        {
            await DialogService.ShowRichText(new MemoryStream(EmbedResHelper.GetPrivacy().ToArray()));
        }

        private async void LinkOpenSource_Click(object sender, RoutedEventArgs e)
        {
            await DialogService.ShowRichText(new MemoryStream(EmbedResHelper.GetOpenSource().ToArray()));
        }
    }
}
