using System.Threading.Tasks;
using TeamsCX.WFM.API.Models.DTOs;

namespace TeamsCX.WFM.API.Repositories
{
    public interface IAdherenceRepository
    {
        Task<Models.DTOs.AdherenceResponse> GetDashboardSummaryAsync(string[] queueMicrosoftIds = null);
        Task<Models.DTOs.AgentStatusDistribution> GetAgentStatusDistributionAsync(string[] queueMicrosoftIds = null);
        Task<Models.DTOs.AgentsSummary> GetAgentsSummaryAsync(string[] queueMicrosoftIds = null);
        Task<QueueMetricsResponse> GetQueueMetricsAsync(string[] queueMicrosoftIds = null);
    }
}