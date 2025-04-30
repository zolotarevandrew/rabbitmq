using MassTransit;
using Microsoft.Extensions.Hosting;
using RabbitTransit.Direct;

namespace RabbitTransit;

public class BusHostedService : IHostedService
{
    private readonly IBusControl _busControl;
    private readonly IPublishEndpoint _publishEndpoint;

    public BusHostedService(IBusControl busControl, IPublishEndpoint publishEndpoint )
    {
        _busControl = busControl;
        _publishEndpoint = publishEndpoint;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _busControl.StartAsync(cancellationToken);
        
        await _publishEndpoint.Publish(new DirectMessage
        {
            Text = "Hello, world!"
        }, cancellationToken );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _busControl.StopAsync(cancellationToken);
    }
}