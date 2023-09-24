using JboxTransfer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Teru.Code.Services;

namespace JboxTransfer.Modules.Sync
{
    public class TboxAccessTokenKeeper
    {
        public static TboxSpaceCred Cred;
        public static DateTime LastUpdate;
        public static PauseToken PauseToken;
        public static PauseTokenSource PauseTokenSource;

        static TboxAccessTokenKeeper()
        {
            PauseTokenSource = new PauseTokenSource();
            PauseToken = new PauseToken(PauseTokenSource);
            PauseTokenSource.Pause();
            worker = new LoopWorker();
            worker.Interval = 60 * 1000;
            worker.OnGoAnimation += () => { };
            worker.CanRun += () => true;
            worker.Go += Worker_Go;
        }

        private static TaskState Worker_Go()
        {
            try
            {
                if (Cred != null)
                {
                    var passed = (LastUpdate - DateTime.Now).TotalSeconds;
                    if (Cred.ExpiresIn - passed > 2*60)//大于两分钟
                    {
                        return TaskState.Started;
                    }
                }
                PauseTokenSource.Pause();
                var res = TboxService.GetSpace();
                if (!res.Success)
                {
                    //log
                    return TaskState.Started;
                }
                Cred = res.Result;
                PauseTokenSource.Resume();

            }
            catch(Exception ex)
            {
                //log
            }
            return TaskState.Started;
        }

        private static LoopWorker worker;

        public static void Register()
        {
            worker.StartRun();
        }

        public static void UnRegister()
        {
            worker.StopRun();
        }
    }
}
