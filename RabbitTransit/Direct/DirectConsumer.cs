using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace RabbitTransit.Direct;

public class DirectConsumer : IConsumer<DirectMessage>
{
    public async Task Consume(ConsumeContext<DirectMessage> context)
    {
        //await Task.Delay( TimeSpan.FromSeconds( 5 ) );
        Console.WriteLine($"Received: {context.Message.Id}" + " " + context.GetRetryAttempt(  ));
        throw new Exception( "wtf" );
    }
}

public static class Extensions
{
    
    public static void ConfigureDirectConsumer( this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context )
    {
        cfg.Message<DirectMessage>(d => 
            d.SetEntityName("content.received"));
                
        cfg.Send<DirectMessage>(dd =>
        {
            dd.UseRoutingKeyFormatter( cc => cc.Message.Key );
        });

        cfg.Publish<DirectMessage>(xx =>
        {
            xx.ExchangeType = ExchangeType.Direct;
        });
                
        cfg.ReceiveEndpoint("direct-queue", e =>
        {
            e.SetQueueArgument("x-dead-letter-exchange", "dlx.exchange");
            e.SetQueueArgument("x-dead-letter-routing-key", "expired");
            
            e.ConfigureConsumeTopology = false;
                    
            e.Bind<DirectMessage>(bind =>
            {
                bind.ExchangeType = ExchangeType.Direct;
                bind.RoutingKey = "important";
            });
            e.PrefetchCount = 1;
            e.UseMessageRetry( r => r.Intervals(
                TimeSpan.FromSeconds( 1 ),
                TimeSpan.FromSeconds( 5 ), 
                TimeSpan.FromSeconds( 10 )
                ) 
            );
            e.ConfigureConsumer<DirectConsumer>( context );
        });
        
        cfg.ReceiveEndpoint("expired-queue", e =>
        {
            e.Bind("dlx.exchange", x =>
            {
                x.RoutingKey = "expired";
            });
            
            e.PrefetchCount = 1;

            e.Handler<DirectMessage>(ctx =>
            {
                Console.WriteLine($"TTL expired: {ctx.Message.Text}");
                return Task.CompletedTask;
            });
        });
    }
}

public class DirectMessage
{
    public string Key { get; set; }
    public string Id { get; set; }
    public string Text { get; set; }
}