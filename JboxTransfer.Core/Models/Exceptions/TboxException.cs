using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Exceptions
{
    public class TboxException : Exception
    {
        public TboxException(string message) : base(message)
        {
        }
    }
}
