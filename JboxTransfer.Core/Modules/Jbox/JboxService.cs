using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Jbox;
using JboxTransfer.Core.Models.Tbox;
using JboxTransfer.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Buffers;
using System.Net;
using System.Net.Http.Headers;
using TboxWebdav.Server.Modules;
using Teru.Code.Models;

namespace JboxTransfer.Core.Modules.Jbox
{
    public class JboxService
    {
        public const string baseUrl = "https://jbox.sjtu.edu.cn";

        private readonly ILogger<JboxService> _logger;
        private readonly HttpClient _client;
        private readonly JboxCredProvider _credProvider;
        private readonly SystemUserInfoProvider _user;
        private readonly HttpClientFactory _clientFactory;
        private readonly CookieContainerProvider _ccProvider;

        public JboxService(ILogger<JboxService> logger, JboxCredProvider credProvider, SystemUserInfoProvider user, HttpClientFactory clientFactory, CookieContainerProvider ccProvider)
        {
            _logger = logger;
            _credProvider = credProvider;
            _user = user;
            _clientFactory = clientFactory;
            _client = _clientFactory.CreateClient();
            _ccProvider = ccProvider;
        }

        public CommonResult<JboxCredInfo> Login()
        {
            try
            {
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://jbox.sjtu.edu.cn");
                req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new (false, $"服务器响应{res.StatusCode}");
                }

                if (res.RequestMessage.RequestUri.Host.Contains("jaccount"))
                {
                    return new (false, $"未成功认证");
                }

                var body = res.Content.ReadAsStringAsync().Result;

                if (body.Contains("vpn", StringComparison.OrdinalIgnoreCase))
                {
                    return new (false, $"校外访问");
                }

                var cookies = _ccProvider.GetCookieContainer(_user.GetUser()).GetCookies(new Uri("https://jbox.sjtu.edu.cn"));
                var Sc = cookies.FirstOrDefault(x => x.Name == "S");
                if (Sc != null)
                {
                    var cred = new JboxCredInfo(Sc.Value);
                    return new (true, "", cred);
                }
                else
                    return new (false, "找不到 Cookie");
            }
            catch (Exception ex)
            {
                return new (false, ex.Message);
            }
        }
        
        public CommonResult<JboxUserInfo> GetUserInfo()
        {
            try
            {
                var cred = CheckLogined();
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://jbox.sjtu.edu.cn/v2/user/info/get?S=" + cred.S);
                req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new (false, $"服务器响应{res.StatusCode}");
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<JboxUserInfo>(body);

                if (json.Type == "error")
                {
                    return new (false, $"服务器返回失败：{json.Message}");
                }

                return new (true, "", json);
            }
            catch (Exception ex)
            {
                return new (false, ex.Message);
            }
        }

        public async Task<CommonResult<MemoryStream>> DownloadChunk(string path, long start, long size, Pack<long> chunkProgress, CancellationToken ct = default)
        {
            if (size == 0) return new CommonResult<MemoryStream>(true, "", new MemoryStream());
            try
            {
                var cred = CheckLogined();

                Dictionary<string, string> query = new Dictionary<string, string>();
                query.Add("path_type", "self");
                query.Add("S", cred.S);
                query.Add("target_path", path.UrlEncodeByParts());

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://jbox.sjtu.edu.cn:10081/v2/files/databox" + UriHelper.BuildQuery(query));
                req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                req.Headers.Referrer = new Uri("https://jbox.sjtu.edu.cn/");
                req.Headers.Range = new RangeHeaderValue(start, start + size - 1);

                var res = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!res.IsSuccessStatusCode)
                {
                    return new(false, $"服务器响应{res.StatusCode}");
                }

                if (res.RequestMessage.RequestUri.Host == "restrict.sjtu.edu.cn")
                {
                    return new(false, $"非校园网环境");
                }


                var body = await res.Content.ReadAsStreamAsync(ct);

                MemoryStream ms = new MemoryStream();

                chunkProgress.Value = 0;
                byte[] buffer = ArrayPool<byte>.Shared.Rent(81920 / 2);
                try
                {
                    int bytesRead;
                    while ((bytesRead = await body.ReadAsync(buffer, 0, buffer.Length, ct)) != 0)
                    {
                        await ms.WriteAsync(buffer, 0, bytesRead, ct);
                        chunkProgress.Value += bytesRead;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                return new CommonResult<MemoryStream>(true, "", ms);
            }
            catch (TaskCanceledException ex)
            {
                return new(false, "操作已取消");
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<JboxItemInfo> GetJboxItemInfo(string path)
        {
            try
            {
                var cred = CheckLogined();

                Dictionary<string, string> query = new Dictionary<string, string>();
                query.Add("S", cred.S);
                query.Add("target_path", path.UrlEncodeByParts());

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/v2/metadata_page/databox" + UriHelper.BuildQuery(query));
                req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new CommonResult<JboxItemInfo>(false, $"服务器响应{res.StatusCode}");
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<JboxItemInfo>(body);

                if (json.Type == "error")
                {
                    return new CommonResult<JboxItemInfo>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<JboxItemInfo>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<JboxItemInfo>(false, $"{ex.Message}");
            }
        }

        public CommonResult<JboxItemInfo> GetJboxFolderInfo(string path, int page, int pageSize = 50)
        {
            try
            {
                var cred = CheckLogined();
                Dictionary<string, string> forms = new Dictionary<string, string>();
                forms.Add("path_type", "self");
                forms.Add("page_size", pageSize.ToString());
                forms.Add("page_num", page.ToString());
                forms.Add("target_path", path);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/v2/metadata_page/databox" + $"?S={cred.S}");
                req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                req.Content = new FormUrlEncodedContent(forms);
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new CommonResult<JboxItemInfo>(false, $"服务器响应{res.StatusCode}");
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<JboxItemInfo>(body);

                if (json.Type == "error")
                {
                    return new CommonResult<JboxItemInfo>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<JboxItemInfo>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<JboxItemInfo>(false, $"{ex.Message}");
            }
        }

        private JboxCredInfo CheckLogined()
        {
            var cred = _credProvider.GetCred();
            if (cred == null)
            {
                var res = Login();
                if (!res.Success)
                    throw new Exception(res.Message);
                else
                {
                    cred = res.Result;
                    _credProvider.SetCred(cred);
                }
            }
            return cred;
        }
    }
}
