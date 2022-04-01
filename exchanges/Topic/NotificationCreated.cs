using System;
using System.Threading.Tasks;
using MassTransit;

namespace Topic;



public record NotificationCreated(string From, string Name);

public class NotificationCreatedConsumer : IConsumer<NotificationCreated>
{
    public async Task Consume(ConsumeContext<NotificationCreated> context)
    {
        Console.WriteLine(context.Message.From + " " + context.Message.Name + ", " + context.MessageId);
        await Task.CompletedTask;
    }
}