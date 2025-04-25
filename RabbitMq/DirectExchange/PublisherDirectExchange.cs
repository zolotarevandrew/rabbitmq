using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMq;

public class PublisherDirectExchange : BackgroundService
{
    private readonly ConnectionFactory _factory;

    public PublisherDirectExchange()
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
            exchange: "logs_direct", 
            type: ExchangeType.Direct, 
            cancellationToken: stoppingToken);

        int count = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            

            var properties = new BasicProperties
            {
                Persistent = true
            };
            
            var arr = new string[]
            {
                "error", 
                "warn", 
                "debug"
            };
            
            var random = new Random();
            var randomElement = arr[random.Next(arr.Length)];
            
            string message = $"Hello world {++count} - {randomElement}";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "logs_direct", 
                routingKey: randomElement,
                mandatory: true,
                basicProperties: properties,
                body: body, cancellationToken: stoppingToken);

            Console.WriteLine($" [x] Sent: {message}");

            await Task.Delay(100, stoppingToken);
        }
    }
}