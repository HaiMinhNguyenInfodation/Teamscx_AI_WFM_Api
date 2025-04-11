using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TeamsCX.WFM.API.Services
{
    public class RealTimeCallSyncJob : CallSyncJob
    {
        private DateTime _lastSyncTime;

        public RealTimeCallSyncJob(
            ICallRetrievalService callRetrievalService,
            ILogger<RealTimeCallSyncJob> logger,
            string resourceAccounts)
            : base(callRetrievalService, logger, resourceAccounts)
        {
            _lastSyncTime = DateTime.UtcNow.AddMinutes(-10);
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
            // Skip in-progress calls
            if (callDetail.StatusLive == "InProgress")
            {
                return;
            }

            // TODO: Implement call detail processing
            // This should map the GraphQL response to your database entities
            // and save them to the database
            await Task.CompletedTask;
        }
    }
}