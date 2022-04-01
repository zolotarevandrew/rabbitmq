using System;
using System.Threading;
using System.Threading.Tasks;
using Headers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Direct
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
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                
                await scope.ServiceProvider
                    .GetRequiredService<IPublishEndpoint>()
                    .Publish(new LogAppended("test"), p =>
                    {
                        p.Headers.Set("Test-header", "value1");
                        p.Headers.Set("Test-header2", "value2");
                    }, stoppingToken);
                
                await Task.Delay(1500, stoppingToken);
            }
        }
    }
}