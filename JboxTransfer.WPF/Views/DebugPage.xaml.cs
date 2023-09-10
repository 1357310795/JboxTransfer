using CommunityToolkit.Mvvm.ComponentModel;
using JboxTransfer.Core.Helpers;
using JboxTransfer.Services;
using System;
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

        private long len;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Task.Run(Download);
            var md5 = HashHelper.MD5Hash_Start();
            md5.MD5Hash_Proc(Encoding.Default.GetBytes("123456"));
            var res = md5.MD5Hash_Finish();
            StringBuilder sub = new StringBuilder();
            foreach (var t in res)
            {
                sub.Append(t.ToString("x2"));
            }
            Debug.WriteLine(sub.ToString());
        }

        public void Download()
        {
            //var client = NetService.Client;
            //var req = new HttpRequestMessage(HttpMethod.Connect, "http://10.119.4.90:443/auto.exe");
            //var res = client.SendAsync(req).Result;

            HttpWebRequest req = HttpWebRequest.CreateHttp("http://10.119.4.90:443/auto.exe");
            req.Method = "GET";
            var resp = req.GetResponse() as HttpWebResponse;

            var stream = resp.GetResponseStream();
            len = resp.ContentLength;

            MyStream ms = new MyStream();
            ms.OnWrite += Ms_OnWrite;
            stream.CopyTo(ms);


        }

        private void Ms_OnWrite(long off)
        {
            Progress = (double)off / (double)len;
        }
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
