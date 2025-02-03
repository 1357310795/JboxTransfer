using System.Net.WebSockets;
using System.Net;
using System;
using System.Text.RegularExpressions;
using Teru.Code.Models;
using Newtonsoft.Json;
using JboxTransfer.Server.Models.User;

namespace JboxTransfer.Server.Services
{
    public class JaccountFastLoginService
    {
        private CookieContainer cookieContainer;
        private string targetUri;
        private ClientWebSocket ws;
        private HttpClient client;
        private CancellationTokenSource tokensource;
        private TaskCompletionSource prepared;
        private TaskCompletionSource failed;
        private TaskCompletionSource<string> logined;

        public bool Failed => failed.Task.IsCompleted;
        public bool Logined => logined.Task.IsCompleted;
        public bool Prepared => prepared.Task.IsCompleted;

        public CancellationToken token
        {
            get { return tokensource.Token; }
        }

        public string Uuid { get; set; }
        public long Ts { get; set; }
        public string Sig { get; set; }

        public string Message { get; set; }

        public JaccountFastLoginService()
        {
            cookieContainer = new CookieContainer();
            targetUri = "https://my.sjtu.edu.cn/ui/appmyinfo";
            client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = cookieContainer, UseCookies = true, MaxAutomaticRedirections = 10, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
            ws = new ClientWebSocket();
            tokensource = new CancellationTokenSource();
            prepared = new TaskCompletionSource();
            logined = new TaskCompletionSource<string>();
            failed = new TaskCompletionSource();
        }

        public async Task<CommonResult<string>> GetUuid()
        {
            try
            {
                var conn = await client.GetAsync(targetUri);

                if (conn.StatusCode == HttpStatusCode.OK && !conn.RequestMessage.RequestUri.OriginalString.Contains("https://jaccount.sjtu.edu.cn/jaccount/jalogin"))
                {
                    return new CommonResult<string>(true, $"已授权");
                }

                if (conn.StatusCode != HttpStatusCode.OK)
                {
                    failed.SetResult();
                    Message = $"获取uuid时服务器返回{conn.StatusCode}";
                    return new CommonResult<string>(false, $"{conn.StatusCode}");
                }

                var sr = new StreamReader(await conn.Content.ReadAsStreamAsync());

                var html = await sr.ReadToEndAsync();

                if (html == null || html == "")
                {
                    failed.SetResult();
                    Message = $"获取uuid时服务器返回空内容";
                    return new CommonResult<string>(false, $"空内容");
                }

                var reg = new Regex(@"uuid: ""(.+?)""");
                var match = reg.Match(html);
                if (!match.Success)
                {
                    failed.SetResult();
                    Message = $"未找到uuid";
                    return new CommonResult<string>(false, $"未找到uuid");
                }

                Uuid = match.Groups[1].Value;
                return new CommonResult<string>(true, "", Uuid);
            }
            catch (Exception ex)
            {
                failed.SetResult();
                Message = ex.Message;
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
                            Task.Run(() => { MessageReceived(ms); });
                        }
                    }
                });
#pragma warning restore CS8604 // 引用类型参数可能为 null。
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

                return new CommonResult(ws.State == WebSocketState.Open ? true : false, "");
            }
            catch (Exception ex)
            {
                failed.SetResult();
                Message = ex.Message;
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
                failed.SetResult();
                Message = ex.Message;
                return new CommonResult(false, $"发生异常：{ex.Message}");
            }
        }

        private void MessageReceived(MemoryStream ms)
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
                        prepared.SetResult();
                        break;
                    case "LOGIN":
                        Task.Run(ScanSuccess);
                        break;
                }
            }
            catch (Exception ex)
            {
                failed.SetResult();
                Message = ex.Message;
            }
        }

        public string GetQrcodeStr()
        {
            return $"https://jaccount.sjtu.edu.cn/jaccount/confirmscancode?uuid={Uuid}&ts={Ts}&sig={Sig}";
        }

        private async void ScanSuccess()
        {
            try
            {
                var conn = await client.GetAsync($"https://jaccount.sjtu.edu.cn/jaccount/expresslogin?uuid={Uuid}", token);

                while (conn.StatusCode == HttpStatusCode.Found && conn.Headers.Location.Scheme == "http")
                {
                    conn = await client.GetAsync(conn.Headers.Location, token);
                }

                if (!conn.IsSuccessStatusCode)
                {
                    failed.SetResult();
                    Message = $"expresslogin失败，服务器返回{conn.StatusCode}";
                    return;
                }

                if (conn.StatusCode == HttpStatusCode.OK && conn.RequestMessage.RequestUri.OriginalString.Contains("https://jaccount.sjtu.edu.cn/jaccount/jalogin"))
                {
                    failed.SetResult();
                    Message = $"expresslogin失败，未认证";
                    return;
                }

                var jaauth = cookieContainer.GetAllCookies().FirstOrDefault(x => x.Name == "JAAuthCookie");
                if (jaauth == null)
                {
                    failed.SetResult();
                    Message = $"expresslogin完成，但是找不到JAAuthCookie";
                    return;
                }

                logined.SetResult(jaauth.Value);
            }
            catch (Exception ex)
            {
                failed.SetResult();
                Message = ex.Message;
            }
        }

        public CommonResult<UserInfoEntity> GetUserInfo()
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://my.sjtu.edu.cn/api/resource/my/info");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                while (res.StatusCode == HttpStatusCode.Found && res.Headers.Location.Scheme == "http")
                {
                    req = new HttpRequestMessage(HttpMethod.Get, res.Headers.Location);
                    req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                    req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                    req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                    res = client.SendAsync(req).GetAwaiter().GetResult();
                }

                if (!res.IsSuccessStatusCode)
                {
                    return new(false, $"服务器响应{res.StatusCode}");
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<UserInfoDto>(body);

                if (json.Errno != 0)
                {
                    return new(false, $"服务器返回错误：{json.Error}");
                }

                return new(true, "", json.Entities);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public void WaitForQrCode()
        {
            Task.WaitAny([prepared.Task, failed.Task], 5000);
        }

        public void WaitForLogined()
        {
            Task.WaitAny([logined.Task, failed.Task], 3000);
        }

        public string GetCookie()
        {
            return logined.Task.Result;
        }

        public void Dispose()
        {
            try
            {
                tokensource.Cancel();
                ws?.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            }
            catch (Exception ex)
            {

            }
        }

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
