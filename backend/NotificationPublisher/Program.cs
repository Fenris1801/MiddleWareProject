using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var brokerHost = Environment.GetEnvironmentVariable("ACTIVEMQ_BROKER") ?? "tcp://activemq:61616";

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(new Func<string>(() => brokerHost));
                services.AddHostedService(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<NotificationService>>();
                    return new NotificationService(logger, brokerHost);
                });
            })
            .Build();

        await host.RunAsync();
    }
}
