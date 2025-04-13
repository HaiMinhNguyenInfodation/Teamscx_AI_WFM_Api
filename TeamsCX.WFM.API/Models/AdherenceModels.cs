using System.Collections.Generic;

namespace TeamsCX.WFM.API.Models
{
    public class AgentsSummary
    {
        public int TotalAgents { get; set; }
        public int AgentsLoggedIn { get; set; }
        public double AgentsIdlePercentage { get; set; }
        public double AdherencePercentage { get; set; }
        public double ConformancePercentage { get; set; }
        public int EarlyLogOut { get; set; }
        public int LateLogIn { get; set; }
    }

    public class StatusData
    {
        public int Available { get; set; }
        public int Busy { get; set; }
        public int DoNotDisturb { get; set; }
        public int Away { get; set; }
        public int Offline { get; set; }
        public int InACall { get; set; }
    }

    public class AgentStatusDistribution
    {
        public int TotalAgents { get; set; }
        public StatusData StatusData { get; set; }
    }

    public class AdherenceResponse
    {
        public AgentsSummary AgentsSummary { get; set; }
        public AgentStatusDistribution AgentStatusDistribution { get; set; }
    }
}