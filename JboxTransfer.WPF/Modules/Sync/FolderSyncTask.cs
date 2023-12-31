﻿using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules;
using JboxTransfer.Models;
using JboxTransfer.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; dbModel.Message = value; }
        }

        private SyncTaskDbModel dbModel;

        public PauseTokenSource pts;

        public double Progress { 
            get {
                if (total == 0) return 0;
                return (double)succ / (double)total;
            } 
        }

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
            State = dbModel.State == 2 ? SyncTaskState.Error : SyncTaskState.Wait;
            pts = new PauseTokenSource();
        }

        public string GetName()
        {
            var name = path.Split('/').Last();
            return name == "" ? "根目录" : name;
        }

        public string GetPath()
        {
            return path;
        }

        public string GetParentPath()
        {
            var s = path.Split('/');
            var p = string.Join("/", s.Take(s.Length - 1));
            return p == "" ? "/" : p;
        }

        public string GetProgressStr()
        {
            return $"{succ} / {total}";
        }

        public string GetProgressTextTooltip()
        {
            return $"""
                已处理的子文件/文件夹：{succ}
                总数：{total}
                """;
        }

        public void Start()
        {
            State = SyncTaskState.Running;
            Task.Run(() => { internalStartWrap(pts); });
        }

        public void Pause()
        {
            pts.Pause();
            State = SyncTaskState.Pause;
        }

        public void Resume()
        {
            if (State != SyncTaskState.Pause && State != SyncTaskState.Error)
                return;
            pts = new PauseTokenSource();
            pts.Resume();
            State = SyncTaskState.Running;
            Task.Run(() => { internalStartWrap(pts); });
        }

        public void Cancel()
        {
            pts.Pause();
            State = SyncTaskState.Wait;
            dbModel.State = 4;
            Message = "已取消";
            DbService.db.Update(dbModel);
        }

        public void Recover(bool keepProgress)
        {
            if (keepProgress)
            {
                dbModel.State = 0;
                DbService.db.Update(dbModel);
            }
            else
            {
                dbModel.ConfirmKey = null;
                dbModel.State = 0;
                dbModel.RemainParts = null;
                DbService.db.Update(dbModel);
            }
        }

        private void internalStartWrap(PauseTokenSource inst_pts)
        {
            try
            {
                Monitor.Enter(this);
                if (inst_pts.IsPaused)
                {
                    Monitor.Exit(this);
                    State = SyncTaskState.Pause;
                    return;
                }
                internalStart(pts);
                if (inst_pts.IsPaused)
                {
                    Monitor.Exit(this);
                    State = SyncTaskState.Pause;
                    return;
                }
            }
            catch (Exception ex)
            {
                //log
                Debug.WriteLine(ex);
                Message = $"{ex.Message}";
                State = SyncTaskState.Error;
                dbModel.State = 2;
                DbService.db.Update(dbModel);
            }
            Monitor.Exit(this);
        }

        public void internalStart(PauseTokenSource inst_pts)
        {
            State = SyncTaskState.Running;

            if (inst_pts.IsPaused)
                return;

            //Create Folder in tbox
            var res0 = TboxService.CreateDirectory(path);
            if (!res0.Success)
            {
                if (res0.Result == null || (res0.Result.Code != "SameNameDirectoryOrFileExists" && res0.Result.Code != "RootDirectoryNotAllowed"))
                {
                    State = SyncTaskState.Error;
                    Message = $"创建文件夹失败：{res0.Result.Message}";
                    dbModel.State = 2;
                    DbService.db.Update(dbModel);
                    return;
                }
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

                        Monitor.Enter(DbService.db);
                        if (inst_pts.IsPaused)
                        {
                            Monitor.Exit(DbService.db);
                            return;
                        }
                        var order = DbService.GetMinOrder() - 1;
                        DbService.db.RunInTransaction(() =>
                        {
                            foreach (var item in info.Content.Where(x => x.IsDir == true))
                                DbService.db.Insert(new SyncTaskDbModel(1, item.Path, 0, order));
                            foreach (var item in info.Content.Where(x => x.IsDir == false))
                                DbService.db.Insert(new SyncTaskDbModel(0, item.Path, item.Bytes, order) { MD5_Ori = item.Hash });
                            dbModel.RemainParts = page.ToString();
                            DbService.db.Update(dbModel);
                        });
                        Monitor.Exit(DbService.db);

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
                    dbModel.State = 2;
                    Message = ex.Message;
                    DbService.db.Update(dbModel);
                    return;
                }
                if (info.Content.Length == 0)
                    break;
            }
            dbModel.State = 3;
            State = SyncTaskState.Complete;
            Message = "同步完成";
            DbService.db.Update(dbModel);
        }
    }
}
