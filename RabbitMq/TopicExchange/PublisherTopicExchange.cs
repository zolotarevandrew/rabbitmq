using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMq;

public class PublisherTopicExchange : BackgroundService
{
    private readonly ConnectionFactory _factory;

    public PublisherTopicExchange()
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
            exchange: "topic_logs", 
            type: ExchangeType.Topic, 
            cancellationToken: stoppingToken);

        int count = 0;
        
        var severity = new string[]
        {
            "info", 
            "warn", 
            "debug"
        };

        var stage = new string[]
        {
            "local",
            "stage",
            "prod"
        };
        
        //local.info
        //local.warn
        //local.debug
        
        //stage.info

        while (!stoppingToken.IsCancellationRequested)
        {
            var properties = new BasicProperties
            {
                Persistent = true
            };
            
            var random = new Random();
            var severityElement = severity[random.Next(severity.Length)];
            var stageElement = stage[random.Next(stage.Length)];
            var sendElement = $"{stageElement}.{severityElement}";
            
            string message = $"Hello world {++count} - {sendElement}";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "topic_logs", 
                routingKey: sendElement,
                mandatory: true,
                basicProperties: properties,
                body: body, cancellationToken: stoppingToken);

            Console.WriteLine($" [x] Sent: {message}");

            await Task.Delay(100, stoppingToken);
        }
    }
}