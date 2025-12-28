using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FilesAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            try
            {
                var dbContext = host.Services.CreateScope().ServiceProvider.GetRequiredService<FilesDbContext>();
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error during database migration: {ex.Message}");
            }
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    // URLs are configured via environment variables (ASPNETCORE_URLS)
                });
    }
}