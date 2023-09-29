using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules;
using JboxTransfer.Models;
using JboxTransfer.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Models;

namespace JboxTransfer.Modules.Sync
{
    public class FolderSyncTask : IBaseTask
    {
        private const int RetryTimes = 3;
        private string path;
        private Exception ex;
        private int total;
        private int succ;
        private int page;
        private JboxItemInfo info;
        public SyncTaskState State { get; set; }
        public string Message { get; set; }

        private SyncTaskDbModel dbModel;

        public PauseTokenSource pts;

        public double Progress { get; set; }

        //Only for debug
        public FolderSyncTask(string path, string hash, long size)
        {
            this.path = path;
            this.page = 0;
            this.total = this.succ = 0;
            State = SyncTaskState.Wait;
            pts = new PauseTokenSource();
        }

        public FolderSyncTask(SyncTaskDbModel dbModel)
        {
            this.dbModel = dbModel;
            if (dbModel.RemainParts == null)
            {
                this.page = 0;
                this.succ = 0;
            }
            else
            {
                this.page = int.Parse(dbModel.RemainParts);
                this.succ = this.page * 50;
            }
            this.total = 0;
            this.path = dbModel.FilePath;
            State = SyncTaskState.Wait;
            pts = new PauseTokenSource();
        }

        public string GetProgressStr()
        {
            return $"{succ} / {total}";
        }

        public void Start()
        {
            Task.Run(internalStart);
        }

        public void Parse()
        {
            pts.Pause();
            State = SyncTaskState.Parse;
        }

        public void Resume()
        {
            if (State != SyncTaskState.Parse)
                return;
            pts = new PauseTokenSource();
            pts.Resume();
            Task.Run(internalStart);
        }

        public void internalStart()
        {
            var inst_pts = pts;
            State = SyncTaskState.Running;

            if (inst_pts.IsPaused)
                return;

            //Create Folder in tbox
            var res0 = TboxService.CreateDirectory(path);
            if (!res0.Success && res0.Result.Code != "SameNameDirectoryOrFileExists")
            {
                State = SyncTaskState.Error;
                Message = $"创建文件夹失败：{res0.Result.Message}";
                return;
            }

            while (true)
            {
                var t = RetryTimes;
                while (t-- > 0)
                {
                    try
                    {
                        if (inst_pts.IsPaused)
                            return;

                        var res = JboxService.GetJboxFolderInfo(path, page);
                        info = res.Result;

                        if (!res.Success)
                            throw new Exception($"获取文件夹信息失败：{res.Message}");

                        total = (int)info.ContentSize;

                        if (info.Content.Length == 0)
                            break;

                        var order = DbService.GetMinOrder() - 1;
                        DbService.db.BeginTransaction();
                        foreach (var item in info.Content.Where(x => x.IsDir == true))
                            DbService.db.Insert(new SyncTaskDbModel(1, item.Path, 0, order));
                        foreach (var item in info.Content.Where(x => x.IsDir == false))
                            DbService.db.Insert(new SyncTaskDbModel(0, item.Path, item.Bytes, order) { MD5_Ori = item.Hash});
                        dbModel.RemainParts = page.ToString();
                        DbService.db.Update(dbModel);
                        DbService.db.Commit();

                        succ += info.Content.Length;
                        page++;
                        break;
                    }
                    catch (Exception ex)
                    {
                        this.ex = ex;
                    }
                }
                if (t <= 0)
                {
                    State = SyncTaskState.Error;
                    Message = ex.Message;
                    return;
                }
                if (info.Content.Length == 0)
                    break;
            }
            State = SyncTaskState.Complete;
            Message = "同步完成";
        }
    }
}
