using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Services
{
    public abstract class CallSyncJob : BackgroundService
    {
        protected readonly ICallRetrievalService _callRetrievalService;
        protected readonly ILogger<CallSyncJob> _logger;
        protected readonly string _resourceAccounts;

        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        protected CallSyncJob(
            ICallRetrievalService callRetrievalService,
            ILogger<CallSyncJob> logger,
            string resourceAccounts)
        {
            _callRetrievalService = callRetrievalService;
            _logger = logger;
            _resourceAccounts = resourceAccounts;
        }

        protected abstract Task<DateTime> GetSyncStartTimeAsync();
        protected abstract Task<DateTime> GetSyncEndTimeAsync();

        public async Task TriggerSyncAsync()
        {
            if (!await _syncLock.WaitAsync(0))
            {
                _logger.LogWarning("Sync is already in progress");
                return;
            }

            try
            {
                await ExecuteSyncAsync();
            }
            finally
            {
                _syncLock.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteSyncAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during call sync");
                }

                await Task.Delay(GetDelayTime(), stoppingToken);
            }
        }

        private async Task ExecuteSyncAsync()
        {
            var from = await GetSyncStartTimeAsync();
            var to = await GetSyncEndTimeAsync();

            _logger.LogInformation($"Starting call sync from {from} to {to}");

            var response = await _callRetrievalService.GetCallDetailsAsync(from, to, _resourceAccounts);

            if (response?.Data?.CallDetails != null)
            {
                foreach (var callDetail in response.Data.CallDetails)
                {
                    await ProcessCallDetailAsync(callDetail);
                }
            }

            _logger.LogInformation("Calls sync completed successfully");
        }

        protected abstract TimeSpan GetDelayTime();
        protected abstract Task ProcessCallDetailAsync(CallDetail callDetail);
    }
}