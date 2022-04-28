using Consumer;
using MassTransit;
using Messages;
using IHost = Microsoft.Extensions.Hosting.IHost;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
                {
                    x.AddConsumer<PriorityMessageConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint("priority-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.PrefetchCount = 3;
                            x.EnablePriority(10);
                            x.Consumer<PriorityMessageConsumer>();
                            x.Bind<PriorityMessage>();
                        });
                    });
                });
                services.AddMassTransitHostedService();
    })
    .Build();

await host.RunAsync();