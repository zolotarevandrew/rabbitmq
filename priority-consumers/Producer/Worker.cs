using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Producer
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int idx = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                
                await scope.ServiceProvider
                    .GetRequiredService<IPublishEndpoint>()
                    .Publish(new TestMessage(idx.ToString()),  stoppingToken);

                await Task.Delay(500, stoppingToken);
                idx++;
            }
        }
    }
}