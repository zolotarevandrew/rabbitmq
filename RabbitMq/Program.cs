//tbd -
// 1 - simple producer consumer with autoacks - done
// 2 - simple producer consumer without autoacks - done
// 3 - simple producer consumer durable queue - done
// 4 - simple producer consumer durable queue + persistent message - done 
// 5 - simple producer consumer durable queue + persistent message - (message is lost)
//tbd 

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using RabbitMq;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter mode (publisher / consumer): ");
        var mode = Console.ReadLine()?.Trim().ToLower();

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                switch (mode)
                {
                    case "p":
                        services.AddHostedService<Publisher>();
                        break;
                    case "c":
                        services.AddHostedService<Consumer>();
                        break;
                    default:
                        Console.WriteLine("Invalid mode. Use 'publisher' or 'consumer'.");
                        Environment.Exit(1);
                        break;
                }
            });

        await builder.Build().RunAsync();
    }
}


