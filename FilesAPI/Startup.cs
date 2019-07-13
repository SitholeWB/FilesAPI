using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Settings;
using Services;
using Swashbuckle.AspNetCore.Swagger;

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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					builder =>
					{
						builder
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials();
					});
			});
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info
				{
					Title = "Files API",
					Version = "v1"
				});
			});
			//App Settings Injection
			services.Configure<MongoDBAppSettings>(Configuration.GetSection("MongoDBAppSettings"));

			services.AddTransient<IStorageService, StorageService>();
			services.AddSingleton<ISettingsService, SettingsService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
			app.UseMvc();

		}
	}
}
