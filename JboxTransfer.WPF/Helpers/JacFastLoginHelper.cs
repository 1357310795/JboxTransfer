using JboxTransfer.Services;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Teru.Code.Models;
using Teru.Code.Services;

namespace JboxTransfer.Helpers
{
    public class JacFastLoginHelper
    {
        #region Private 
        private CookieContainer cookieContainer;
        private string targetUri;
        private ClientWebSocket ws;
        private CancellationTokenSource tokensource;
        private LoopWorker worker = new LoopWorker();
        HttpClient client;

        public CancellationToken token
        {
            get { return tokensource.Token; }
        }

        public delegate void MessageReceivedDelegate(MemoryStream ms);
        public delegate void ScanSuccessDelegate();

        private event MessageReceivedDelegate MessageReceived;
        private event ScanSuccessDelegate ScanSuccess;
        #endregion

        #region Public
        public string Uuid { get; set; }
        public long Ts { get; set; }
        public string Sig { get; set; }

        public delegate void LoginSuccessDelegate();
        public delegate void LoginFailDelegate(string message);
        public delegate void PayloadPreparedDelegate();

        public event LoginSuccessDelegate LoginSuccess;
        public event LoginFailDelegate LoginFail;
        public event PayloadPreparedDelegate Prepared;

        public bool Failed { get; set; }
        #endregion

        #region Constructor
        public JacFastLoginHelper(CookieContainer cc, string targetUri)
        {
            cookieContainer = cc;
            this.targetUri = targetUri;
            client = NetService.Client;
            worker = new LoopWorker();
            tokensource = new CancellationTokenSource();
            ws = new ClientWebSocket();
        }
        #endregion

        #region Data
        public string GetQrcodeStr()
        {
            return $"https://jaccount.sjtu.edu.cn/jaccount/confirmscancode?uuid={Uuid}&ts={Ts}&sig={Sig}";
        }
        #endregion

        #region Liftime
        public void Start()
        {
            Task.Run(async () =>
            {
                worker = new LoopWorker();
                worker.Interval = 50 * 1000;
                worker.CanRun += () => true;
                worker.OnGoAnimation += () => { };
                worker.Go += Worker_Go;
                ws = new ClientWebSocket();
                tokensource = new CancellationTokenSource();
                MessageReceived -= JacFastLoginHelper_MessageReceived;
                MessageReceived += JacFastLoginHelper_MessageReceived;
                ScanSuccess -= JacFastLoginHelper_ScanSuccess;
                ScanSuccess += JacFastLoginHelper_ScanSuccess;

                var res1 = await GetUuid();
                if (token.IsCancellationRequested)
                    return;
                if (!res1.Success)
                {
                    Fail(res1.Message);
                    return;
                }
                var res2 = await InitWebSocket();
                if (token.IsCancellationRequested)
                    return;
                if (!res2.success)
                {
                    Fail(res2.result);
                    return;
                }
                worker.StartRun();
            });
        }

        public void Refresh()
        {
            Failed = false;
            Dispose();
            Start();
        }

        private void Fail(string message)
        {
            Failed = true;
            LoginFail.Invoke(message);
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                tokensource.Cancel();
                worker.StopRun();
                ws?.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Main
        public async Task<CommonResult<string>> GetUuid()
        {
            try
            {
                var conn = await client.GetAsync(targetUri, token);

                if (conn.StatusCode == HttpStatusCode.OK && !conn.RequestMessage.RequestUri.OriginalString.Contains("https://jaccount.sjtu.edu.cn/jaccount/jalogin"))
                {
                    tokensource.Cancel();
                    return new CommonResult<string>(true, $"已授权");
                }

                if (conn.StatusCode != HttpStatusCode.OK)
                    return new CommonResult<string>(false, $"{conn.StatusCode}");

                var sr = new StreamReader(await conn.Content.ReadAsStreamAsync());

                var html = await sr.ReadToEndAsync();

                if (html == null || html == "")
                    return new CommonResult<string>(false, $"空内容");

                var reg = new Regex(@"<input type=""hidden"" name=""uuid"" value=""(.+?)"">");
                var match = reg.Match(html);
                if (!match.Success)
                    return new CommonResult<string>(false, $"未找到uuid");

                Uuid = match.Groups[1].Value;
                return new CommonResult<string>(true, "", Uuid);
            }
            catch (Exception ex)
            {
                return new CommonResult<string>(false, $"发生异常：{ex.Message}");
            }
        }

        public async Task<CommonResult> InitWebSocket()
        {
            try
            {
                await ws.ConnectAsync(new Uri($"wss://jaccount.sjtu.edu.cn/jaccount/sub/{Uuid}"), tokensource.Token);

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        MemoryStream ms = new MemoryStream();
                        var messageBuffer = WebSocket.CreateClientBuffer(8192, 8192);
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await ws.ReceiveAsync(messageBuffer, token);
                            ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);

                        }
                        while (!result.EndOfMessage);
                        if (ms.Length != 0)
                        {
                            Task.Run(() => { MessageReceived.Invoke(ms); });
                        }
                    }
                });
#pragma warning restore CS8604 // 引用类型参数可能为 null。
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

                return new CommonResult(ws.State == WebSocketState.Open ? true : false, "");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, $"发生异常：{ex.Message}");
            }
        }

        public async Task<CommonResult> SendGetPayload()
        {
            try
            {
                string message = "{ \"type\": \"UPDATE_QR_CODE\" }";
                ArraySegment<byte> bytes = new ArraySegment<byte>(System.Text.Encoding.ASCII.GetBytes(message));
                await ws.SendAsync(bytes, WebSocketMessageType.Text, true, token);

                return new CommonResult(true, "");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, $"发生异常：{ex.Message}");
            }
        }

        private void JacFastLoginHelper_MessageReceived(MemoryStream ms)
        {
            try
            {
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                var message = sr.ReadToEnd();
                var dto = JsonConvert.DeserializeObject<LoginPayloadDto>(message);
                if (dto.Error != 0)
                    throw new Exception($"{dto.Error}");
                switch (dto.Type.ToUpper())
                {
                    case "UPDATE_QR_CODE":
                        Ts = dto.Payload.Ts;
                        Sig = dto.Payload.Sig;
                        Task.Run(Prepared.Invoke);
                        break;
                    case "LOGIN":
                        Task.Run(ScanSuccess.Invoke);
                        break;
                }
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private async void JacFastLoginHelper_ScanSuccess()
        {
            try
            {
                //client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                //client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");

                var conn = await client.GetAsync($"https://jaccount.sjtu.edu.cn/jaccount/expresslogin?uuid={Uuid}", token);

                while (conn.StatusCode == HttpStatusCode.Found && conn.Headers.Location.Scheme == "http")
                {
                    conn = await client.GetAsync(conn.Headers.Location.OriginalString.Replace("http","https"), token);
                }

                if (!conn.IsSuccessStatusCode)
                {
                    Fail($"expresslogin失败，服务器返回{conn.StatusCode}");
                    return;
                }

                if (conn.StatusCode == HttpStatusCode.OK && conn.RequestMessage.RequestUri.OriginalString.Contains("https://jaccount.sjtu.edu.cn/jaccount/jalogin"))
                {
                    Fail($"expresslogin失败，未认证");
                    return;
                }

                //var query = conn.RequestMessage.RequestUri.Query;

                //var req = new HttpRequestMessage(HttpMethod.Post, "https://ssc.sjtu.edu.cn/api/ja/login");
                //req.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                //req.Content = new StringContent($$"""
                //    {"code":"{{query.Substring(query.IndexOf("=") + 1)}}","redirectUri":"https%3A%2F%2Fssc.sjtu.edu.cn%2Fjaccount-login%3BredirectUrl%3D%252Farch%252Fhome"}
                //    """);
                //var res = client.SendAsync(req).Result;

                //if (!res.IsSuccessStatusCode)
                //{
                //    Fail($"获取用户信息失败，服务器返回{res.StatusCode}");
                //    return;
                //}

                //var regex = new Regex(@"""email"":""(.+?)@sjtu\.edu\.cn""");
                //var text = res.Content.ReadAsStringAsync().Result;
                //var match = regex.Match(text);
                //if (!match.Success)
                //{
                //    Fail($"获取用户信息失败，未找到用户名");
                //    return;
                //}

                Task.Run(() => { LoginSuccess.Invoke(); });
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private TaskState Worker_Go()
        {
            try
            {
                var res = SendGetPayload().GetAwaiter().GetResult();
                if (!res.success)
                {
                    Fail(res.result);
                    return TaskState.Error;
                }
                else
                    return TaskState.Started;
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
                return TaskState.Error;
            }
        }
        #endregion

        #region Models
        public partial class LoginPayloadDto
        {
            [JsonProperty("error")]
            public long Error { get; set; }

            [JsonProperty("payload")]
            public PayloadDto Payload { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public partial class PayloadDto
        {
            [JsonProperty("sig")]
            public string Sig { get; set; }

            [JsonProperty("ts")]
            public long Ts { get; set; }
        }
        #endregion
    }
}
