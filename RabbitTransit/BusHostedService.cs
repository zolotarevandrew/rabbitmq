using MassTransit;
using Microsoft.Extensions.Hosting;
using RabbitTransit.Direct;
using RabbitTransit.Topic;

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
        await PublishTopic( cancellationToken );
    }

    private async Task PublishDirect( CancellationToken cancellationToken )
    {
        await _publishEndpoint.Publish(new TopicMessage
        {
            Key = "important",
            Text = "test"
        }, cancellationToken );
    }
    
    private async Task PublishTopic( CancellationToken cancellationToken )
    {
        await _publishEndpoint.Publish(new TopicMessage
        {
            Key = "my.important",
            Text = "test"
        }, cancellationToken );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _busControl.StopAsync(cancellationToken);
    }
}