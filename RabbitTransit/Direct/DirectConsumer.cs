using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace RabbitTransit.Direct;

public class DirectConsumer : IConsumer<DirectMessage>
{
    public Task Consume(ConsumeContext<DirectMessage> context)
    {
        Console.WriteLine($"Received: {context.Message.Text}");
        return Task.CompletedTask;
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
            e.ConfigureConsumeTopology = false;
                    
            e.Bind<DirectMessage>(bind =>
            {
                bind.ExchangeType = ExchangeType.Direct;
                bind.RoutingKey = "important";
            });
            e.PrefetchCount = 16;
            e.ConfigureConsumer<DirectConsumer>( context );
        });
    }
}

public class DirectMessage
{
    public string Key { get; set; }
    public string Text { get; set; }
}