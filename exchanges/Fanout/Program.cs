using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    x.AddConsumer<SubmitOrderConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });
                            
                        cfg.Message<SubmitOrder>(x => x.SetEntityName("submitorder"));
                            
                        //Exchanges:SubmitOrder(fanout) -> orders(fanout) -> orders(queue)
                        cfg.ReceiveEndpoint("orders", e =>
                        {
                            e.ConfigureConsumeTopology = false;
                            e.Consumer<SubmitOrderConsumer>();
                            e.Bind<SubmitOrder>();
                        });
                    });
                });
                services.AddMassTransitHostedService();
                services.AddHostedService<Worker>();
            });
}