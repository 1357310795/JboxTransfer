using AutoMapper;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules.Sync;
using JboxTransfer.Core.Modules;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using JboxTransfer.Server.Modules.DataWrapper;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Output;

namespace JboxTransfer.Server.Controllers
{
    [ApiController]
    [Route("api/v1/db")]
    public class DbController : ControllerBase
    {
        private readonly ILogger<DbController> _logger;
        private readonly DefaultDbContext _db;
        private readonly SystemUserInfoProvider _user;
        private readonly IMapper _mapper;
        public DbController(ILogger<DbController> logger, DefaultDbContext db, SystemUserInfoProvider user, IMapper mapper)
        {
            _logger = logger;
            _db = db;
            _user = user;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("query")]
        [Authorize]
        public ApiResponse Query([FromForm] string search, [FromForm] string? state,
            [FromForm] int pageSize, [FromForm] int current)
        {
            if (string.IsNullOrEmpty(search))
            {
                return new ApiResponse(400, "SearchStringEmptyError", "搜索关键词不能为空");
            }
            var user = _user.GetUser();
            var query = _db.SyncTasks
                .Where(x => x.UserId == user.Id);

            if (state != null)
                if (Enum.TryParse<SyncTaskDbState>(state, out var stateEnum))
                    query = query.Where(x => x.State == stateEnum);
            query = query.Where(x => x.FilePath.Contains(search));
            query = query.OrderBy(x => x.Id);

            var rescnt = query.Count();
            var res = query.Skip((current - 1) * pageSize).Take(pageSize).ToList();
            var resout = res.Select(x => _mapper.Map<SyncTaskDbModelOutputDto>(x)).ToList();
            return new ApiResponse(new PartialListOutputDto<SyncTaskDbModelOutputDto>(resout, rescnt));
        }
    }
}
