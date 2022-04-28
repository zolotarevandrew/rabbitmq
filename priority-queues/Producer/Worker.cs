using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Producer
{
    public static class PriorityMessageExtensions
    {
        public static async Task PublishPriorityMessage<T>(this IPublishEndpoint endpoint, T message, CancellationToken token)
            where T: IPriorityMessage
        {
            await endpoint.Publish(message, p => p.SetPriority(message.Priority),  token);
        }
    }
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            byte idx = 10;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (idx == 0) idx = 10;
                using var scope = _serviceProvider.CreateScope();
                
                await scope.ServiceProvider
                    .GetRequiredService<IPublishEndpoint>()
                    .PublishPriorityMessage(new PriorityMessage(Guid.NewGuid().ToString("N"), idx),  stoppingToken);

                await Task.Delay(500, stoppingToken);
                idx--;
            }
        }
    }
}