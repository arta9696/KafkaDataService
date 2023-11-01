using Confluent.Kafka;
using KafkaDataService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace KafkaDataService
{
    public class KafkaCommentService : IHostedService
    {
        private readonly WebContext _context;
        static ConsumerConfig config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "dotnet-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        public KafkaCommentService(IServiceScopeFactory serviceScopeFactory)
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
                consumer.Subscribe("Comments");

                while (true)
                {
                    try
                    {
                        var result = consumer.Consume(cancellationToken);
                        if (result == null) { continue; }
                        if (result.IsPartitionEOF) { if (cancellationToken.IsCancellationRequested) { break; } Thread.Sleep(1000); }
                        _context.Add(JsonSerializer.Deserialize<Comments>(result.Message.Value));
                        _context.SaveChanges();
                    }
                    catch (OperationCanceledException)
                    {
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
