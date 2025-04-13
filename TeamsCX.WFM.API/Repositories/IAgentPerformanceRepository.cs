using System.Collections.Generic;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models.DTOs;

namespace TeamsCX.WFM.API.Repositories
{
    public interface IAgentPerformanceRepository
    {
        Task<AgentPerformanceResponseDTO> GetAgentPerformanceAsync(List<string> queueMicrosoftId);
    }
}