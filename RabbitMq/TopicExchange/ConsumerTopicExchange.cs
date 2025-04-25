using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq;

public class ConsumerTopicExchange : BackgroundService
{
    private readonly ILogger<ConsumerFanoutExchange> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public ConsumerTopicExchange(ILogger<ConsumerFanoutExchange> logger)
    {
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var queueName = "prod_logs";
        await _channel.QueueDeclareAsync(
            queue: queueName, 
            durable: true, 
            exclusive: false, 
            autoDelete: false,
            arguments: null, cancellationToken: cancellationToken);
        
        await _channel.QueueBindAsync(
            queue: queueName, 
            exchange: "topic_logs", 
            routingKey: "prod.#", 
            cancellationToken: cancellationToken);
        
        await _channel.QueueBindAsync(
            queue: queueName, 
            exchange: "topic_logs", 
            routingKey: "*.warn", 
            cancellationToken: cancellationToken);
        
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        _logger.LogInformation("Connected to RabbitMQ and declared queue.");
        
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message: {Message}", message);
            await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        };

        await _channel!.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.DisposeAsync();
        _connection?.DisposeAsync();
        return base.StopAsync(cancellationToken);
    }
}