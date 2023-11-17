using JboxTransfer.Models;
using JboxTransfer.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Services
{
    public class GlobalSyncInfoService : StorageableBase
    {
        public static GlobalSyncInfoService Default = new GlobalSyncInfoService();
        public override string FileName => "syncinfo.json";
        public override StorageMode Mode => StorageMode.AppdataFolder;
        public GlobalSyncInfo Info { get; set; }

        public void Save()
        {
            base.Save(JsonConvert.SerializeObject(Info));
        }

        public GlobalSyncInfo Read()
        {
            Info = new GlobalSyncInfo();
            try
            {
                var json = base.Read();
                Info = JsonConvert.DeserializeObject<GlobalSyncInfo>(json);
            }
            catch (Exception ex)
            {

            }
            return Info;
        }
    }
}
