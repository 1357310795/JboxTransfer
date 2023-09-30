using JboxTransfer.Services.Contracts;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Services
{
    public class SnackBarService : ISnackBarService
    {
        public ISnackbarMessageQueue MessageQueue { get; set; }


    }
}
