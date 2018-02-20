using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomaruBot.DAL;
using KomaruBot.WebAPI.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KomaruBot.WebAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<Models.AppSettings>(Configuration.GetSection("ApplicationSettings"));
            services.AddTransient<Authentication.IAuthenticationProvider, Authentication.TwitchAuthentication>();
            services.AddTransient<RedisContext>();
            services.AddTransient<UserHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddFile("Logs/KomaruBot-{Date}.txt", LogLevel.Warning);
            }
            else
            {
                loggerFactory.AddAWSProvider(this.Configuration.GetAWSLoggingConfigSection(), 
                    formatter: (logLevel, message, exception) => $"[{DateTime.UtcNow}] {logLevel}: {message}");
            }


            app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new List<string> { "index.html" },  });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseMvc();

            loggerFactory.CreateLogger("Startup").LogWarning("Application starting up.");
        }
    }
}
