using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Services.Contracts
{
    public interface ISnackBarService
    {
        ISnackbarMessageQueue MessageQueue { get; set; }
    }
}
