namespace ChatWithYourData.WebApp.Bootstrapper
{
    using ChatWithYourData.Application.Interfaces;
    using ChatWithYourData.Application.Services;
    using ChatWithYourData.Domain.Data;
    using ChatWithYourData.Infrastructure.Managers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.OpenApi.Models;
    using System.Reflection;

    /// <summary>
    /// App Builder 
    /// </summary>
    public static class AppBuilder
    {
        /// <summary>
        /// Get App
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static WebApplication GetApp(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ChatWithYourDataContext>(options => options.UseSqlServer(ConfigurationManager.GetValue("CHAT_WITH_YOUR_DATA_CONNECTION_STRING")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHealthChecks();

            MemoryCache memoryCache = new(new MemoryCacheOptions { SizeLimit = 10 });
            builder.Services.AddSingleton<IMemoryCache>(memoryCache);

            // Services
            builder.Services.AddScoped<IUserTokenService, UserTokenService>();
            builder.Services.AddScoped<INLToSQLQueryService, NLToSQLQueryService>();

            // Logs
            builder.Logging.ClearProviders();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Chat With Your Data Service API",
                    Description = "Chat with your data token utilization and cost monitoring challenge documentation API",
                    Contact = new OpenApiContact
                    {
                        Name = "Microsoft Learn",
                        Url = new Uri("https://learn.microsoft.com/")
                    }
                });

                string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            WebApplication app = builder.Build();
            return app;
        }
    }
}
