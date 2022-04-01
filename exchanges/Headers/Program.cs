using Direct;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Headers;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMassTransit(x =>
                {
                    x.AddConsumer<LogAppendedConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });
      
                        cfg.Message<LogAppended>(x => x.SetEntityName("logAppended"));
                        cfg.Publish<LogAppended>(x => x.ExchangeType = ExchangeType.Headers);
                        
                        
                        cfg.ReceiveEndpoint("logs-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;

                            x.Consumer<LogAppendedConsumer>();

                            x.Bind("logAppended", s =>
                            {
                                s.SetBindingArgument("x-match", "all");
                                s.SetBindingArgument("Test-header", "value1");
                                s.SetBindingArgument("Test-header2", "value2");
                                s.ExchangeType = ExchangeType.Headers;
                            });
                        });
                    });
                });
                services.AddMassTransitHostedService();
                services.AddHostedService<Worker>();
            });
}