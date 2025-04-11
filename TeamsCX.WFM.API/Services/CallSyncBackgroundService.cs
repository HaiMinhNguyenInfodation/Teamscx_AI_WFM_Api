using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TeamsCX.WFM.API.Services
{
    public class CallSyncBackgroundService : BackgroundService
    {
        private readonly CallSyncService _callSyncService;
        private readonly ILogger<CallSyncBackgroundService> _logger;
        private readonly string _resourceAccounts;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(10);

        public CallSyncBackgroundService(
            CallSyncService callSyncService,
            ILogger<CallSyncBackgroundService> logger,
            IConfiguration configuration)
        {
            _callSyncService = callSyncService;
            _logger = logger;
            _resourceAccounts = configuration["CallSync:ResourceAccounts"] ?? throw new ArgumentNullException("CallSync:ResourceAccounts configuration is missing");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // First, sync historical data
                _logger.LogInformation("Starting historical call sync");
                //await _callSyncService.SyncHistoricalCallsAsync(_resourceAccounts);
                _logger.LogInformation("Historical call sync completed");

                // Then start periodic sync
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Starting periodic call sync");
                        //await _callSyncService.SyncRecentCallsAsync(_resourceAccounts);
                        _logger.LogInformation("Periodic call sync completed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during periodic call sync");
                    }

                    await Task.Delay(_syncInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in call sync background service");
                throw;
            }
        }
    }
}