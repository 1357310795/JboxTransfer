using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Db
{
    public class UserStatisticsOutputDto
    {
        public long TotalTransferredBytes { get; set; }
        public long JboxSpaceUsedBytes { get; set; }
        public bool OnlyFullTransfer { get; set; }
    }
}
