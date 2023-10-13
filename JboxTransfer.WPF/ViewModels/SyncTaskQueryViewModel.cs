using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Extensions;
using JboxTransfer.Helpers;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JboxTransfer.ViewModels
{
    public partial class SyncTaskQueryViewModel : ObservableObject
    {
        [ObservableProperty]
        public string fileName;

        [ObservableProperty]
        public string parentPath;

        [ObservableProperty]
        public ImageSource icon;

        [ObservableProperty]
        public int state;

        [ObservableProperty]
        public string size;

        public SyncTaskDbModel dbModel;

        public QueryType queryType;
        public SyncTaskQueryViewModel(SyncTaskDbModel item)
        {
            this.dbModel = item;
            this.FileName = item.FileName;
            this.ParentPath = item.FilePath.GetParentPath();
            this.Icon = item.Type == 0 ?
                        IconHelper.FindIconForFilename(item.FileName, true) :
                        IconHelper.FindIconForFolder(true);
            this.State = item.State;
            this.Size = item.Size.PrettyPrint();
        }
    }
}
