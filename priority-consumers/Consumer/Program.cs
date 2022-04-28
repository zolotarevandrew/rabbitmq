using Consumer;
using MassTransit;
using Messages;
using IHost = Microsoft.Extensions.Hosting.IHost;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((host, services) =>
    {
        services.AddMassTransit(x =>
                {
                    x.AddConsumer<TestMessageConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint("queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.PrefetchCount = 3;
                            x.ConsumerPriority = host.Configuration.GetSection("Priority").Get<int>();
                            x.Consumer<TestMessageConsumer>();
                            x.Bind<TestMessage>();
                        });
                    });
                });
                services.AddMassTransitHostedService();
    })
    .Build();

await host.RunAsync();