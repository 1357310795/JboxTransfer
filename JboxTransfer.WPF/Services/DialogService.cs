using JboxTransfer.Views.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Models;

namespace JboxTransfer.Services
{
    public class DialogService
    {
        public const string DialogIdentifier = "MainDialogHost";
        public static async Task<CommonResult<string>> QuerySyncPath()
        {
            var res = await DialogHost.Show(new QuerySyncPathDialog(), DialogIdentifier);
            return (CommonResult<string>)res;
        }
    }
}
