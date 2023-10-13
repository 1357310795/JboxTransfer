using JboxTransfer.Core.Helpers;
using JboxTransfer.Models;
using JboxTransfer.Services;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Extensions;
using Teru.Code.Models;
using Teru.Code.Services;

namespace JboxTransfer.Modules.Sync
{
    public class JboxService
    {
        public const string baseUrl = "https://jbox.sjtu.edu.cn";
        public static string S;
        public static bool Logined;
        public static CommonResult Login()
        {
            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://jbox.sjtu.edu.cn");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new CommonResult(false, $"服务器响应{res.StatusCode}");
                }

                if (res.RequestMessage.RequestUri.Host.Contains("jaccount"))
                {
                    return new CommonResult(false, $"未成功认证");
                }

                var body = res.Content.ReadAsStringAsync().Result;

                if (body.Contains("vpn", StringComparison.OrdinalIgnoreCase))
                {
                    return new CommonResult(false, $"校外访问");
                }

                var cookies = GlobalCookie.CookieContainer.GetCookies(new Uri("https://jbox.sjtu.edu.cn"));
                var Sc = cookies.FirstOrDefault(x => x.Name == "S");
                if (Sc != null)
                {
                    S = Sc.Value;
                    Logined = true;
                }
                else
                    return new CommonResult(false, "找不到 Cookie");
                return new CommonResult(true, "");
            }
            catch(Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public static CommonResult<MemoryStream> DownloadChunk(string path, long start, long size, Pack<long> chunkProgress)
        {
            if (size == 0) return new CommonResult<MemoryStream>(true, "", new MemoryStream());
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("path_type", "self");
            form.Add("S", S);

            string url = "https://jbox.sjtu.edu.cn:10081/v2/files/databox";
            url += path.UrlEncodeByParts();
            StringBuilder sb = new StringBuilder();
            sb.Append(url);

            if (form.Count > 0)
            {
                sb.Append("?");
                int i = 0;
                foreach (var item in form)
                {
                    if (i > 0)
                        sb.Append("&");
                    sb.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
            }
            url = sb.ToString();

            HttpWebRequest req = HttpWebRequest.CreateHttp(url);
            req.Method = "GET";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76";
            req.Referer = "https://jbox.sjtu.edu.cn/";
            req.AddRange(start, start + size - 1);
            req.CookieContainer = GlobalCookie.CookieContainer;
            //req.Content = new FormUrlEncodedContent(form);

            var resp = req.GetResponse();
            if (resp.ContentType == "text/html" && resp.ContentLength == 867)
            {
                return new CommonResult<MemoryStream>(false, "下载时返回数据类型错误，您可能未登录或者在非校园网环境下");
            }
            var body = resp.GetResponseStream();

            MemoryStream ms = new MemoryStream();

            chunkProgress.Value = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(81920 / 2);
            try
            {
                int bytesRead;
                while ((bytesRead = body.Read(buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                    chunkProgress.Value += bytesRead;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new CommonResult<MemoryStream>(true, "", ms);
        }

        public static CommonResult<JboxItemInfo> GetJboxFileInfo(string path)
        {
            if (!Logined)
                return new CommonResult<JboxItemInfo>(false, $"未登录，请先登录");

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/v2/metadata_page/databox" + path.UrlEncodeByParts() + $"?S={S}");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

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

        public static CommonResult<JboxItemInfo> GetJboxFolderInfo(string path, int page)
        {
            if (!Logined)
                return new CommonResult<JboxItemInfo>(false, $"未登录，请先登录");

            Dictionary<string, string> forms = new Dictionary<string, string>();
            forms.Add("path_type", "self");
            forms.Add("page_size", "50");
            forms.Add("page_num", page.ToString());
            forms.Add("target_path", path);

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/v2/metadata_page/databox" + $"?S={S}");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
            req.Content = new FormUrlEncodedContent(forms);

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

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
            catch(Exception ex)
            {
                return new CommonResult<JboxItemInfo>(false, $"{ex.Message}");
            }
        }
    }
}
