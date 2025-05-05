using GreenPipes.Configurators;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitTransit;
using RabbitTransit.Direct;
using RabbitTransit.Topic;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<DirectConsumer>();
            x.AddConsumer<TopicConsumer>();
            x.AddConsumer<HeadersConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureDirectConsumer( context );
                cfg.ConfigureTopicConsumer( context );
                cfg.ConfigureHeadersConsumer( context );
            });
        });
        
        services.AddHostedService<BusHostedService>();
    })
    .Build();

await host.RunAsync();
