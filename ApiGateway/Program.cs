using System.Net;
using System.Net.WebSockets;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway;

public class Program
{
    
    public static void Main(string[] args)
    {
        string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                var configX = config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
                if (hostingContext.HostingEnvironment.IsProduction())
                {
                    configX.AddJsonFile("ocelot.json");
                }
                else
                {
                    configX.AddJsonFile("ocelot.dev.json");
                }

                configX
                    .AddEnvironmentVariables();
            })
            .ConfigureServices(s =>
            {
                s.AddCors();
                s.AddSignalR();
                s.AddOcelot();
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConsole();
            })
            .UseIISIntegration()
            .Configure((context, app) =>
            {
                var origins = context.Configuration.GetSection("CorsOrigins").Get<List<string>>();
                if (origins != null && origins.Count != 0)
                {
                    app.UseCors(x =>
                        x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(origins.ToArray()));
                }

                app.UseWebSockets();
                app.UseOcelot().Wait();
            });

        if (environmentName != "Development")
        {
            host.UseUrls("http://*:80/");
        }
        
        host.Build().Run();
    }
}