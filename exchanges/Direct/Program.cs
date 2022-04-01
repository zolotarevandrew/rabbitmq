using Direct;
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
                    x.AddConsumer<ClientCreatedConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });
                            
                        cfg.Send<ClientCreated>(x =>
                        {
                            x.UseRoutingKeyFormatter( c => c.Message.Key);
                        });
                        cfg.Message<ClientCreated>(x => x.SetEntityName("clientcreated"));
                        cfg.Publish<ClientCreated>(x => x.ExchangeType = ExchangeType.Direct);
                            
           
                        cfg.ReceiveEndpoint("new-clients", x =>
                        {
                            x.ConfigureConsumeTopology = false;

                            x.Consumer<ClientCreatedConsumer>();

                            x.Bind("clientcreated", s => 
                            {
                                s.RoutingKey = "NEW";
                                s.ExchangeType = ExchangeType.Direct;
                            });
                        });
                        
                        cfg.ReceiveEndpoint("old-clients", x =>
                        {
                            x.ConfigureConsumeTopology = false;

                            x.Consumer<ClientCreatedConsumer>();

                            x.Bind("clientcreated", s => 
                            {
                                s.RoutingKey = "OLD";
                                s.ExchangeType = ExchangeType.Direct;
                            });
                        });
                    });
                });
                services.AddMassTransitHostedService();
                services.AddHostedService<Worker>();
            });
}