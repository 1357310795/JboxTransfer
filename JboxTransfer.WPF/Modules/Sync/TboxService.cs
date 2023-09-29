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
using Teru.Code.Extensions;
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

            try
            {
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
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public static CommonResult<TboxSpaceCred> GetSpace()
        {
            if (!Logined)
                return new CommonResult<TboxSpaceCred>(false, $"未登录，请先登录");

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/user/v1/space/1/personal?user_token={UserToken}");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxSpaceCred>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxSpaceCred>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxSpaceCred>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxSpaceCred>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxSpaceCred>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxSpaceCred>(false, ex.Message);
            }
            
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
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{TboxAccessTokenKeeper.Cred.LibraryId}/{TboxAccessTokenKeeper.Cred.SpaceId}/{path.UrlEncodeByParts()}" + UrlHelper.BuildQuery(query));

            StringBuilder sb = new StringBuilder();
            sb.Append("1");
            for (int i = 2; i <= Math.Min(chunkCount, 50); i++)
                sb.Append($",{i}");
            req.Content = new StringContent($"{{\"partNumberRange\":[{sb.ToString()}]}}");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxStartChunkUploadResDto>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxStartChunkUploadResDto>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxStartChunkUploadResDto>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxStartChunkUploadResDto>(false, ex.Message);
            }
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

            if (partNumberRange == null || partNumberRange.Count == 0)
                return new CommonResult<TboxStartChunkUploadResDto>(false, "partNumberRange 为空");

            StringBuilder sb = new StringBuilder();
            sb.Append(partNumberRange.First());
            for (int i = 1; i < Math.Min(partNumberRange.Count, 50); i++)
                sb.Append($",{partNumberRange[i]}");
            req.Content = new StringContent($"{{\"partNumberRange\":[{sb.ToString()}]}}");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxStartChunkUploadResDto>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxStartChunkUploadResDto>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxStartChunkUploadResDto>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxStartChunkUploadResDto>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxStartChunkUploadResDto>(false, ex.Message);
            }
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

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxConfirmChunkUploadResDto>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxConfirmChunkUploadResDto>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxConfirmChunkUploadResDto>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxConfirmChunkUploadResDto>(false, ex.Message);
            }
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

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxChunkUploadInfoResDto>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxChunkUploadInfoResDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxChunkUploadInfoResDto>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxChunkUploadInfoResDto>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxChunkUploadInfoResDto>(true, "", json);
            }
            catch(Exception ex)
            {
                return new CommonResult<TboxChunkUploadInfoResDto>(false, ex.Message);
            }
        }

        public static CommonResult<TboxErrorMessageDto> CreateDirectory(string dirpath)
        {
            if (!Logined)
                return new CommonResult<TboxErrorMessageDto>(false, $"未登录，请先登录");

            var query = new Dictionary<string, string>();
            query.Add("conflict_resolution_strategy", "ask");
            query.Add("access_token", TboxAccessTokenKeeper.Cred.AccessToken);

            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, baseUrl + $"/api/v1/directory/{TboxAccessTokenKeeper.Cred.LibraryId}/{TboxAccessTokenKeeper.Cred.SpaceId}/{dirpath}" + UrlHelper.BuildQuery(query));

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxErrorMessageDto>(false, $"{errjson.Message}", errjson);
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxErrorMessageDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxErrorMessageDto>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxErrorMessageDto>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxErrorMessageDto>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxErrorMessageDto>(false, ex.Message);
            }
        }
    }
}
