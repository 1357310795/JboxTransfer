using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Output
{
    public class SyncTaskListOutputDto : ListOutputDto<SyncTaskOutputDto>
    {
        public bool HasMore { get; set; }
        public bool IsTooManyError { get; set; }
        public string Message { get; set; }

        public SyncTaskListOutputDto(List<SyncTaskOutputDto> entities) : base(entities)
        {

        }
    }
}
