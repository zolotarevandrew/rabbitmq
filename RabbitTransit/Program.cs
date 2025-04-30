using GreenPipes.Configurators;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitTransit;
using RabbitTransit.Direct;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<DirectConsumer>( c =>
            {
                /*c.UseMessageRetry( r =>
                {
                    //r.SetRetryPolicy( new RetryPolicyFactory() );
                } );*/
                //what?
                c.UseConcurrentMessageLimit( 20 );
            });
            x.AddConsumer<FaultDirectConsumer>( c =>
            {
                /*c.UseMessageRetry( r =>
                {
                    //r.SetRetryPolicy( new RetryPolicyFactory() );
                } );*/
                //what?
                c.UseConcurrentMessageLimit( 20 );
            });
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("direct-queue", e =>
                {
                    e.PrefetchCount = 16;
                    
                    e.ConfigureConsumer<DirectConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("fault-directmessage-queue", e =>
                {
                    e.Bind("MassTransit:Fault--RabbitTransit.Direct:DirectMessage--", s =>
                    {
                        s.ExchangeType = "fanout";
                    });
                    
                    e.PrefetchCount = 16;

                    e.ConfigureConsumer<FaultDirectConsumer>(context);
                });
            });
        });
        
        services.AddHostedService<BusHostedService>();
    })
    .Build();

await host.RunAsync();