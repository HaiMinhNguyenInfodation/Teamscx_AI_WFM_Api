using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TeamsCX.WFM.API.Services
{
    public interface ICallRetrievalService
    {
        Task<GraphQLResponse> GetCallDetailsAsync(DateTime from, DateTime to, string resourceAccounts);
    }

    public class CallRetrievalService : ICallRetrievalService
    {
        private readonly GraphQLCallService _graphQLService;
        private readonly ILogger<CallRetrievalService> _logger;

        public CallRetrievalService(
            GraphQLCallService graphQLService,
            ILogger<CallRetrievalService> logger)
        {
            _graphQLService = graphQLService;
            _logger = logger;
        }

        public async Task<GraphQLResponse> GetCallDetailsAsync(DateTime from, DateTime to, string resourceAccounts)
        {
            try
            {
                _logger.LogInformation($"Retrieving call details from {from} to {to} for resource accounts: {resourceAccounts}");
                var response = await _graphQLService.GetCallDetailsAsync(from, to, resourceAccounts);
                _logger.LogInformation($"Successfully retrieved {response?.Data?.CallDetails?.Length ?? 0} call details");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving call details");
                throw;
            }
        }
    }
}