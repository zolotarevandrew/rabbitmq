using System;
using System.Threading.Tasks;
using MassTransit;

namespace Headers;



public record LogAppended(string Message);

public class LogAppendedConsumer : IConsumer<LogAppended>
{
    public async Task Consume(ConsumeContext<LogAppended> context)
    {
        Console.WriteLine(context.Message.Message + ", " + context.MessageId);
        await Task.CompletedTask;
    }
}