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
        //await PublishDirect( cancellationToken );
    }
    
    private async Task PublishHeaders( CancellationToken cancellationToken )
    {
        await _publishEndpoint.Publish(new HeadersMessage
        {
            Text = "test1"
        }, context =>
        {
            context.Headers.Set("type", "important");
        }, cancellationToken );
    }

    private async Task PublishDirect( CancellationToken cancellationToken )
    {
        await _publishEndpoint.Publish(new DirectMessage
        {
            Key = "important",
            Text = "test",
            Id = Guid.NewGuid( ).ToString( ),
        }, context =>
        {
            context.TimeToLive = TimeSpan.FromSeconds( 20 );
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