using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Core.Models;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Sync;
using JboxTransfer.Core.Services;
using JboxTransfer.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Teru.Code.Services;

namespace JboxTransfer.Views
{
    /// <summary>
    /// DebugPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class DebugPage : Page
    {
        public DebugPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        [ObservableProperty]
        private double progress;

        [ObservableProperty]
        private string text;

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private string path;

        [ObservableProperty]
        private long size;

        [ObservableProperty]
        private string hash;

        [ObservableProperty]
        private ImageSource image;

        IBaseTask task;

        private LoopWorker worker;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //worker = new LoopWorker();
            //worker.Interval = 1000;
            //worker.CanRun += () => true;
            //worker.Go += Worker_Go;
            //DbService.Init("testuser");
            var file = EmbedResHelper.GetELUA();
            //DbService.db.Query<SyncTaskDbModel>()

            //var icon = System.Drawing.Icon.ExtractAssociatedIcon(@"cccccc.idb");
            //MemoryStream ms = new MemoryStream();
            //icon.Save(ms);
            //ms.Position = 0;
            //Image = BitmapHelper.ToImageSource(ms);
            //var crc64 = CRC64.Create();
            //crc64.CRC64Hash_Proc(System.Text.Encoding.Default.GetBytes("123456"));
            //crc64.CRC64Hash_Proc(System.Text.Encoding.Default.GetBytes("123456"));
            //var hash = crc64.CRC64Hash_Finish();
            //Debug.WriteLine(hash.ToString("x"));

            //Task.Run(Download);
            //var md5 = HashHelper.MD5Hash_Start();
            //md5.MD5Hash_Proc(Encoding.Default.GetBytes("123456"));
            //var res = md5.MD5Hash_Finish();
            //StringBuilder sub = new StringBuilder();
            //foreach (var t in res)
            //{
            //    sub.Append(t.ToString("x2"));
            //}
            //Debug.WriteLine(sub.ToString());
        }

        private TaskState Worker_Go(CancellationTokenSource cts)
        {
            Thread.Sleep(1000);
            Text = task.GetProgressStr();
            Message = task.Message;
            return TaskState.Started;
        }

        public void Download()
        {
            SyncTaskDbModel item = null;
            DbService.db.RunInTransaction(() =>
            {
                item = DbService.db.Table<SyncTaskDbModel>().OrderBy(x=>x.Order).Where(x=>x.State == 0).First(); //.Where(x => x.State == 0)
                item.State = 1;
                DbService.db.Update(item);
            });

            if (item.Type == 0)
            {
                task = new FileSyncTask(item);
                task.Start();
            }
            else
            {
                task = new FolderSyncTask(item);
                task.Start();
            }
            
            //var client = NetService.Client;
            //var req = new HttpRequestMessage(HttpMethod.Connect, "http://10.119.4.90:443/auto.exe");
            //var res = client.SendAsync(req).Result;

            //HttpWebRequest req = HttpWebRequest.CreateHttp("http://10.119.4.90:443/auto.exe");
            //req.Method = "GET";
            //var resp = req.GetResponse() as HttpWebResponse;

            //var stream = resp.GetResponseStream();
            //len = resp.ContentLength;

            //MyStream ms = new MyStream();
            //ms.OnWrite += Ms_OnWrite;
            ////stream.CopyTo(ms);

            //byte[] buffer = ArrayPool<byte>.Shared.Rent(81920/2);
            //try
            //{
            //    int bytesRead;
            //    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            //    {
            //        ms.Write(buffer, 0, bytesRead);
            //    }
            //}
            //finally
            //{
            //    ArrayPool<byte>.Shared.Return(buffer);
            //}

        }

        private void Listen()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Text = task.GetProgressStr();
                Message = task.Message;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MD5 md5 = MD5.Create();
            var file =  File.ReadAllBytes(@"C:\Users\13573\Downloads\touch_payload.bin");
            md5.TransformBlock(file, 0, file.Length);

            var json = md5.GetValue();
            var jsonstr = JsonConvert.SerializeObject(json);
            json = JsonConvert.DeserializeObject<MD5StateStorage>(jsonstr);

            md5 = MD5.Create(json);
            md5.TransformBlock(file, 0, file.Length);

            var hash = md5.TransformFinalBlock();
            StringBuilder sub = new StringBuilder();
            foreach (var t in hash)
            {
                sub.Append(t.ToString("x2"));
            }
            Debug.WriteLine(sub.ToString());
            //Debug.WriteLine(TboxAccessTokenKeeper.Cred.SpaceId);

            //var res = TboxService.StartChunkUpload("finale2.mp4", 50);
            //FileSyncTask task = new FileSyncTask("/final2.mp4", "a2959e6affa4f4cf1baa1d74b0e07afc", 111515990);
            //task.Start();
            //while (true)
            //{
            //    Thread.Sleep(1000);
            //    Text = task.GetProgressStr();
            //    Message = task.Message;
            //}
        }

        private void ButtonGetInfo_Click(object sender, RoutedEventArgs e)
        {
            var res = JboxService.GetJboxFileInfo(Path);
            if (!res.Success)
            {
                Message = res.Message; 
                return;
            }
            Hash = res.Result.Hash;
            Size = res.Result.Bytes;


            var order = DbService.GetMinOrder() - 1;
            DbService.db.Insert(new SyncTaskDbModel(res.Result.IsDir ? 1 : 0, res.Result.Path, Size, order) { MD5_Ori = Hash});
            Message = $"插入数据库成功";
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Download();
            worker.StartRun();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            task.Pause();
        }

        private void ButtonResume_Click(object sender, RoutedEventArgs e)
        {
            task.Resume();
        }

        private void ButtonGetIcon_Click(object sender, RoutedEventArgs e)
        {
            var icon = IconHelper.FindIconForFilename(Path, true);
            Image = icon;

        }

        //private void Ms_OnWrite(long off)
        //{
        //    Progress = (double)off / (double)len;
        //}
    }

    public class MyStream : MemoryStream
    {
        public delegate void OnWriteHandler(long off);
        public event OnWriteHandler OnWrite;

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            Thread.Sleep(1009);
            OnWrite(this.Position);
        }
    }
}
