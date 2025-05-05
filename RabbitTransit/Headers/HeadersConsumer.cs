using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace RabbitTransit.Topic;

public class HeadersConsumer : IConsumer<HeadersMessage>
{
    public Task Consume(ConsumeContext<HeadersMessage> context)
    {
        Console.WriteLine($"Headers Received: {context.Message.Text}");
        return Task.CompletedTask;
    }
}

public static class HeadersExtensions
{
    
    public static void ConfigureHeadersConsumer( this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context )
    {
        cfg.Message<HeadersMessage>(d => 
            d.SetEntityName("headers.received"));

        cfg.Publish<HeadersMessage>(xx =>
        {
            xx.ExchangeType = ExchangeType.Headers;
        });
                
        cfg.ReceiveEndpoint("headers-queue", e =>
        {
            e.ConfigureConsumeTopology = false;
                    
            e.Bind<HeadersMessage>(bind =>
            {
                bind.ExchangeType = ExchangeType.Headers;
                bind.SetBindingArgument("type", "important");
                bind.SetBindingArgument("x-match", "all");
            });
            e.PrefetchCount = 16;
            e.ConfigureConsumer<HeadersConsumer>( context );
        });
    }
}

public class HeadersMessage
{
    public string Text { get; set; }
}