using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Models;
using Newtonsoft.Json;
using Services;
using System;

namespace FilesAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Kestrel server options for unlimited file uploads
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = null; // Remove limit completely
                options.Limits.MinRequestBodyDataRate = null; // Remove timeout for slow uploads
                options.Limits.MinResponseDataRate = null; // Remove timeout for slow downloads
                options.Limits.MaxConcurrentConnections = null; // Remove connection limit
                options.Limits.MaxConcurrentUpgradedConnections = null; // Remove upgraded connection limit
            });

            // Note: IIS file size limits are configured via web.config See web.config for
            // maxAllowedContentLength and requestLimits configuration Configure form options for
            // unlimited file uploads
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue; // Remove form value length limit
                options.MultipartBodyLengthLimit = long.MaxValue; // Remove multipart body limit
                options.MultipartHeadersLengthLimit = int.MaxValue; // Remove header length limit
                options.MultipartBoundaryLengthLimit = int.MaxValue; // Remove boundary length limit
                options.KeyLengthLimit = int.MaxValue; // Remove key length limit
                options.ValueCountLimit = int.MaxValue; // Remove value count limit
                options.BufferBody = true; // Buffer the request body
                options.MemoryBufferThreshold = int.MaxValue; // Set memory buffer threshold
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Files API",
                    Version = "v1"
                });
            });
            //App Settings Injection
            services.Configure<MongoDBAppSettings>(Configuration.GetSection("MongoDBAppSettings"));
            services.Configure<LiteDBAppSettings>(Configuration.GetSection("LiteDBAppSettings"));

            // Configure database services based on environment
            var useEmbeddedDatabase = Configuration.GetValue<bool>("USE_EMBEDDED_DATABASE", false) ||
                                     Environment.GetEnvironmentVariable("USE_EMBEDDED_DATABASE") == "true";

            var useSqlDatabase = Configuration.GetValue<bool>("USE_SQL_DATABASE", false) ||
                         Environment.GetEnvironmentVariable("USE_SQL_DATABASE") == "true";

            if (useEmbeddedDatabase)
            {
                // Use LiteDB for self-contained operation
                var databasePath = Environment.GetEnvironmentVariable("DATABASE_PATH") ??
                                  Configuration.GetValue<string>("DATABASE_PATH", "./data/filesapi.db");
                var uploadsPath = Environment.GetEnvironmentVariable("UPLOADS_PATH") ??
                                 Configuration.GetValue<string>("UPLOADS_PATH", "./uploads");

                services.AddScoped<IStorageRepository>(provider =>
                    new LiteDbStorageRepository(databasePath, uploadsPath));
                services.AddScoped<IFileDetailsRepository>(provider =>
                    new LiteDbFileDetailsRepository(databasePath));
                services.AddScoped<IDownloadAnalyticsRepository>(provider =>
                    new LiteDbDownloadAnalyticsRepository(databasePath));
            }
            else if (useSqlDatabase)
            {
                // Use SQL database for operation
                services.AddDbContext<FilesDbContext>(options => options.UseSqlite("Data Source=database.db"));
                services.AddScoped<IStorageRepository, SqlStorageRepository>();
                services.AddScoped<IFileDetailsRepository, SqlFileDetailsRepository>();
                services.AddScoped<IDownloadAnalyticsRepository, SqlDbDownloadAnalyticsRepository>();
            }
            else
            {
                // Use MongoDB for traditional operation
                services.AddScoped<IStorageRepository, MongoStorageRepository>();
                services.AddScoped<IFileDetailsRepository, MongoFileDetailsRepository>();
                services.AddScoped<IDownloadAnalyticsRepository, MongoDbDownloadAnalyticsRepository>();
            }

            services.AddScoped<IStorageService, StorageService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddScoped<RecordDownloadHandler>();
            services.AddScoped<RecordDownloadAnalyticsHandler>();
            services.AddScoped<EventHandlerContainer>();
            EventHandlerContainer.Subscribe<FileDownloadedEvent, RecordDownloadHandler>();
            EventHandlerContainer.Subscribe<EnhancedFileDownloadedEvent, RecordDownloadAnalyticsHandler>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            var swaggerUrl = "/swagger/v1/swagger.json";
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(swaggerUrl, "Files API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;
                if (exception is FilesApiException)
                {
                    var result = JsonConvert.SerializeObject(new { error = exception.Message });
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync(result);
                }
            }));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}