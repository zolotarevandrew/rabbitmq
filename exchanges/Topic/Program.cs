using Direct;
using MassTransit;
using MassTransit.RabbitMqTransport.Topology.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Topic;

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
                    x.AddConsumer<NotificationCreatedConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });
                            
                        cfg.Send<NotificationCreated>(x =>
                        {
                            x.UseRoutingKeyFormatter( c => c.Message.From);
                        });
                        cfg.Message<NotificationCreated>(x => x.SetEntityName("notificationcreated"));
                        cfg.Publish<NotificationCreated>(x =>
                        {
                            x.ExchangeType = ExchangeType.Topic;
                        });
                        
                        cfg.ReceiveEndpoint("sales-notifications-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;

                            x.Consumer<NotificationCreatedConsumer>();

                            x.Bind("notificationcreated", s => 
                            {
                                s.RoutingKey = "sales.*";
                                s.ExchangeType = ExchangeType.Topic;
                            });
                        });
                    });
                });
                services.AddMassTransitHostedService();
                services.AddHostedService<Worker>();
            });
}