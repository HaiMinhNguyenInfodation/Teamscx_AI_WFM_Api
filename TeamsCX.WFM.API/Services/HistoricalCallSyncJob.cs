using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TeamsCX.WFM.API.Services
{
    public class HistoricalCallSyncJob : CallSyncJob
    {
        public HistoricalCallSyncJob(
            ICallRetrievalService callRetrievalService,
            ILogger<HistoricalCallSyncJob> logger,
            string resourceAccounts)
            : base(callRetrievalService, logger, resourceAccounts)
        {
        }

        protected override Task<DateTime> GetSyncStartTimeAsync()
        {
            return Task.FromResult(DateTime.UtcNow.AddMonths(-3).Date);
        }

        protected override Task<DateTime> GetSyncEndTimeAsync()
        {
            return Task.FromResult(DateTime.UtcNow);
        }

        protected override TimeSpan GetDelayTime()
        {
            // Run once and stop
            return TimeSpan.FromDays(1);
        }

        protected override async Task ProcessCallDetailAsync(CallDetail callDetail)
        {
            // TODO: Implement call detail processing
            // This should map the GraphQL response to your database entities
            // and save them to the database
            await Task.CompletedTask;
        }
    }
}