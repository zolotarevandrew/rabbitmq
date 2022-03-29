using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exchanges
{
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
                        x.AddConsumer<SubmitOrderConsumer>();
                        x.UsingRabbitMq((context,cfg) =>
                        {
                            cfg.Host("localhost", "/", h => {
                                h.Username("guest");
                                h.Password("guest");
                            });
                            
                            
                            cfg.Send<SubmitOrder>(x => { x.UseRoutingKeyFormatter(context => "routingKey"); });
                            cfg.Message<SubmitOrder>(x => x.SetEntityName("submit_order"));
                            cfg.Publish<SubmitOrder>(x =>
                            {
                                x.ExchangeType = "direct";
                            });
                            //fanout
                            cfg.ReceiveEndpoint("orders", e =>
                            {
                                e.Consumer<SubmitOrderConsumer>();
                                e.Bind("submit_order", x =>
                                {
                                    x.ExchangeType = "direct";
                                    x.RoutingKey = "routingKey";
                                });
                            });
                        });
                    });
                    services.AddMassTransitHostedService();
                    services.AddHostedService<Worker>();
                });
    }
}