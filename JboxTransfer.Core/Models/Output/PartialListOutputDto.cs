using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Output
{
    public class PartialListOutputDto<T>
    {
        public int Total { get; set; }
        public int Current { get; set; }
        public List<T> Entities { get; set; }

        public PartialListOutputDto(List<T> entities, int total)
        {
            Entities = entities;
            Total = total;
            Current = entities.Count;
        }
    }
}
