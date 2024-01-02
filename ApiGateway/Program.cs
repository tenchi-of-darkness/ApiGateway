using System.Net;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
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
                s.AddOcelot();
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConsole();
            })
            .UseIISIntegration()
            .Configure(app =>
            {
                app.UseCors(x =>
                    x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000", "https://travel-planner.melanievandegraaf.nl"));
                app.UseWebSockets();
                app.UseOcelot().Wait();
            })
            .Build()
            .Run();
    }
}