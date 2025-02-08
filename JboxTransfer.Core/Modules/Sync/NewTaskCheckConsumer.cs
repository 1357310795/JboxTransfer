using JboxTransfer.Core.Models.Message;
using JboxTransfer.Core.Modules.Db;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules.Sync
{
    public class NewTaskCheckConsumer : IConsumer<NewTaskCheckMessage>
    {
        private readonly ILogger<NewTaskCheckConsumer> _logger;
        private readonly SyncTaskCollectionProvider _syncTaskCollectionProvider;
        private readonly DefaultDbContext _db;

        public NewTaskCheckConsumer(ILogger<NewTaskCheckConsumer> logger, SyncTaskCollectionProvider syncTaskCollectionProvider, DefaultDbContext db)
        {
            _logger = logger;
            _syncTaskCollectionProvider = syncTaskCollectionProvider;
            _db = db;
        }

        public Task Consume(ConsumeContext<NewTaskCheckMessage> context)
        {
            var taskCollection = _syncTaskCollectionProvider.GetRequiredSyncTaskCollection(context.Message.UserId);
            Task.Run(() => { taskCollection.UpdateFromDb(); });
            return Task.CompletedTask;
        }
    }
}
