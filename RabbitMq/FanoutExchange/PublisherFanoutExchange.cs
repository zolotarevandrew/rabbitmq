using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMq;

public class PublisherFanoutExchange : BackgroundService
{
    private readonly ConnectionFactory _factory;

    public PublisherFanoutExchange()
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
            exchange: "logs", 
            type: ExchangeType.Fanout, 
            cancellationToken: stoppingToken);

        int count = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            string message = $"Hello world {++count}";
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: "logs", 
                routingKey: string.Empty,
                mandatory: true,
                basicProperties: properties,
                body: body, cancellationToken: stoppingToken);

            Console.WriteLine($" [x] Sent: {message}");

            await Task.Delay(100, stoppingToken);
        }
    }
}