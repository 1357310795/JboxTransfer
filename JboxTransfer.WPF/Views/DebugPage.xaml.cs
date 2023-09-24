﻿using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules;
using JboxTransfer.Modules.Sync;
using JboxTransfer.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        FileSyncTask task;

        private long len;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var crc64 = CRC64.Create();
            crc64.CRC64Hash_Proc(System.Text.Encoding.Default.GetBytes("123456"));
            crc64.CRC64Hash_Proc(System.Text.Encoding.Default.GetBytes("123456"));
            var hash = crc64.CRC64Hash_Finish();
            Debug.WriteLine(hash.ToString("x"));

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

        public void Download()
        {
            task = new FileSyncTask(Path, Hash, Size);
            task.Start();
            
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
            Debug.WriteLine(TboxAccessTokenKeeper.Cred.SpaceId);

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
            var res = JboxService.GetJboxItemInfo(Path);
            if (!res.Success)
            {
                Message = res.Message; 
                return;
            }
            Hash = res.Result.Hash;
            Size = res.Result.Bytes;
            Message = $"获取信息成功，大小为{Size}";
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Download();
            Task.Run(Listen);
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            task.Parse();
        }

        private void ButtonResume_Click(object sender, RoutedEventArgs e)
        {
            task.Resume();
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
