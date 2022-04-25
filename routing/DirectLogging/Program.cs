using DirectLogging;
using MassTransit;
using MassTransit.RabbitMqTransport.Topology.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Fanout;

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
                    x.AddConsumer<LogCreatedConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.Send<LogCreated>(x =>
                        {
                            x.UseRoutingKeyFormatter(r => r.Message.Type.ToString());
                        });
                        cfg.Publish<LogCreated>(x => x.ExchangeType = ExchangeType.Direct);
                        
                        cfg.ReceiveEndpoint("info-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.Consumer<LogCreatedConsumer>();
                            x.Bind<LogCreated>(s => 
                            {
                                s.RoutingKey = LogType.Info.ToString();
                                s.ExchangeType = ExchangeType.Direct;
                            });
                        });
                        cfg.ReceiveEndpoint("warn-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.Consumer<LogCreatedConsumer>();
                            x.Bind<LogCreated>(s => 
                            {
                                s.RoutingKey = LogType.Warn.ToString();
                                s.ExchangeType = ExchangeType.Direct;
                            });
                        });
                        cfg.ReceiveEndpoint("error-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.Consumer<LogCreatedConsumer>();
                            x.Bind<LogCreated>(s => 
                            {
                                s.RoutingKey = LogType.Error.ToString();
                                s.ExchangeType = ExchangeType.Direct;
                            });
                        });
                    });
                });
                services.AddMassTransitHostedService();
                services.AddHostedService<Worker>();
            });
}