using TeamsCX.WFM.API.Models.DTOs;
using System.Threading.Tasks;

namespace TeamsCX.WFM.API.Services
{
    public interface IAdherenceService
    {
        Task<Models.DTOs.AdherenceResponse> GetDashboardSummaryAsync(string[] queueMicrosoftIds = null);
        Task<QueueMetricsResponse> GetQueueMetricsAsync(string[] queueMicrosoftIds = null);
    }
}