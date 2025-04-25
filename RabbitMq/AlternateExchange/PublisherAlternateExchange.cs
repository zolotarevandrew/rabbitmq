using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMq;

public class PublisherAlternateExchange : BackgroundService
{
    private readonly ConnectionFactory _factory;

    public PublisherAlternateExchange()
    {
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await channel.ExchangeDeclareAsync(
            exchange: "my_ae", 
            type: ExchangeType.Fanout, 
            durable: true, cancellationToken: stoppingToken);

        // Объявляем очередь и привязываем её к альтернативному обменнику
        await channel.QueueDeclareAsync(
            queue: "unrouted_queue", 
            durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        
        await channel.QueueBindAsync(queue: "unrouted_queue", exchange: "my_ae", routingKey: "", cancellationToken: stoppingToken);

        // Аргументы для основного обменника, включая указание AE
        var args = new Dictionary<string, object>
        {
            { "alternate-exchange", "my_ae" }
        };
        
        await channel.ExchangeDeclareAsync(exchange: "my_direct", 
            type: ExchangeType.Direct, durable: true, arguments: args!, cancellationToken: stoppingToken);

        
        await channel.QueueDeclareAsync(queue: "routed_queue", 
            durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queue: "routed_queue", exchange: "my_direct", routingKey: "key1", cancellationToken: stoppingToken);
        
        var properties = new BasicProperties
        {
            Persistent = true
        };

        var body = "Without Routing Key"u8.ToArray();
        
        await channel.BasicPublishAsync(
            exchange: "my_direct", 
            routingKey: "unknown_key",
            mandatory: true,
            basicProperties: properties,
            body: body, cancellationToken: stoppingToken);
    }
}