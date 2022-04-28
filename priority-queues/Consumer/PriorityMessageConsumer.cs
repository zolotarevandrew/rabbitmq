using MassTransit;
using Messages;

namespace Consumer;

public class PriorityMessageConsumer : IConsumer<PriorityMessage>
{
    public async Task Consume(ConsumeContext<PriorityMessage> context)
    {
        Console.WriteLine(context.Message.Id + " priority " + context.Message.Priority);
    }
}