using MassTransit;

namespace RabbitTransit.Direct;

public class DirectConsumer : IConsumer<DirectMessage>
{
    public Task Consume(ConsumeContext<DirectMessage> context)
    {
        Console.WriteLine($"Received: {context.Message.Text}");
        throw new Exception( "test" );
        return Task.CompletedTask;
    }
}

public class FaultDirectConsumer : IConsumer<Fault<DirectMessage>>
{
    public Task Consume(ConsumeContext<Fault<DirectMessage>> context)
    {
        Console.WriteLine($"Received: {context.Message.Message.Text}");
        return Task.CompletedTask;
    }
}

public class DirectMessage
{
    public string Text { get; set; }
}