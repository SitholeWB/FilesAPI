using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Models.Events;
using Models.Exceptions;
using Models.Settings;
using Newtonsoft.Json;
using Services;
using Services.Events;
using Services.Events.Handlers;

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
			services.Configure<KestrelServerOptions>(options =>
			{
				options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
			});
			services.Configure<IISServerOptions>(options =>
			{
				options.MaxRequestBodySize = int.MaxValue;
			});
			services.Configure<FormOptions>(options =>
			{
				options.ValueLengthLimit = int.MaxValue;
				options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
				options.MultipartHeadersLengthLimit = int.MaxValue;
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

			//services.AddSingleton<IStorageService, FilesService>();
			services.AddTransient<IStorageService, StorageService>();
			services.AddSingleton<ISettingsService, SettingsService>();

			services.AddScoped<RecordDownloadHandler>();
			services.AddScoped<EventHandlerContainer>();
			EventHandlerContainer.Subscribe<FileDownloadedEvent, RecordDownloadHandler>();

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
			else
			{
				swaggerUrl = "/FilesAPI/swagger/v1/swagger.json";//For IIS hosting
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