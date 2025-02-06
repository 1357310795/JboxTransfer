using AutoMapper;
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
        public ApiResponse List(string type = "transfering")
        {
            return new ApiResponse();
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
            return new ApiResponse();
        }        

        [HttpPost]
        [Route("cancelall")]
        [Authorize]
        public ApiResponse CancelAll()
        {
            return new ApiResponse();
        }

        // error queue
        [HttpPost]
        [Route("restartall")]
        [Authorize]
        public ApiResponse RestartAll(bool clearProgress = false)
        {
            return new ApiResponse();
        }

        [HttpPost]
        [Route("cancelallerr")]
        [Authorize]
        public ApiResponse CancelAllError()
        {
            return new ApiResponse();
        }

        // complete queue
        [HttpPost]
        [Route("deleteall")]
        [Authorize]
        public ApiResponse DeleteAll()
        {
            return new ApiResponse();
        }
    }
}
