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
        public static string dbpath => Path.Combine(PathHelper.AppPath, "sqlite.db");
        public static void Init()
        {
            db = new SQLiteConnection(dbpath);
            db.CreateTable<SyncTaskDbModel>();
        }

        public static int GetMinOrder()
        {
            var res = db.Table<SyncTaskDbModel>().OrderBy(x => x.Order).Take(1).FirstOrDefault();
            if (res == null)
                return 0;
            else
                return res.Order;
        }

        public static int GetMaxOrder()
        {
            var res = db.Table<SyncTaskDbModel>().OrderByDescending(x => x.Order).Take(1).FirstOrDefault();
            if (res == null)
                return 0;
            else
                return res.Order;
        }
    }
}
