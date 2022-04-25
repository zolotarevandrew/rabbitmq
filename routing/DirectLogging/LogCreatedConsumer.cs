using System;
using System.Threading.Tasks;
using MassTransit;

namespace DirectLogging;

public enum LogType
{
    Info = 1,
    Warn = 2,
    Error = 3
}

public record LogCreated(LogType Type, string Message);

public class LogCreatedConsumer : IConsumer<LogCreated>
{
    public async Task Consume(ConsumeContext<LogCreated> context)
    {
        Console.WriteLine(context.SourceAddress + " ," + context.Message.Type + " " + context.Message.Message);
        await Task.CompletedTask;
    }
}