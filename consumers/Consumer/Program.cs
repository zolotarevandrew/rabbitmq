using Consumer;
using MassTransit;
using Messages;
using IHost = Microsoft.Extensions.Hosting.IHost;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
                {
                    x.AddConsumer<JobCreatedConsumer>();
                    x.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint("job-queue", x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.SingleActiveConsumer = true;
                            x.Durable = true;
                            x.PrefetchCount = 1;
                            x.Consumer<JobCreatedConsumer>();
                            x.Bind<JobCreated>();
                        });
                    });
                });
                services.AddMassTransitHostedService();
    })
    .Build();

await host.RunAsync();