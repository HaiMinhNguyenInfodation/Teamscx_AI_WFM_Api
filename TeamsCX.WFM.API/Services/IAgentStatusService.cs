using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Models.DTOs;

namespace TeamsCX.WFM.API.Services
{
    public interface IAgentStatusService
    {
        Task<List<AgentStatusHistoryDTO>> GetAgentStatusHistoryAsync(List<string> callQueues, DateTime startTime, DateTime endTime);
        Task<List<AgentScheduleDTO>> GetAgentSchedulesAsync(List<string> callQueues, DateTime date);
        Task<List<int>> GetAgentsInCallQueuesAsync(List<string> callQueues);
    }

    public class AgentStatusHistory
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AgentStatus Status { get; set; }
    }

    public class AgentSchedule
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}