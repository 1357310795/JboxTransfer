
using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Models;
using JboxTransfer.Modules.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using JboxTransfer.Core.Helpers;

namespace JboxTransfer.ViewModels
{
    public partial class JboxItemViewModel : ObservableObject
    {
        public ImageSource Icon { get; set; }

        public string Name => Info.Path.PathToName();

        public JboxItemInfo Info { get; set; }
    }
}
