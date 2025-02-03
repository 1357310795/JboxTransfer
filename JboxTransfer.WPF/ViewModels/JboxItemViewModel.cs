
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Jbox;

namespace JboxTransfer.ViewModels
{
    public partial class JboxItemViewModel : ObservableObject
    {
        public ImageSource Icon { get; set; }

        public string Name => Info.Path.PathToName();

        public JboxItemInfo Info { get; set; }
    }
}
