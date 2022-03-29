using System;
using System.Threading.Tasks;
using MassTransit;

namespace Exchanges;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        Console.WriteLine(context.Message.Name + ", " + context.MessageId);
        await Task.CompletedTask;
    }
}