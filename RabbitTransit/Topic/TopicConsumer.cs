using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace RabbitTransit.Topic;

public class TopicConsumer : IConsumer<TopicMessage>
{
    public Task Consume(ConsumeContext<TopicMessage> context)
    {
        Console.WriteLine($"Topic Received: {context.Message.Text}");
        return Task.CompletedTask;
    }
}

public static class TopicExtensions
{
    
    public static void ConfigureTopicConsumer( this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context )
    {
        cfg.Message<TopicMessage>(d => 
            d.SetEntityName("topic.received"));
                
        cfg.Send<TopicMessage>(dd =>
        {
            dd.UseRoutingKeyFormatter( cc => cc.Message.Key );
        });

        cfg.Publish<TopicMessage>(xx =>
        {
            xx.ExchangeType = ExchangeType.Topic;
        });
                
        cfg.ReceiveEndpoint("topic-queue", e =>
        {
            e.ConfigureConsumeTopology = false;
                    
            e.Bind<TopicMessage>(bind =>
            {
                bind.ExchangeType = ExchangeType.Topic;
                bind.RoutingKey = "*.important";
            });
            e.PrefetchCount = 16;
            e.ConfigureConsumer<TopicConsumer>( context );
        });
    }
}

public class TopicMessage
{
    public string Key { get; set; }
    public string Text { get; set; }
}