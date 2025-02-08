using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Db
{
    public class UserStatistics
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long TotalTransferredBytes { get; set; }
        public long JboxSpaceUsedBytes { get; set; }
        public bool OnlyFullTransfer { get; set; }
    }
}
