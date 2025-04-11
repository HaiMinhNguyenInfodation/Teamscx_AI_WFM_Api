using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Services;

namespace TeamsCX.WFM.API.Services.BackgroundJobs
{
    public class QueueReportedAgentsSyncJob : BackgroundService
    {
        private readonly ILogger<QueueReportedAgentsSyncJob> _logger;
        private readonly IQueueReportedAgentService _queueReportedAgentService;

        public QueueReportedAgentsSyncJob(
            ILogger<QueueReportedAgentsSyncJob> logger,
            IQueueReportedAgentService queueReportedAgentService)
        {
            _logger = logger;
            _queueReportedAgentService = queueReportedAgentService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var nextRun = now.Date.AddHours(2); // 2:00 AM UTC
                    if (now > nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    await Task.Delay(delay, stoppingToken);

                    await _queueReportedAgentService.SyncReportedAgentsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation exceptions
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing reported agents");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retrying
                }
            }
        }
    }
}