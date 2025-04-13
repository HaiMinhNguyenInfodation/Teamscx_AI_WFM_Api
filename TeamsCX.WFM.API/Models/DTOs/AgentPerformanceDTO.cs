using System.Collections.Generic;

namespace TeamsCX.WFM.API.Models.DTOs
{
    public class AgentPerformanceResponseDTO
    {
        public List<AgentPerformanceDTO> Agents { get; set; }
    }

    public class AgentPerformanceDTO
    {
        public string AgentName { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime StartedTime { get; set; }
        public List<string> ActiveCQ { get; set; }
        public string ScheduledCQ { get; set; }
    }
}