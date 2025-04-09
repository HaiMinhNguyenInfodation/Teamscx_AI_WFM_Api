using System;

namespace TeamsCX.WFM.API.Models.RealTime
{
    public class AgentRealTimeStatus
    {
        public string AgentDisplayName { get; set; }
        public string ScheduledCallQueue { get; set; }
        public bool Adherence { get; set; }
        public DateTime StatusTime { get; set; }
        public string Status { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string ActiveInCallQueue { get; set; }
    }

    public class RealTimeOverview
    {
        public AgentStatusSummary AgentStatus { get; set; }
        public AgentMetrics AgentMetrics { get; set; }
        public List<AgentRealTimeStatus> AgentDetails { get; set; }
    }

    public class AgentStatusSummary
    {
        public int TotalAgents { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; }
    }

    public class AgentMetrics
    {
        public string AgentLoggedInRatio { get; set; }
        public string AgentIdleRatio { get; set; }
        public double AgentIdleRate { get; set; }
    }
}