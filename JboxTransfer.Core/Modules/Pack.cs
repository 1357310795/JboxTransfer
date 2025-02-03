using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules
{
    public class Pack<T>
    {
        public Pack(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
