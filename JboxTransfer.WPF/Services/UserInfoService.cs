using JboxTransfer.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Models;
using ZXing.Aztec.Internal;

namespace JboxTransfer.Services
{
    public class UserInfoService : IUserInfoService
    {
        public static UserInfoEntity entity;
        public static CommonResult GetUserInfo()
        {
            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://my.sjtu.edu.cn/api/resource/my/info");
            req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
            var res = client.SendAsync(req).GetAwaiter().GetResult();

            while (res.StatusCode == HttpStatusCode.Found && res.Headers.Location.Scheme == "http")
            {
                req = new HttpRequestMessage(HttpMethod.Get, res.Headers.Location.OriginalString.Replace("http", "https"));
                req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                req.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                req.Headers.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.76");
                res = client.SendAsync(req).GetAwaiter().GetResult();
            }

            if (!res.IsSuccessStatusCode)
            {
                return new CommonResult(false, $"服务器响应{res.StatusCode}");
            }

            var body = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<UserInfoDto>(body);

            if (json.Errno != 0)
            {
                return new CommonResult(false, $"服务器返回错误：{json.Error}");
            }

            entity = json.Entities;
            return new CommonResult(true, "");
        }
    }

    public partial class UserInfoDto
    {
        [JsonProperty("entities")]
        public UserInfoEntity Entities { get; set; }

        [JsonProperty("errno")]
        public long Errno { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public partial class UserInfoEntity
    {
        [JsonProperty("accountNo")]
        public string AccountNo { get; set; }

        [JsonProperty("authAccounts")]
        public System.Collections.Generic.List<string> AuthAccounts { get; set; }

        [JsonProperty("avatars")]
        public object Avatars { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("expireDate")]
        public string ExpireDate { get; set; }

        [JsonProperty("identities")]
        public System.Collections.Generic.List<Identity> Identities { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("organizeId")]
        public string OrganizeId { get; set; }

        [JsonProperty("organizeName")]
        public string OrganizeName { get; set; }

        [JsonProperty("responseName")]
        public object ResponseName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusEN")]
        public string StatusEn { get; set; }

        [JsonProperty("userStyleName")]
        public string UserStyleName { get; set; }

        [JsonProperty("userType")]
        public string UserType { get; set; }
    }

    public partial class Identity
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("createDate")]
        public long CreateDate { get; set; }

        [JsonProperty("defaultOptional")]
        public bool DefaultOptional { get; set; }

        [JsonProperty("expireDate")]
        public string ExpireDate { get; set; }

        [JsonProperty("facultyType")]
        public object FacultyType { get; set; }

        [JsonProperty("gjm")]
        public string Gjm { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("organize")]
        public Organize Organize { get; set; }

        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("topOrganize")]
        public TopOrganize TopOrganize { get; set; }

        [JsonProperty("type")]
        public TypeClass Type { get; set; }

        [JsonProperty("updateDate")]
        public long UpdateDate { get; set; }

        [JsonProperty("userStyleName")]
        public string UserStyleName { get; set; }

        [JsonProperty("userType")]
        public string UserType { get; set; }
    }

    public partial class Organize
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class TopOrganize
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class TypeClass
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
