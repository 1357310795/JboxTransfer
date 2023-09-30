using JboxTransfer.Models;
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
    public class GlobalSyncInfoService
    {
        public static GlobalSyncInfo Info { get; set; }

        static GlobalSyncInfoService()
        {
            _fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "syncinfo.json");
        }

        private static string _fileName;

        public static void Save()
        {
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(Info));
        }

        public static GlobalSyncInfo Read()
        {
            Info = new GlobalSyncInfo();
            if (File.Exists(_fileName))
            {
                var json = File.ReadAllText(_fileName);
                var Info = JsonConvert.DeserializeObject<GlobalSyncInfo>(json);
            }
            return Info;
        }

    }
}
