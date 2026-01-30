using Eroad.CQRS.Core.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eroad.RouteManagement.Query.Infrastructure.Consumers
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Task _consumerTask;
        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _startupCompleted;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _startupCompleted = new TaskCompletionSource<bool>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service starting.");
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _startupCompleted = new TaskCompletionSource<bool>();

            _consumerTask = Task.Run(async () =>
            {
                try
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
                        var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? "route-management-events";

                        _logger.LogInformation("Consumer initialized, starting to consume from topic: {Topic}", topic);
                        _startupCompleted.TrySetResult(true);

                        await eventConsumer.ConsumeAsync(topic, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Event consumer was cancelled");
                    _startupCompleted.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error in event consumer");
                    _startupCompleted.TrySetException(ex);
                    throw;
                }
            }, _cancellationTokenSource.Token);

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(10));
                    await _startupCompleted.Task.ConfigureAwait(false);
                    _logger.LogInformation("Event consumer service started successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start event consumer service");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service stopping.");
            _cancellationTokenSource?.Cancel();

            if (_consumerTask != null)
            {
                try
                {
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(30));
                        await _consumerTask.ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Consumer shutdown timeout exceeded");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during consumer shutdown");
                }
            }

            _cancellationTokenSource?.Dispose();
        }
    }
}
