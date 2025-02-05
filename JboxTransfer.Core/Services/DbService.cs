﻿using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Db;
using SQLite;

namespace JboxTransfer.Core.Services
{
    public class DbService
    {
        public static SQLiteConnection db;
        public static string dbpath { get; set; }
        public static void Init(string username)
        {
            dbpath = Path.Combine(PathHelper.AppDataPath, $"{username}.db");
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
