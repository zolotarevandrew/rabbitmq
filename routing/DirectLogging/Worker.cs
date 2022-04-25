using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectLogging
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
                    .Publish(new LogCreated(LogType.Info, "info test"), stoppingToken);
                
                await scope.ServiceProvider
                    .GetRequiredService<IPublishEndpoint>()
                    .Publish(new LogCreated(LogType.Warn, "warn test"), stoppingToken);
                
                await scope.ServiceProvider
                    .GetRequiredService<IPublishEndpoint>()
                    .Publish(new LogCreated(LogType.Error, "error test"), stoppingToken);
                
                await Task.Delay(1500, stoppingToken);
            }
        }
    }
}