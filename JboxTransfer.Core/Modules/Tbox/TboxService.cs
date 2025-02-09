using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models.Tbox;
using JboxTransfer.Core.Modules.Jbox;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using TboxWebdav.Server.Modules;
using Teru.Code.Models;

namespace JboxTransfer.Core.Modules.Tbox
{
    public class TboxService
    {
        public const string baseUrl = "https://pan.sjtu.edu.cn";

        private readonly ILogger<TboxService> _logger;
        private readonly HttpClient _client;
        private readonly HttpClientFactory _clientFactory;
        private readonly TboxUserTokenProvider _userTokenProvider;
        private readonly TboxSpaceCredProvider _credProvider;

        public string UserToken => _userTokenProvider.GetUserToken();

        public TboxService(ILogger<TboxService> logger, TboxUserTokenProvider userTokenProvider, TboxSpaceCredProvider credProvider, HttpClientFactory clientFactory)
        {
            _logger = logger;
            _userTokenProvider = userTokenProvider;
            _credProvider = credProvider;
            _clientFactory = clientFactory;
            _client = _clientFactory.CreateClient();
        }

        public static CommonResult<TboxLoginResDto> LoginUseJaccount(HttpClient client)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://pan.sjtu.edu.cn/user/v1/sign-in/sso-login-redirect/xpw8ou8y?auto_redirect=true&from=web&custom_state=4ycSqbzfqM9mPuzOKmvTUQ%25253D%25253D");

            string code = "";
            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new(false, $"服务器响应{res.StatusCode}");
                }

                if (res.RequestMessage.RequestUri.Host.Contains("jaccount"))
                {
                    return new(false, $"未成功认证");
                }

                var reg = new Regex("code=(.+?)&state=");
                var match = reg.Match(res.RequestMessage.RequestUri.OriginalString);

                if (!match.Success)
                {
                    return new(false, $"未找到回调code");
                }
                code = match.Groups[1].Value;
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }

            req = new HttpRequestMessage(HttpMethod.Post, $"https://pan.sjtu.edu.cn/user/v1/sign-in/verify-account-login/xpw8ou8y?device_id=Chrome+116.0.0.0&type=sso&credential={code}");

            try
            {
                var res = client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new(false, $"服务器响应{res.StatusCode}");
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxLoginResDto>(body);

                if (json.Status != 0)
                {
                    return new(false, $"服务器返回失败：{json.Message}");
                }

                if (json.UserToken.Length != 128)
                {
                    return new(false, $"UserToken无效");
                }
                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxSpaceCred> GetSpace()
        {
            try
            {
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/user/v1/space/1/personal?user_token={UserToken}");
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

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

        public CommonResult<TboxSpaceQuotaInfo> GetSpaceQuotaInfo()
        {
            try
            {
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/user/v1/space/1?user_token={UserToken}");
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxSpaceQuotaInfo>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxMergedItemDto> GetItemInfo(string path)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("info", null);
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/directory/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxMergedItemDto>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxFileInfoDto> GetFileInfo(string path, string historyId = null)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("info", null);
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/directory/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxFileInfoDto>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxFileInfoDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxFileInfoDto>(body);

                return new CommonResult<TboxFileInfoDto>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxFileInfoDto>(false, ex.Message);
            }
        }

        public CommonResult<TboxFolderInfoDto> GetFolderInfo(string path)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("info", null);
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/directory/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxFolderInfoDto>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxItemListDto> ListItems(string path, int page, int count)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("page", $"{page}");
                query.Add("page_size", $"{count}");
                //query.Add("order_by", $"name");
                query.Add("order_by_type", $"desc");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/directory/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxItemListDto>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxStartChunkUploadResDto> StartChunkUpload(string path, int chunkCount)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("multipart", null);
                query.Add("conflict_resolution_strategy", "rename");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                StringBuilder sb = new StringBuilder();
                sb.Append("1");
                for (int i = 2; i <= Math.Min(chunkCount, 50); i++)
                    sb.Append($",{i}");
                req.Content = new StringContent($"{{\"partNumberRange\":[{sb.ToString()}]}}");

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

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

        public CommonResult<TboxStartChunkUploadResDto> RenewChunkUpload(string confirmKey, List<int> partNumberRange)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("renew", null);
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{confirmKey}" + UriHelper.BuildQuery(query));

                if (partNumberRange == null || partNumberRange.Count == 0)
                    return new CommonResult<TboxStartChunkUploadResDto>(false, "partNumberRange 为空");

                StringBuilder sb = new StringBuilder();
                sb.Append(partNumberRange.First());
                for (int i = 1; i < Math.Min(partNumberRange.Count, 50); i++)
                    sb.Append($",{partNumberRange[i]}");
                req.Content = new StringContent($"{{\"partNumberRange\":[{sb.ToString()}]}}");

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

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

        public CommonResult<TboxConfirmUploadResDto> ConfirmUpload(string confirmKey, ulong? crc64 = null)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("confirm", null);
                query.Add("conflict_resolution_strategy", "overwrite");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{confirmKey}" + UriHelper.BuildQuery(query));
                if (crc64.HasValue)
                {
                    req.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    req.Content = new StringContent($$"""
                {"crc64":"{{crc64}}"}
                """);
                }

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<TboxConfirmUploadResDto>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<TboxConfirmUploadResDto>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxConfirmUploadResDto>(body);

                if (json.Status != 0)
                {
                    return new CommonResult<TboxConfirmUploadResDto>(false, $"服务器返回失败：{json.Message}");
                }

                return new CommonResult<TboxConfirmUploadResDto>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<TboxConfirmUploadResDto>(false, ex.Message);
            }
        }

        public CommonResult<TboxChunkUploadInfoResDto> GetChunkUploadInfo(string confirmKey)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("upload", null);
                query.Add("no_upload_part_info", "1");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{confirmKey}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

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
            catch (Exception ex)
            {
                return new CommonResult<TboxChunkUploadInfoResDto>(false, ex.Message);
            }
        }

        public CommonResult<TboxSimpleUploadInfoDto> StartSimpleUpload(string path)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("conflict_resolution_strategy", "overwrite");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                req.Content = JsonContent.Create(new object());

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxSimpleUploadInfoDto>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<HttpStatusCode> UploadPart(TboxSimpleUploadInfoDto info, Stream stream)
        {
            try
            {
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"https://{info.Domain}{info.Path}");

                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(info.Headers["content-type"]);
                req.Content = content;
                foreach (var header in info.Headers)
                {
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new(false, $"服务器响应{res.StatusCode}");
                }

                return new(true, "", res.StatusCode);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<HttpStatusCode> UploadChunk(TboxStartChunkUploadResDto info, Stream stream, int partNumber, Pack<long> chunkProgress)
        {
            try
            {
                Dictionary<string, string> query = new Dictionary<string, string>();
                query.Add("uploadId", info.UploadId);
                query.Add("partNumber", partNumber.ToString());

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"https://{info.Domain}{info.Path}{UriHelper.BuildQuery(query)}");

                var content = new ProgressableStreamContent(stream, (x) => { chunkProgress.Value = x; });
                req.Content = content;
                foreach (var header in info.Parts[partNumber.ToString()].Headers)
                {
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var res = _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    return new(false, $"服务器响应{res.StatusCode}");
                }

                return new(true, "", res.StatusCode);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxErrorMessageDto> CreateDirectory(string dirpath)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("conflict_resolution_strategy", "ask");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, baseUrl + $"/api/v1/directory/{cred.LibraryId}/{cred.SpaceId}/{dirpath}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

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

        public CommonResult<List<TboxUserInfoDto>> GetUserInfo()
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/user/v1/organization?user_token={UserToken}");

            try
            {
                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new CommonResult<List<TboxUserInfoDto>>(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new CommonResult<List<TboxUserInfoDto>>(false, $"服务器响应{res.StatusCode}");
                    }
                }

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<List<TboxUserInfoDto>>(body);

                //if (json.Status != 0)
                //{
                //    return new CommonResult<List<TboxUserInfoDto>>(false, $"服务器返回失败：{json.Message}");
                //}

                return new CommonResult<List<TboxUserInfoDto>>(true, "", json);
            }
            catch (Exception ex)
            {
                return new CommonResult<List<TboxUserInfoDto>>(false, ex.Message);
            }

        }

        public CommonResult<Stream> GetFileStream(string path, long? start, long? end)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                if (start.HasValue)
                {
                    req.Headers.Range = new RangeHeaderValue(start, end);
                }
                var res = _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }

                return new(true, "", res.Content.ReadAsStreamAsync().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }

        }

        public CommonResult<TboxDeleteFileDto> DeleteFile(string path)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("permanent", "0"); //Todo: 回收站设置
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Delete, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{path.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }
                if (res.StatusCode == HttpStatusCode.NoContent)
                    return new(true, "", null);

                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxDeleteFileDto>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        public CommonResult<TboxMoveFileDto> CopyOrMoveFile(string sourcePath, string destinationPath, bool isMove = false)
        {
            try
            {
                var cred = CheckLogined();
                var query = new Dictionary<string, string>();
                query.Add("conflict_resolution_strategy", "ask");
                query.Add("access_token", cred.AccessToken);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, baseUrl + $"/api/v1/file/{cred.LibraryId}/{cred.SpaceId}/{destinationPath.UrlEncodeByParts()}" + UriHelper.BuildQuery(query));

                req.Content = new StringContent($$"""
                    {"{{(isMove ? "from" : "copyFrom")}}":"{{sourcePath}}"}
                    """);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var res = _client.SendAsync(req).GetAwaiter().GetResult();

                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var errbody = res.Content.ReadAsStringAsync().Result;
                        var errjson = JsonConvert.DeserializeObject<TboxErrorMessageDto>(errbody);
                        return new(false, $"[{errjson.Code}]{errjson.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new(false, $"服务器响应{res.StatusCode}");
                    }
                }
                var body = res.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<TboxMoveFileDto>(body);

                return new(true, "", json);
            }
            catch (Exception ex)
            {
                return new(false, ex.Message);
            }
        }

        private TboxSpaceCred CheckLogined()
        {
            if (string.IsNullOrEmpty(UserToken))
                throw new Exception("未登录");
            var cred = _credProvider.GetSpaceCred(UserToken);
            if (cred == null)
            {
                var res = GetSpace();
                if (!res.Success)
                    throw new Exception(res.Message);
                else
                {
                    cred = res.Result;
                    _credProvider.SetSpaceCred(UserToken, cred);
                }
            }
            return cred;
        }
    }
}
