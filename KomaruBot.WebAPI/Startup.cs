using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomaruBot.Common;
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
            _loggerFactory = loggerFactory;
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



            var settings = app.ApplicationServices.GetService<IOptions<Models.AppSettings>>();
            chatBotManager = new ChatBot.ChatBotManager(loggerFactory.CreateLogger("ChatBot"), settings.Value.ChatBotTwitchUsername, settings.Value.ChatBotTwitchOauthToken);

            var logger = loggerFactory.CreateLogger("Startup");
            
            logger.LogWarning("Application starting up...");

            List<ChatBot.ClientConfiguration> configurations = null;

            try
            {
                var userHelper = app.ApplicationServices.GetService<UserHelper>();
                configurations = userHelper.GetAllUserClientConfigurations();

                logger.LogWarning($"Loaded {configurations.Count} chat bots. Starting them... Chatbot names: {string.Join(", ", configurations.Select(x => x.channelName).ToArray())}");

                foreach (var a in configurations)
                {
                    chatBotManager.RegisterConnection(a);
                }

            }
            catch (Exception exc)
            {
                logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name} - Starting chatbots"));
            }

            keepAlive = new KeepAliveHelper(loggerFactory.CreateLogger("KeepAlive"), settings.Value.ClientUrl + "/api/keepalive");

            logger.LogWarning("Application started up.");
        }

        public static KeepAliveHelper keepAlive;
        public static ChatBot.ChatBotManager chatBotManager;
        public static ILoggerFactory _loggerFactory;
    }
}
