using System.Collections.Generic;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models.DTOs;
using TeamsCX.WFM.API.Repositories;

namespace TeamsCX.WFM.API.Services
{
    public class AgentPerformanceService : IAgentPerformanceService
    {
        private readonly IAgentPerformanceRepository _repository;

        public AgentPerformanceService(IAgentPerformanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<AgentPerformanceResponseDTO> GetAgentPerformanceAsync(List<string> queueMicrosoftId)
        {
            return await _repository.GetAgentPerformanceAsync(queueMicrosoftId);
        }
    }
}