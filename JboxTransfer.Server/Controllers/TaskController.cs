using JboxTransfer.Server.Modules.DataWrapper;
using JboxTransfer.Server.Modules.Db;
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
        private readonly DefaultDbContext _context;
        public TaskController(ILogger<TaskController> logger, IMemoryCache memoryCache, DefaultDbContext context)
        {
            _logger = logger;
            _mcache = memoryCache;
            _context = context;
        }

        [HttpGet]
        [Route("info")]
        [Authorize]
        public ApiResponse GetInfo([FromQuery]int id)
        {
            return new ApiResponse();
        }

        [HttpPost]
        [Route("pause")]
        [Authorize]
        public ApiResponse Pause([FromForm]int id)
        {
            return new ApiResponse();
        }

        [HttpPost]
        [Route("start")]
        [Authorize]
        public ApiResponse Start(int id)
        {
            return new ApiResponse();
        }

        [HttpPost]
        [Route("cancel")]
        [Authorize]
        public ApiResponse Cancel(int id)
        {
            return new ApiResponse();
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
        public ApiResponse Restart(int id, bool clearProgress = false)
        {
            return new ApiResponse();
        }
    }
}
