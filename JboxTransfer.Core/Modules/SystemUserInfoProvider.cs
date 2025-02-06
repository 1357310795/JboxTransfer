using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Modules.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JboxTransfer.Core.Modules
{
    public class SystemUserInfoProvider
    {
        private readonly ILogger<SystemUserInfoProvider> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly DefaultDbContext _db;
        private SystemUser? _localUser;

        public SystemUserInfoProvider(ILogger<SystemUserInfoProvider> logger, IHttpContextAccessor contextAccessor, DefaultDbContext db)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _db = db;
        }

        public SystemUser GetUser()
        {
            if (_localUser != null)
                return _localUser;
            if (_contextAccessor.HttpContext == null)
                return null;
            var jaccount = _contextAccessor.HttpContext.User.FindFirst("jaccount");
            if (jaccount == null)
                return null;
            var user = _db.Users
                .AsNoTracking()
                .FirstOrDefault(x => x.Jaccount == jaccount.Value);
            return user;
        }

        public void SetUser(SystemUser user)
        {
            _localUser = user;
        }
    }
}
