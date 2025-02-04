using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using JboxTransfer.Server.Modules.DataWrapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using UserInfoDto = JboxTransfer.Server.Models.Output.UserInfoDto;
using JboxTransfer.Server.Services;
using JboxTransfer.Server.Models.User;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Modules.Db;

namespace JboxTransfer.Server.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMemoryCache _mcache;
        private readonly DefaultDbContext _context;
        public UserController(ILogger<UserController> logger, IMemoryCache memoryCache, DefaultDbContext context)
        {
            _logger = logger;
            _mcache = memoryCache;
            _context = context;
        }

        [Route("info")]
        [HttpGet]
        [Authorize]
        public ApiResponse Info()
        {
            var jaccount = User.FindFirstValue("jaccount");
            if (jaccount == null)
                return new ApiResponse(400, "UserIdMissingError", "找不到用户");
            var sysuser = _context.Users.FirstOrDefault(x => x.Jaccount == jaccount);
            if (sysuser == null)
                return new ApiResponse(400, "UserNotFoundError", "找不到用户");

            return new ApiResponse(new UserInfoDto()
            {
                Name = sysuser.Name,
                Role = sysuser.Role,
                Jaccount = sysuser.Jaccount,
            });
        }

        [Route("logout")]
        [HttpPost]
        public ApiResponse Logout()
        {
            HttpContext.SignOutAsync();
            return new ApiResponse(true) { Message = "退出登录成功" };
        }

        //[Route("jaccount/getqrcode")]
        //[HttpGet]
        //[Authorize]
        //public ApiResponse GetQrCode()
        //{
        //    var user = this.User;

        //    JaccountFastLoginService loginService = new JaccountFastLoginService();
        //    Task.Run(async () =>
        //    {
        //        var res1 = await loginService.GetUuid();
        //        if (!res1.Success)
        //            return;
        //        var res2 = await loginService.InitWebSocket();
        //        if (!res2.success)
        //            return;
        //        var res3 = await loginService.SendGetPayload();
        //        if (!res3.success)
        //            return;
        //    });

        //    loginService.WaitForQrCode();
        //    if (loginService.Failed || !loginService.Prepared)
        //    {
        //        return new ApiResponse(StatusCodes.Status500InternalServerError, "GetQrcodeError", "无法获取登录二维码");
        //    }

        //    _mcache.Set(loginService.Uuid, loginService, new MemoryCacheEntryOptions()
        //        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3))
        //        .SetPriority(CacheItemPriority.Normal)
        //        .RegisterPostEvictionCallback((key, value, reason, substate) =>
        //        {
        //            JaccountFastLoginService loginService1 = value as JaccountFastLoginService;
        //            loginService1.Dispose();
        //        }));

        //    return new ApiResponse(new JaccountQrCodeDataDto()
        //    {
        //        Qrcode = loginService.GetQrcodeStr(),
        //        Uuid = loginService.Uuid
        //    });
        //}

        //[Route("jaccount/validate")]
        //[HttpGet]
        //[Authorize]
        //public ApiResponse Validate([FromQuery]string uuid)
        //{
        //    var user = this.User;

        //    var flag = _mcache.TryGetValue<JaccountFastLoginService>(uuid, out var loginService);
        //    if (!flag)
        //    {
        //        return new ApiResponse(StatusCodes.Status408RequestTimeout, "RequestTimeoutError", "请求超时，请在3分钟内完成操作。");
        //    }

        //    loginService.WaitForLogined();
        //    if (loginService.Failed || !loginService.Logined)
        //    {
        //        return new ApiResponse(StatusCodes.Status500InternalServerError, "LoginFailError", $"登录失败：{loginService.Message}");
        //    }

        //    var userinfores = loginService.GetUserInfo();
        //    if (!userinfores.Success)
        //    {
        //        return new ApiResponse(StatusCodes.Status500InternalServerError, "LoginFailError", $"获取用户信息失败：{userinfores.Message}");
        //    }

        //    var sysuser = UpdateUserCookie(user, userinfores.Result, loginService.GetCookie());
        //    if (sysuser == null)
        //    {
        //        return new ApiResponse(400, "ValidateError", "找不到用户");
        //    }
        //    UserSignin(sysuser);

        //    return new ApiResponse(true);
        //}

        //[Route("jaccount/redirect")]
        //[HttpGet]
        //[Authorize]
        //public ApiResponse Redirect([FromQuery] string? redirect_uri)
        //{
        //    return new ApiResponse($"https://jaccount.sjtu.edu.cn/oauth2/authorize?response_type=code&scope=openid+basic&client_id={GlobalConfigService.Config.OAuthConfig.ClientId}&redirect_uri={redirect_uri ?? HttpUtility.UrlEncode($"{Request.Scheme}://{Request.Host}" + "/oauth/redirectback")}");
        //}

        //[Route("jaccount/redirectback")]
        //[HttpGet]
        //[Authorize]
        //public ApiResponse RedirectBack([FromQuery]string code, [FromQuery] string? redirect_uri)
        //{
        //    var jaccount = User.FindFirstValue("jaccount");
        //    if (jaccount == null)
        //        return new ApiResponse(400, "UserIdMissingError", "找不到用户");
        //    var sysuser = _context.Users.FirstOrDefault(x => x.Jaccount == jaccount);
        //    if (sysuser == null)
        //        return new ApiResponse(400, "UserNotFoundError", "找不到用户");

        //    HttpClient client = new HttpClient();
        //    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://jaccount.sjtu.edu.cn/oauth2/token");
        //    Dictionary<string, string> form = new Dictionary<string, string>();
        //    form.Add("code", code);
        //    form.Add("grant_type", "authorization_code");
        //    form.Add("redirect_uri", redirect_uri ?? $"{Request.Scheme}://{Request.Host}" + "/oauth/redirectback");
        //    form.Add("client_id", GlobalConfigService.Config.OAuthConfig.ClientId);
        //    form.Add("client_secret", GlobalConfigService.Config.OAuthConfig.ClientSecret);
        //    req.Content = new FormUrlEncodedContent(form);
        //    var res = client.Send(req);
        //    if (!res.IsSuccessStatusCode)
        //    {
        //        return new ApiResponse(StatusCodes.Status400BadRequest, "GetJaccountAppTokenError", "验证登录失败");
        //    }
        //    if (res.Content.Headers.ContentType.MediaType != "application/json")
        //    {
        //        return new ApiResponse(StatusCodes.Status400BadRequest, "GetJaccountAppTokenError", "验证登录失败");
        //    }
        //    var rawjson = res.Content.ReadAsStringAsync().Result;
        //    var json = JsonConvert.DeserializeObject<JacAppTokenResDto>(rawjson);

        //    IJsonSerializer serializer = new JsonNetSerializer();
        //    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        //    JwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);
        //    var id_token = decoder.Decode(json.IdToken, false);
        //    var id_token_json = JsonConvert.DeserializeObject<JacOAuthIdToken>(id_token);
        //    jaccount = id_token_json.Sub;

        //    sysuser.Jaccount = jaccount;
        //    _context.Update(sysuser);
        //    _context.SaveChanges();

        //    return new ApiResponse(jaccount) { Message = "绑定成功" };
        //}

        [Route("jaccount/getqrcode")]
        [HttpGet]
        public ApiResponse LoginByJac()
        {
            JaccountFastLoginService loginService = new JaccountFastLoginService();
            Task.Run(async () =>
            {
                var res1 = await loginService.GetUuid();
                if (!res1.Success)
                    return;
                var res2 = await loginService.InitWebSocket();
                if (!res2.success)
                    return;
                var res3 = await loginService.SendGetPayload();
                if (!res3.success)
                    return;
            });

            loginService.WaitForQrCode();
            if (loginService.Failed || !loginService.Prepared)
            {
                return new ApiResponse(StatusCodes.Status500InternalServerError, "GetQrcodeError", "无法获取登录二维码");
            }

            _mcache.Set(loginService.Uuid, loginService, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3))
                .SetPriority(CacheItemPriority.Normal)
                .RegisterPostEvictionCallback((key, value, reason, substate) =>
                {
                    JaccountFastLoginService loginService1 = value as JaccountFastLoginService;
                    loginService1.Dispose();
                }));

            return new ApiResponse(new JaccountQrCodeDataDto()
            {
                Qrcode = loginService.GetQrcodeStr(),
                Uuid = loginService.Uuid
            });
        }

        [Route("jaccount/validate")]
        [HttpGet]
        public ApiResponse LoginByJacValidate([FromQuery] string uuid)
        {
            var flag = _mcache.TryGetValue<JaccountFastLoginService>(uuid, out var loginService);
            if (!flag)
            {
                return new ApiResponse(StatusCodes.Status408RequestTimeout, "RequestTimeoutError", "请求超时，请在3分钟内完成操作。");
            }

            loginService.WaitForLogined();
            if (loginService.Failed || !loginService.Logined)
            {
                return new ApiResponse(StatusCodes.Status500InternalServerError, "LoginFailError", $"登录失败：{loginService.Message}");
            }

            var userinfores = loginService.GetUserInfo();
            if (!userinfores.Success)
            {
                return new ApiResponse(StatusCodes.Status500InternalServerError, "LoginFailError", $"获取用户信息失败：{userinfores.Message}");
            }

            var sysuser = _context.Users.FirstOrDefault(x => x.Jaccount == userinfores.Result.AccountNo);

            if (sysuser == null)
            {
                sysuser = new SystemUser() { Name = userinfores.Result.Name, Jaccount = userinfores.Result.AccountNo, Cookie = loginService.GetCookie(), RegistrationTime = DateTime.Now };
                _context.Add(sysuser);
                _context.SaveChanges();
            }
            else
            {
                sysuser.Cookie = loginService.GetCookie();
                sysuser.Jaccount = userinfores.Result.AccountNo;
                _context.Update(sysuser);
                _context.SaveChanges();
            }
            
            UserSignin(sysuser);
            return new ApiResponse(true);
        }

        //[Route("list")]
        //[HttpGet]
        //public List<SystemUser> GetUsers()
        //{
        //    var users = _context.Users.ToList();
        //    return users;
        //}

        [NonAction]
        private void UserSignin(SystemUser user)
        {
            var claims = new List<Claim>
            {
                new Claim("userid", user.Id.ToString()),
                new Claim("jaccount", user.Jaccount),
            };
            //if (user.Username == "admin")
            //    claims.Add(new Claim(ClaimTypes.Role, "admin"));
            //else
            //    claims.Add(new Claim(ClaimTypes.Role, "user"));

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
            };

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties).GetAwaiter().GetResult();
        }

        [NonAction]
        private SystemUser UpdateUserCookie(ClaimsPrincipal user, UserInfoEntity userinfo, string cookie)
        {
            var sysuser = _context.Users.FirstOrDefault(x => x.Jaccount == user.FindFirstValue("jaccount"));
            if (sysuser == null)
            {
                return null;
            }
            sysuser.Cookie = cookie;
            sysuser.Jaccount = userinfo.AccountNo;
            _context.Update(sysuser);
            _context.SaveChanges();
            return sysuser;
        }
    }
}
