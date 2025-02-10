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
        private readonly Dictionary<int, SyncTaskCollection> _dic;

        public SyncTaskCollectionProvider(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _dic = new Dictionary<int, SyncTaskCollection>();
        }

        public SyncTaskCollection GetSyncTaskCollection(SystemUser user)
        {
            lock (_dic)
            {
                if (!_dic.TryGetValue(user.Id, out SyncTaskCollection? stc))
                {
                    stc = new SyncTaskCollection(user, _serviceScopeFactory);
                    _dic.Add(user.Id, stc);
                }
                return stc;
            }
        }        
        
        public SyncTaskCollection GetRequiredSyncTaskCollection(int userId)
        {
            lock (_dic)
            {
                if (!_dic.TryGetValue(userId, out SyncTaskCollection? stc))
                {
                    throw new Exception("SyncTaskCollection not found");
                }
                return stc;
            }
        } 

        public void RemoveCollection(int userId)
        {
            lock (_dic)
            {
                if (_dic.TryGetValue(userId, out SyncTaskCollection stc))
                {
                    stc.PauseAll();
                    _dic.Remove(userId);
                }
            }
        }
    }
}
