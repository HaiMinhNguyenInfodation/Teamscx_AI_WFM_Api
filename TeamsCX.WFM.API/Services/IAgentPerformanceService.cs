using System.Collections.Generic;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models.DTOs;

namespace TeamsCX.WFM.API.Services
{
    public interface IAgentPerformanceService
    {
        Task<AgentPerformanceResponseDTO> GetAgentPerformanceAsync(List<string> queueMicrosoftId);
    }
}