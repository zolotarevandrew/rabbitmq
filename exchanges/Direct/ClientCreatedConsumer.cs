using System;
using System.Threading.Tasks;
using MassTransit;

namespace Direct;

public record ClientCreated(string Key, string Name);

public class ClientCreatedConsumer : IConsumer<ClientCreated>
{
    public async Task Consume(ConsumeContext<ClientCreated> context)
    {
        Console.WriteLine(context.Message.Key + " " + context.Message.Name + ", " + context.MessageId);
        await Task.CompletedTask;
    }
}