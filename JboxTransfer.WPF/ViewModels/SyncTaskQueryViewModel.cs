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

        public static SyncTaskQueryViewModel Create(Dictionary<string, object> item, QueryType queryType)
        {
            object obj = null;
            SyncTaskDbModel model = new SyncTaskDbModel();
            if (item.TryGetValue("Id", out obj)) model.Id = (int)(long)obj;
            if (item.TryGetValue("Order", out obj)) model.Order = (int)(long)obj;
            if (item.TryGetValue("Type", out obj)) model.Type = (int)(long)obj;
            if (item.TryGetValue("FileName", out obj)) model.FileName = (string)obj;
            if (item.TryGetValue("FilePath", out obj)) model.FilePath = (string)obj;
            if (item.TryGetValue("Size", out obj)) model.Size = (long)obj;
            if (item.TryGetValue("ConfirmKey", out obj)) model.ConfirmKey = (string)obj;
            if (item.TryGetValue("State", out obj)) model.State = (int)(long)obj;
            if (item.TryGetValue("MD5_Part", out obj)) model.MD5_Part = (string)obj;
            if (item.TryGetValue("MD5_Ori", out obj)) model.MD5_Ori = (string)obj;
            if (item.TryGetValue("CRC64_Part", out obj)) model.CRC64_Part = (long)obj;
            if (item.TryGetValue("RemainParts", out obj)) model.RemainParts = (string)obj;
            if (item.TryGetValue("Message", out obj)) model.Message = (string)obj;
            return new SyncTaskQueryViewModel(model) { queryType = queryType };
        }
    }
}
