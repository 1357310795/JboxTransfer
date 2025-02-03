using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Core.Models;
using JboxTransfer.Core.Modules.Sync;
using System.Windows.Media;

namespace JboxTransfer.ViewModels
{
    public partial class SyncTaskViewModel : ObservableObject
    {
        [ObservableProperty]
        public string fileName;

        [ObservableProperty]
        public string parentPath;

        [ObservableProperty]
        public ImageSource icon;

        [ObservableProperty]
        public double progress;

        [ObservableProperty]
        public string progressStr;

        [ObservableProperty]
        public SyncTaskState state;

        [ObservableProperty]
        public string progressTextTooltip;

        public IBaseTask Task;
        public bool IsUserPause;
    }
}
