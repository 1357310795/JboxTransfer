using JboxTransfer.Core.Models.Db;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules.Sync
{
    public class SyncTaskCollectionProvider
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Dictionary<string, SyncTaskCollection> _dic;

        public SyncTaskCollectionProvider(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _dic = new Dictionary<string, SyncTaskCollection>();
        }

        public SyncTaskCollection GetSyncTaskCollection(SystemUser user)
        {
            if (!_dic.TryGetValue(user.Jaccount, out SyncTaskCollection? stc))
            {
                stc = new SyncTaskCollection(user, _serviceScopeFactory);
                _dic.Add(user.Jaccount, stc);
            }
            return stc;
        }
    }
}
