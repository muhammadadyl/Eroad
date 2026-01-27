using Eroad.CQRS.Core.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eroad.FleetManagement.Query.Infrastructure.Consumers
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Task _consumerTask;
        private CancellationTokenSource _cancellationTokenSource;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service starting.");
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _consumerTask = Task.Run(async () =>
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
                    var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? "fleet-management-events";

                    try
                    {
                        await eventConsumer.ConsumeAsync(topic, _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in event consumer");
                    }
                }
            }, _cancellationTokenSource.Token);

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service stopping.");
            _cancellationTokenSource?.Cancel();

            if (_consumerTask != null)
            {
                await _consumerTask;
            }

            _cancellationTokenSource?.Dispose();
        }
    }
}
