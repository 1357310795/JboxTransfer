using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public class PageChangedMessage : ValueChangedMessage<string>
    {
        public PageChangedMessage(string page) : base(page)
        {

        }
    }
}
