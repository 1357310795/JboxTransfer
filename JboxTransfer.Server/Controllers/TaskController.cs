using AutoMapper;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Message;
using JboxTransfer.Core.Models.Output;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules.Sync;
using JboxTransfer.Core.Services;
using JboxTransfer.Server.Modules.DataWrapper;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JboxTransfer.Server.Controllers
{
    [ApiController]
    [Route("api/v1/task")]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly IMemoryCache _mcache;
        private readonly DefaultDbContext _db;
        private readonly SystemUserInfoProvider _user;
        private readonly SyncTaskCollectionProvider _taskCollectionProvider;
        private readonly JboxService _jbox;
        private readonly IMapper _mapper;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public TaskController(ILogger<TaskController> logger, IMemoryCache memoryCache, DefaultDbContext db, SystemUserInfoProvider user, SyncTaskCollectionProvider taskCollectionProvider, JboxService jbox, IMapper mapper, ISendEndpointProvider sendEndpointProvider)
        {
            _logger = logger;
            _mcache = memoryCache;
            _db = db;
            _user = user;
            _taskCollectionProvider = taskCollectionProvider;
            _jbox = jbox;
            _mapper = mapper;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpGet]
        [Route("info")]
        [Authorize]
        public ApiResponse GetInfo([FromQuery]int id)
        {
            return new ApiResponse();
        }

        [HttpPost]
        [Route("add")]
        [Authorize]
        public async Task<ApiResponse> Add([FromForm]string path) //EnqueueTask
        {
            var user = _user.GetUser();
            var res = _jbox.GetJboxItemInfo(path);
            if (!res.Success)
            {
                return new ApiResponse(500, "GetJboxItemInfoError", $"获取文件信息失败：{res.Message}");
            }
            var noentry = !_db.SyncTasks
                .Where(x => x.UserId == user.Id)
                .Any();
            var stat = _db.UserStats
                .Where(x => x.UserId == user.Id)
                .First();
            if (noentry && path == "/")
            {
                stat.OnlyFullTransfer = true;
            }
            else
            {
                stat.OnlyFullTransfer = false;
            }
            _db.Update(stat);

            var order = _db.GetMinOrder() - 1;
            var entity = _db.Add(new SyncTaskDbModel(_user.GetUser(), res.Result.IsDir ? SyncTaskType.Folder : SyncTaskType.File, res.Result.Path, res.Result.Bytes, order) { MD5_Ori = res.Result.Hash });

            _db.SaveChanges();

            _taskCollectionProvider.GetSyncTaskCollection(user);

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:add_task_from_db"));
            await endpoint.Send(new NewTaskCheckMessage() { UserId = _user.GetUser().Id });

            return new ApiResponse(_mapper.Map<SyncTaskDbModelOutputDto>(entity.Entity));
        }

        [HttpPost]
        [Route("pause")]
        [Authorize]
        public ApiResponse Pause([FromForm]int id)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.PauseOne(id);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "PauseTaskError", res.result);
            }
        }

        [HttpPost]
        [Route("start")]
        [Authorize]
        public ApiResponse Start([FromForm] int id)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.StartOne(id);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "StartTaskError", res.result);
            }
        }

        [HttpPost]
        [Route("cancel")]
        [Authorize]
        public ApiResponse Cancel([FromForm] int id)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.CancelOne(id);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "CancelTaskError", res.result);
            }
        }

        [HttpPost]
        [Route("cancelerr")]
        [Authorize]
        public ApiResponse CancelError([FromForm] int id)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.CancelOneError(id);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "CancelErrorTaskError", res.result);
            }
        }

        [HttpPost]
        [Route("settop")]
        [Authorize]
        public ApiResponse SetTop([FromForm] int id)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.SetTop(id);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "SetTopError", res.result);
            }
        }

        [HttpPost]
        [Route("restarterr")]
        [Authorize]
        public ApiResponse RestartError([FromForm] int id, [FromForm] bool keepProgress = true)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.RestartOneError(id, keepProgress);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "RestartErrorTaskError", res.result);
            }
        }

    }
}
