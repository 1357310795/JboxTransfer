using CommunityToolkit.Mvvm.Messaging.Messages;
using JboxTransfer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models.Messages
{
    public class SetTopMessage : RequestMessage<bool>
    {
        public SyncTaskDbModel DbModel { get; set; }
        public SetTopMessage(SyncTaskDbModel dbModel)
        {
            DbModel = dbModel;
        }
    }
}
