using AutoMapper;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Output;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules.Sync;
using JboxTransfer.Core.Services;
using JboxTransfer.Server.Modules.DataWrapper;
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
        public TaskController(ILogger<TaskController> logger, IMemoryCache memoryCache, DefaultDbContext db, SystemUserInfoProvider user, SyncTaskCollectionProvider taskCollectionProvider, JboxService jbox, IMapper mapper)
        {
            _logger = logger;
            _mcache = memoryCache;
            _db = db;
            _user = user;
            _taskCollectionProvider = taskCollectionProvider;
            _jbox = jbox;
            _mapper = mapper;
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
        public ApiResponse Add([FromForm]string path) //EnqueueTask
        {
            var res = _jbox.GetJboxFileInfo(path);
            if (!res.Success)
            {
                return new ApiResponse(500, "GetJboxItemInfoError", $"获取文件信息失败：{res.Message}");
            }

            var order = _db.GetMinOrder() - 1;
            var entity = _db.Add(new SyncTaskDbModel(_user.GetUser(), res.Result.IsDir ? SyncTaskType.Folder : SyncTaskType.File, res.Result.Path, res.Result.Bytes, order) { MD5_Ori = res.Result.Hash });
            _db.SaveChanges();

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
        public ApiResponse Start(int id)
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
        public ApiResponse Cancel(int id)
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
        [Route("settop")]
        [Authorize]
        public ApiResponse SetTop(int id)
        {
            return new ApiResponse();
        }

        [HttpPost]
        [Route("restart")]
        [Authorize]
        public ApiResponse Restart(int id, bool keepProgress = true)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.RestartOneError(id, keepProgress);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "CancelTaskError", res.result);
            }
        }
    }
}
