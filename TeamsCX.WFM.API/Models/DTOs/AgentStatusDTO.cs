using System;

namespace TeamsCX.WFM.API.Models.DTOs
{

    public class AgentStatusHistoryDTO
    {
        public string Id { get; set; }
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AgentStatus Status { get; set; }
        public string CallQueueId { get; set; }
    }

    public class AgentScheduleDTO
    {
        public string Id { get; set; }
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CallQueueId { get; set; }
    }
}