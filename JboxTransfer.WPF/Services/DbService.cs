using JboxTransfer.Core.Helpers;
using JboxTransfer.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Services
{
    public class DbService
    {
        public static SQLiteConnection db;
        public static string dbpath { get; set; }
        public static void Init(string username)
        {
            dbpath = Path.Combine(PathHelper.AppPath, $"{username}.db");
            Directory.CreateDirectory(PathHelper.AppDataPath);
            db = new SQLiteConnection(dbpath);
            db.CreateTable<SyncTaskDbModel>();
        }

        public static int GetMinOrder()
        {
            var res = db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).FirstOrDefault();
            if (res == null)
                return 0;
            else
                return res.Order;
        }

        public static int GetMaxOrder()
        {
            var res = db.Table<SyncTaskDbModel>().OrderByDescending(x => x.Order).FirstOrDefault();
            if (res == null)
                return 0;
            else
                return res.Order;
        }
    }
}
