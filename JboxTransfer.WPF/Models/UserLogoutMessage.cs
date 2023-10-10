using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public class UserLogoutMessage : RequestMessage<int>
    {
        public UserLogoutMessage(int v)
        {
        }
    }
}
