using Confluent.Kafka;
using KafkaDataService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace KafkaDataService
{
    public class KafkaPostsService : IHostedService
    {
        private readonly WebContext _context;
        static ConsumerConfig config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "dotnet-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        public KafkaPostsService(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _context = scope.ServiceProvider.GetService<WebContext>();
            }
        }
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}")).Build())
            {
                consumer.Subscribe("Posts");

                while (true)
                {
                    try
                    {
                        var result = consumer.Consume(cancellationToken);
                        if (result == null) { continue; }
                        if (result.IsPartitionEOF) { if (cancellationToken.IsCancellationRequested) { break; } Thread.Sleep(1000); }
                        _context.Add(JsonSerializer.Deserialize<Posts>(result.Message.Value));
                        _context.SaveChanges();
                    
                    }
                    catch (OperationCanceledException) {
                        break;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                consumer.Close();
            }
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
