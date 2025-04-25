using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMq;

public class Publisher : BackgroundService
{
    private readonly ConnectionFactory _factory;

    public Publisher()
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

        await channel.QueueDeclareAsync(
            queue: "logs_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null, cancellationToken: stoppingToken);

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
                exchange: "",
                routingKey: "task_queue",
                mandatory: true,
                basicProperties: properties,
                body: body, cancellationToken: stoppingToken);

            Console.WriteLine($" [x] Sent: {message}");

            await Task.Delay(100, stoppingToken);
        }
    }
}