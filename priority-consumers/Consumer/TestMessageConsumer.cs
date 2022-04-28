using MassTransit;
using Messages;

namespace Consumer;

public class TestMessageConsumer : IConsumer<TestMessage>
{
    public async Task Consume(ConsumeContext<TestMessage> context)
    {
        Console.WriteLine(context.Message.Id);
    }
}