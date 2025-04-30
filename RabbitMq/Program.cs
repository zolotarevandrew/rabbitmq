//tbd -
// 1 - simple producer consumer with autoacks - done
// 2 - simple producer consumer without autoacks - done
// 3 - simple producer consumer durable queue - done
// 4 - simple producer consumer durable queue + persistent message - done 
// 5 - simple producer consumer durable queue + persistent message - (message is lost)
//tbd 

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq;

class Program
{

    static async Task Main(string[] args)
    {
    }

    public static async Task RunRpcServer()
    {
        await RpcServer.Start();
    }

    public static async Task RunRpcClient()
    {
        Console.WriteLine("RPC Client");
        string n = "30";
        
        await using var rpcClient = new RpcClient();
        await rpcClient.StartAsync();

        Console.WriteLine(" [x] Requesting fib({0})", n);
        var response = await rpcClient.CallAsync(n);
        Console.WriteLine(" [.] Got '{0}'", response);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static async Task RunConsumerProducer(string[] args)
    {
        Console.Write("Enter mode (publisher / consumer): ");
        var mode = Console.ReadLine()?.Trim().ToLower();

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                switch (mode)
                {
                    case "p":
                        services.AddHostedService<PublisherAlternateExchange>();
                        break;
                    case "c":
                        services.AddHostedService<ConsumerAlternateExchange>();
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


