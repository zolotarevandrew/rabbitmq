using MassTransit;
using Messages;

namespace Consumer;

public class JobCreatedConsumer : IConsumer<JobCreated>
{
    public async Task Consume(ConsumeContext<JobCreated> context)
    {
        await Task.Delay(2000);
        Console.WriteLine(context.Message.Id + "Simulating - " + context.Message.Name);
    }
}