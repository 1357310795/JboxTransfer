
using JboxTransfer.Core.Models.Message;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.AutoMapper;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules.Sync;
using JboxTransfer.Core.Modules.Tbox;
using JboxTransfer.Server.Helpers;
using JboxTransfer.Server.Modules.DataWrapper;
using MassTransit;
using MassTransit.Contracts.JobService;
using MassTransit.Middleware;
using MassTransit.Testing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TboxWebdav.Server.Modules;
using TboxWebdav.Server.Modules.Tbox;

namespace JboxTransfer.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<DataWrapperFilter>();
                options.Filters.Add<ExceptionDataWrapperFilter>();
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(x =>
                            {
                                x.Events.OnRedirectToAccessDenied = (x) => {
                                    x.HttpContext.Response.ContentType = "application/json";
                                    x.HttpContext.Response.WriteAsJsonAsync<ApiResponse>(new ApiResponse(403, "ForbidError", "无权限，请使用 admin 账号"));
                                    return Task.CompletedTask;
                                };
                                x.Events.OnRedirectToLogin = (x) => {
                                    x.HttpContext.Response.ContentType = "application/json";
                                    x.HttpContext.Response.WriteAsJsonAsync<ApiResponse>(new ApiResponse(401, "NotLoginedError", "请先登录"));
                                    return Task.CompletedTask;
                                };
                                //x.Cookie.HttpOnly = true;
                                //x.Cookie.SameSite = SameSiteMode.None;
                                //x.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                            });
            builder.Services.AddAuthorization(options =>
            {
                //options.AddPolicy("CustomPolicy", policy =>
                //{
                //    policy.Requirements.Add(new CustomAuthorizationRequirement());
                //    policy.RequireAuthenticatedUser();
                //});
                //options.DefaultPolicy = options.GetPolicy("CustomPolicy");
            });

            // HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Cache
            builder.Services.AddMemoryCache();

            // DataWrapper
            builder.Services.AddSingleton<IDataWrapperOptions, DataWrapperOptions>();
            builder.Services.AddSingleton<IDataWrapperExecutor, DefaultWrapperExecutor>();

            // Sqlite
            Directory.CreateDirectory(PathHelper.AppDataPath);
            builder.Services.AddDbContext<DefaultDbContext>(options => {
                options.UseSqlite($"DataSource={Path.Combine(PathHelper.AppDataPath, "jboxtransfer.server.db")};");
                options.EnableSensitiveDataLogging();
            });

            // AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<SyncTaskMapperProfile>();
                cfg.AddProfile<UserMapperProfile>();
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    x => x.WithOrigins("http://localhost:5173")
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => origin == "http://localhost:5173")
                        .AllowCredentials()
                );
            });

            //MassTransit
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<NewTaskCheckConsumer>((context, cfg) =>
                {
                    var partition = new Partitioner(16, new Murmur3UnsafeHashGenerator());
                    cfg.Message<NewTaskCheckMessage>(s => {
                        s.UsePartitioner(partition, x => x.Message.UserId);
                        s.UseTimeout(timeout => timeout.Timeout = TimeSpan.FromSeconds(15));
                    });
                    cfg.UseInlineFilter((context, next) => { return next.Send(context).ContinueWith((x) => Task.Delay(1000)); });
                });

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("add_task_from_db", e =>
                    {
                        //e.UseConsumeFilter<GlobalSubscribeCheckFilter>(context);
                        e.ConfigureConsumer<NewTaskCheckConsumer>(context);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Application
            builder.Services.AddScoped<TboxSpaceCredProvider>();
            builder.Services.AddScoped<TboxUserTokenProvider>();
            builder.Services.AddScoped<TboxSpaceInfoProvider>();
            builder.Services.AddScoped<TboxService>();
            builder.Services.AddTransient<TboxUploadSession>();

            builder.Services.AddScoped<JboxCredProvider>();
            builder.Services.AddScoped<JboxService>();
            builder.Services.AddScoped<JboxQuotaInfoProvider>();
            builder.Services.AddTransient<JboxDownloadSession>();

            builder.Services.AddSingleton<CookieContainerProvider>();
            builder.Services.AddScoped<HttpClientFactory>();
            builder.Services.AddScoped<SystemUserInfoProvider>();

            builder.Services.AddTransient<FileSyncTask>();
            builder.Services.AddTransient<FolderSyncTask>();
            builder.Services.AddSingleton<SyncTaskCollectionProvider>();

            // Host
            //builder.WebHost.UseUrls("http://0.0.0.0:18888");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "/static",
                FileProvider = new PhysicalFileProvider(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Static"))
            });

            app.UseRewriter(new RewriteOptions().AddRewrite("^[^.]*$", "index.html", true));
            app.UseStaticFiles();

            app.UseCors();

            //迁移数据库
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                context.Database.Migrate();
            }

            app.Run();
        }
    }
}
