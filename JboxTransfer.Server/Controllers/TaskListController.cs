using AutoMapper;
using JboxTransfer.Core.Models.Output;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules.Sync;
using JboxTransfer.Server.Modules.DataWrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JboxTransfer.Server.Controllers
{
    [ApiController]
    [Route("api/v1/tasklist")]
    public class TaskListController : ControllerBase
    {
        private readonly ILogger<TaskListController> _logger;
        private readonly IMemoryCache _mcache;
        private readonly DefaultDbContext _db;
        private readonly SystemUserInfoProvider _user;
        private readonly SyncTaskCollectionProvider _taskCollectionProvider;
        private readonly JboxService _jbox;
        private readonly IMapper _mapper;

        public TaskListController(ILogger<TaskListController> logger, IMemoryCache mcache, DefaultDbContext db, SystemUserInfoProvider user, SyncTaskCollectionProvider taskCollectionProvider, JboxService jbox, IMapper mapper)
        {
            _logger = logger;
            _mcache = mcache;
            _db = db;
            _user = user;
            _taskCollectionProvider = taskCollectionProvider;
            _jbox = jbox;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("list")]
        [Authorize]
        public ApiResponse List(string type = "transferring")
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = type switch
            {
                "transferring" => collection.GetCurrentListInfo(),
                "completed" => collection.GetCompletedListInfo(),
                "error" => collection.GetErrorListInfo(),
                _ => throw new NotImplementedException(),
            };
            if (res.Success)
            {
                return new ApiResponse(new ListOutputDto<SyncTaskOutputDto>(res.Result));
            }
            else
            {
                return new ApiResponse(500, "QueryTaskQueueError", res.Message);
            }
        }

        [HttpPost]
        [Route("startall")]
        [Authorize]
        public ApiResponse StartAll()
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.StartAll();
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "StartTaskQueueError", res.result);
            }
        }        
        
        [HttpPost]
        [Route("pauseall")]
        [Authorize]
        public ApiResponse PauseAll()
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.PauseAll();
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "PauseTaskQueueError", res.result);
            }
        }        

        [HttpPost]
        [Route("cancelall")]
        [Authorize]
        public ApiResponse CancelAll()
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.CancelAll();
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "PauseTaskQueueError", res.result);
            }
        }

        // error queue
        [HttpPost]
        [Route("restartallerr")]
        [Authorize]
        public ApiResponse RestartAll(bool keepProgress = true)
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.RestartAllError(keepProgress);
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "RestartTaskQueueError", res.result);
            }
        }

        [HttpPost]
        [Route("cancelallerr")]
        [Authorize]
        public ApiResponse CancelAllError()
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.CancelAllError();
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "CancelTaskQueueError", res.result);
            }
        }

        // complete queue
        [HttpPost]
        [Route("deletealldone")]
        [Authorize]
        public ApiResponse DeleteAll()
        {
            var collection = _taskCollectionProvider.GetSyncTaskCollection(_user.GetUser());
            var res = collection.DeleteAllDone();
            if (res.success)
            {
                return new ApiResponse(true);
            }
            else
            {
                return new ApiResponse(500, "DeleteTaskQueueError", res.result);
            }
        }
    }
}
