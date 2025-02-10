using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Modules.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace JboxTransfer.Core.Modules
{
    public class SystemUserInfoProvider
    {
        private readonly ILogger<SystemUserInfoProvider> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly DefaultDbContext _db;
        private readonly IMemoryCache _mcache;
        private SystemUser? _localUser;

        public SystemUserInfoProvider(ILogger<SystemUserInfoProvider> logger, IHttpContextAccessor contextAccessor, DefaultDbContext db, IMemoryCache mcache)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _db = db;
            _mcache = mcache;
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
            if (_mcache.TryGetValue(jaccount.Value, out _localUser))
            {
                return _localUser;
            }
            _localUser = _db.Users
                .AsNoTracking()
                .FirstOrDefault(x => x.Jaccount == jaccount.Value);
            if (_localUser != null) 
                _mcache.Set(jaccount.Value, _localUser, TimeSpan.FromMinutes(5));
            return _localUser;
        }

        public void SetUser(SystemUser user)
        {
            _localUser = user;
        }

        public void ClearUser(string jaccount)
        {
            if (_mcache.TryGetValue(jaccount, out var _))
                _mcache.Remove(jaccount);
        }
    }
}
