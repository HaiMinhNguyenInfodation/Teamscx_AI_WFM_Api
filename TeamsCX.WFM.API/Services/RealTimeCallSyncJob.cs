using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TeamsCX.WFM.API.Services
{
    public class RealTimeCallSyncJob : CallSyncJob
    {
        private DateTime _lastSyncTime;
        private readonly IServiceProvider _serviceProvider;

        public RealTimeCallSyncJob(
            ICallRetrievalService callRetrievalService,
            ILogger<RealTimeCallSyncJob> logger,
            string resourceAccounts,
            IServiceProvider serviceProvider)
            : base(callRetrievalService, logger, resourceAccounts)
        {
            _lastSyncTime = DateTime.UtcNow.AddMinutes(-10);
            _serviceProvider = serviceProvider;
        }

        protected override Task<DateTime> GetSyncStartTimeAsync()
        {
            return Task.FromResult(_lastSyncTime);
        }

        protected override Task<DateTime> GetSyncEndTimeAsync()
        {
            var now = DateTime.UtcNow;
            _lastSyncTime = now;
            return Task.FromResult(now);
        }

        protected override TimeSpan GetDelayTime()
        {
            return TimeSpan.FromMinutes(10);
        }

        protected override async Task ProcessCallDetailAsync(CallDetail callDetail)
        {
            using var scope = _serviceProvider.CreateScope();
            var callSyncService = scope.ServiceProvider.GetRequiredService<CallSyncService>();

            try
            {
                await callSyncService.ProcessCallDetailAsync(callDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing call detail for call {CallId}", callDetail.Id);
                throw;
            }
        }
    }
}