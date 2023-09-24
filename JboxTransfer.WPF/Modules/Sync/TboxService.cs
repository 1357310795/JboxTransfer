using JboxTransfer.Core.Helpers;
using JboxTransfer.Models;
using JboxTransfer.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Teru.Code.Models;
using ZXing.QrCode.Internal;

namespace JboxTransfer.Modules.Sync
{
    public class TboxService
    {
        public const string baseUrl = "https://pan.sjtu.edu.cn";
        public static string UserToken;
        public static bool Logined;
        public static CommonResult Login()
        {
            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://pan.sjtu.edu.cn/user/v1/sign-in/sso-login-redirect/xpw8ou8y");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
            var res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult(false, $"服务器响应{res.StatusCode}");
            }

            if (res.RequestMessage.RequestUri.Host.Contains("jaccount"))
            {
                return new CommonResult(false, $"未成功认证");
            }

            var reg = new Regex("code=(.+?)&state=");
            var match = reg.Match(res.RequestMessage.RequestUri.OriginalString);

            if (!match.Success) {
                return new CommonResult(false, $"未找到回调code");
            }
            var code = match.Groups[1].Value;

            ///user/v1/sign-in/verify-account-login/xpw8ou8y
            req = new HttpRequestMessage(HttpMethod.Post, $"https://pan.sjtu.edu.cn/user/v1/sign-in/verify-account-login/xpw8ou8y?device_id=Chrome+116.0.0.0&type=sso&credential={code}");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<TboxLoginResDto>(body);

            if (json.Status != 0)
            {
                return new CommonResult(false, $"服务器返回失败：{json.Message}");
            }

            if (json.UserToken.Length != 128)
            {
                return new CommonResult(false, $"UserToken无效");
            }

            TboxService.UserToken = json.UserToken;
            Logined = true;

            return new CommonResult(true, "");
        }

        public static CommonResult<TboxSpaceCred> GetSpace()
        {
            if (!Logined)
                return new CommonResult<TboxSpaceCred>(false, $"未登录，请先登录");

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/user/v1/space/1/personal?user_token={UserToken}");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            var res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult<TboxSpaceCred>(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<TboxSpaceCred>(body);

            if (json.Status != 0)
            {
                return new CommonResult<TboxSpaceCred>(false, $"服务器返回失败：{json.Message}");
            }

            return new CommonResult<TboxSpaceCred>(true, "", json);
        }

        public static CommonResult<TboxStartChunkUploadResDto> StartChunkUpload(string path, int chunkCount)
        {
            if (!Logined)
                return new CommonResult<TboxStartChunkUploadResDto>(false, $"未登录，请先登录");

            var query = new Dictionary<string, string>();
            query.Add("multipart", null);
            query.Add("conflict_resolution_strategy", "rename");
            query.Add("access_token", TboxAccessTokenKeeper.Cred.AccessToken);

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{TboxAccessTokenKeeper.Cred.LibraryId}/{TboxAccessTokenKeeper.Cred.SpaceId}/{path}" + UrlHelper.BuildQuery(query));
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            StringBuilder sb = new StringBuilder();
            sb.Append("1");
            for (int i = 2; i <= Math.Min(chunkCount, 50); i++)
                sb.Append($",{i}");
            req.Content = new StringContent($"{{\"partNumberRange\":[{sb.ToString()}]}}");
            
            var res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<TboxStartChunkUploadResDto>(body);

            if (json.Status != 0)
            {
                return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器返回失败：{json.Message}");
            }

            return new CommonResult<TboxStartChunkUploadResDto>(true, "", json);
        }

        public static CommonResult<TboxStartChunkUploadResDto> RenewChunkUpload(string confirmKey, List<int> partNumberRange)
        {
            if (!Logined)
                return new CommonResult<TboxStartChunkUploadResDto>(false, $"未登录，请先登录");

            var query = new Dictionary<string, string>();
            query.Add("renew", null);
            query.Add("access_token", TboxAccessTokenKeeper.Cred.AccessToken);

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{TboxAccessTokenKeeper.Cred.LibraryId}/{TboxAccessTokenKeeper.Cred.SpaceId}/{confirmKey}" + UrlHelper.BuildQuery(query));
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            StringBuilder sb = new StringBuilder();
            sb.Append(partNumberRange.First());
            for (int i = 1; i < Math.Min(partNumberRange.Count, 50); i++)
                sb.Append($",{partNumberRange[i]}");
            req.Content = new StringContent($"{{\"partNumberRange\":[{sb.ToString()}]}}");

            var res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<TboxStartChunkUploadResDto>(body);

            if (json.Status != 0)
            {
                return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器返回失败：{json.Message}");
            }

            return new CommonResult<TboxStartChunkUploadResDto>(true, "", json);
        }

        public static CommonResult<TboxConfirmChunkUploadResDto> ConfirmChunkUpload(string confirmKey)
        {
            if (!Logined)
                return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"未登录，请先登录");

            var query = new Dictionary<string, string>();
            query.Add("confirm", null);
            query.Add("conflict_resolution_strategy", "rename");
            query.Add("access_token", TboxAccessTokenKeeper.Cred.AccessToken);

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{TboxAccessTokenKeeper.Cred.LibraryId}/{TboxAccessTokenKeeper.Cred.SpaceId}/{confirmKey}" + UrlHelper.BuildQuery(query));
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            var res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<TboxConfirmChunkUploadResDto>(body);

            if (json.Status != 0)
            {
                return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"服务器返回失败：{json.Message}");
            }

            return new CommonResult<TboxConfirmChunkUploadResDto>(true, "", json);
        }

        public static CommonResult<TboxChunkUploadInfoResDto> GetChunkUploadInfo(string confirmKey)
        {
            if (!Logined)
                return new CommonResult<TboxChunkUploadInfoResDto>(false, $"未登录，请先登录");

            var query = new Dictionary<string, string>();
            query.Add("upload", null);
            query.Add("no_upload_part_info", "1");
            query.Add("access_token", TboxAccessTokenKeeper.Cred.AccessToken);

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/file/{TboxAccessTokenKeeper.Cred.LibraryId}/{TboxAccessTokenKeeper.Cred.SpaceId}/{confirmKey}" + UrlHelper.BuildQuery(query));
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");

            var res = client.SendAsync(req).GetAwaiter().GetResult();

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult<TboxChunkUploadInfoResDto>(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<TboxChunkUploadInfoResDto>(body);

            if (json.Status != 0)
            {
                return new CommonResult<TboxChunkUploadInfoResDto>(false, $"服务器返回失败：{json.Message}");
            }

            return new CommonResult<TboxChunkUploadInfoResDto>(true, "", json);
        }
    }
}
