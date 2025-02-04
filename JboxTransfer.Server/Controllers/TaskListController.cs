using JboxTransfer.Core.Modules.Db;
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
        private readonly DefaultDbContext _context;
        public TaskListController(ILogger<TaskListController> logger, IMemoryCache memoryCache, DefaultDbContext context)
        {
            _logger = logger;
            _mcache = memoryCache;
            _context = context;
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
            return new ApiResponse();
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
