using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TeamsCX.WFM.API.Services
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly ILogger<SyncBackgroundService> _logger;
        private readonly SyncService _syncService;
        private readonly TimeSpan _syncInterval = TimeSpan.FromDays(1);
        private readonly TimeSpan _initialDelay = TimeSpan.FromMinutes(1);

        public SyncBackgroundService(ILogger<SyncBackgroundService> logger, SyncService syncService)
        {
            _logger = logger;
            _syncService = syncService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Initial delay to allow the application to start up
                await Task.Delay(_initialDelay, stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Starting scheduled data sync");
                        await _syncService.SyncAllDataAsync();
                        _logger.LogInformation("Completed scheduled data sync");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during data sync");
                    }

                    // Calculate the next midnight
                    var now = DateTime.UtcNow;
                    var nextMidnight = now.Date.AddDays(1);
                    var delay = nextMidnight - now;

                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Sync background service is stopping");
            }
        }
    }
} 