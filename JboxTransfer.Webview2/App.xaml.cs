using AutoMapper.Internal;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Server;
using JboxTransfer.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.Windows;

namespace JboxTransfer.Webview2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public WebApplication? WebApplication { get; private set; }
        public SplashWindow? Splash { get; private set; }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Splash = new SplashWindow();
            Splash.Show();

            await Task.Run(StartAspApp);

            this.Exit += async (s, e) => await WebApplication.StopAsync();
        }

        private async void StartAspApp()
        {
            WebApplication? app = null;
            try
            {
                var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());

                builder.Services.AddDataWrapper();
                builder.Services.AddSwagger();
                builder.Services.AddCookieAuth();
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddMemoryCache();
                builder.Services.AddSqlite();
                builder.Services.AddMapper();
                builder.Services.AddMassTransit();
                builder.Services.AddAppServices();

                builder.Services.AddSingleton<MainWindow>();
                builder.Services.AddSingleton(this);

                builder.Services.AddControllers()
                    .ConfigureApplicationPartManager(manager =>
                    {
                        // 手动添加包含目标 Controller 的程序集
                        manager.ApplicationParts.Add(new AssemblyPart(typeof(JboxTransfer.Server.Program).Assembly));
                    });

                var server_url = $"http://{GlobalConfigService.Config.ServerConfig.Host}:{GlobalConfigService.Config.ServerConfig.Port}";
                builder.WebHost.UseUrls(server_url);

                app = builder.Build();

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

                app.UseRewriter(new RewriteOptions().AddRewrite("^[^.]*$", "index.html", true));
                app.UseStaticFiles();

                //迁移数据库
                using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                    context.Database.Migrate();
                }
                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Splash.Close();
                        this.MainWindow = WebApplication.Services.GetRequiredService<MainWindow>();
                        this.MainWindow.Show();
                    });
                });
                WebApplication = app;
                await app.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动程序时发生错误：{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(-1);
                return;
            }
        }
    }

}
