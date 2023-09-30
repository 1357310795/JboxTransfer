using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public IBaseTask Task;
    }
}
