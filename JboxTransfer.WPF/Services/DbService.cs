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
    }
}
