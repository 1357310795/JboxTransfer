using AutoMapper;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules;
using Microsoft.AspNetCore.Mvc;
using JboxTransfer.Core.Modules.Tbox;
using Microsoft.AspNetCore.Authorization;
using JboxTransfer.Server.Modules.DataWrapper;
using JboxTransfer.Core.Models.Output;
using JboxTransfer.Core.Helpers;
using System.Linq;
using JboxTransfer.Core.Modules.Db;

namespace JboxTransfer.Server.Controllers
{
    [ApiController]
    [Route("api/v1/cloud")]
    public class CloudController : ControllerBase
    {
        private readonly ILogger<CloudController> _logger;
        private readonly SystemUserInfoProvider _user;
        private readonly JboxService _jbox;
        private readonly TboxService _tbox;
        private readonly IMapper _mapper;
        private readonly DefaultDbContext _db;

        public CloudController(ILogger<CloudController> logger, SystemUserInfoProvider user, JboxService jbox, TboxService tbox, IMapper mapper, DefaultDbContext db)
        {
            _logger = logger;
            _user = user;
            _jbox = jbox;
            _tbox = tbox;
            _mapper = mapper;
            _db = db;
        }

        [HttpGet]
        [Route("jbox/list")]
        [Authorize]
        public ApiResponse GetJboxFileList([FromQuery] string path, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var res = _jbox.GetJboxFolderInfo(path, page - 1, pageSize);
            if (!res.Success)
            {
                return new ApiResponse(500, "GetJboxFileListError", $"获取目录列表失败：{res.Message}");
            }
            //Todo: 优化
            var dbStates = new List<string>(res.Result.Content.Count());
            foreach (var item in res.Result.Content)
            {
                var dbItem = _db.SyncTasks
                    .Where(x => x.UserId == _user.GetUser().Id)
                    .Where(x => x.FilePath == "/" + item.Path)
                    .FirstOrDefault();
                dbStates.Add(dbItem != null ? dbItem.State.ToString() : "None");
            }
            var resout = new FileSystemItemInfoOutputDto(
                res.Result.Path.PathToName(),
                res.Result.Path,
                null,
                res.Result.Modified,
                res.Result.Content.Select((x, i) => x.IsDir ? new FileSystemItemInfoOutputDto(
                    x.Path.PathToName(),
                    x.Path,
                    null,
                    x.Modified,
                    null,
                    0,
                    dbStates[i]
                ) : new FileSystemItemInfoOutputDto(
                    x.Path.PathToName(),
                    x.Path,
                    x.Bytes,
                    null,
                    x.Modified,
                    dbStates[i]
                )).ToList(),
                res.Result.ContentSize
            );
            return new ApiResponse(resout);
        }

        [HttpGet]
        [Route("tbox/list")]
        [Authorize]
        public ApiResponse GetTboxFileList([FromQuery] string path, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var res = _tbox.ListItems(path, page, pageSize);
            if (!res.Success)
            {
                return new ApiResponse(500, "GetTboxFileListError", $"获取目录列表失败：{res.Message}");
            }
            //Todo: 优化
            var dbStates = new List<string>(res.Result.Contents.Count);
            foreach (var item in res.Result.Contents)
            {
                var dbItem = _db.SyncTasks
                    .Where(x => x.UserId == _user.GetUser().Id)
                    .Where(x => x.FilePath == "/" + item.Path.Connect("/"))
                    .FirstOrDefault();
                dbStates.Add(dbItem != null ? dbItem.State.ToString() : "None");
            }
            var resout = new FileSystemItemInfoOutputDto(
                res.Result.Path.LastOrDefault() ?? "根目录",
                "/" + res.Result.Path.Connect("/"),
                null,
                null,
                res.Result.Contents.Select((x, i) => x.Type == "dir" ? new FileSystemItemInfoOutputDto(
                    x.Path.LastOrDefault() ?? "根目录",
                    "/" + x.Path.Connect("/"),
                    x.CreationTime,
                    x.ModificationTime,
                    null,
                    0,
                    dbStates[i]
                ) : new FileSystemItemInfoOutputDto(
                    x.Path.Last(),
                    "/" + x.Path.Connect("/"),
                    long.Parse(x.Size),
                    x.CreationTime,
                    x.ModificationTime,
                    dbStates[i]
                )).ToList(),
                res.Result.TotalNum
            );
            return new ApiResponse(resout);
        }

        [HttpGet]
        [Route("jbox/link")]
        [Authorize]
        public ApiResponse GetJboxLink([FromQuery] string path)
        {
            var res = _jbox.GetJboxItemInfo(path.GetParentPath());
            if (!res.Success)
            {
                return new ApiResponse(500, "InternalError", $"获取信息失败：{res.Message}");
            }
            if (!res.Result.IsDir)
            {
                return new ApiResponse(400, "DirectoryNotFound", $"找不到父文件夹");
            }

            return new ApiResponse($"https://jbox.sjtu.edu.cn/v/list/self/{res.Result.Neid}");
        }

        [HttpGet]
        [Route("tbox/link")]
        [Authorize]
        public ApiResponse GetTboxLink([FromQuery] string path)
        {
            var res = _tbox.GetItemInfo(path.GetParentPath());
            if (!res.Success)
            {
                return new ApiResponse(500, "InternalError", $"获取信息失败：{res.Message}");
            }
            if (res.Result.Type != "dir")
            {
                return new ApiResponse(400, "DirectoryNotFound", $"找不到父文件夹");
            }
            var remotepath = res.Result.Path.Select(x => x.UrlEncode()).Connect("/");

            return new ApiResponse($"https://pan.sjtu.edu.cn/web/desktop/personalSpace?path={remotepath}");
        }
    }
}
