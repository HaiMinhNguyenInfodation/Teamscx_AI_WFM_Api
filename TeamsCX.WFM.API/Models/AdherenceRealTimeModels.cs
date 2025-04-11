using System;
using System.Collections.Generic;

namespace TeamsCX.WFM.API.Models
{
    public class AdherenceRealTimeSummaryResponse
    {
        public AgentsSummary AgentsSummary { get; set; }
        public AgentStatusSummary AgentStatusSummary { get; set; }
    }

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

    public class AgentStatusSummary
    {
        public int TotalAgents { get; set; }
        public int AvailableCount { get; set; }
        public int OnCallCount { get; set; }
        public int BreakCount { get; set; }
        public int MeetingCount { get; set; }
        public int OfflineCount { get; set; }
    }

    public class AgentPerformanceResponse
    {
        public List<AgentPerformance> Agents { get; set; }
    }

    public class AgentPerformance
    {
        public string AgentName { get; set; }
        public string CurrentStatus { get; set; }
        public TimeSpan StatusDuration { get; set; }
        public string ActiveCQ { get; set; }
        public string ScheduledCQ { get; set; }
    }

    public class QueueMetricsResponse
    {
        public List<QueueMetrics> Queues { get; set; }
    }

    public class QueueMetrics
    {
        public string Queue { get; set; }
        public int WaitingCalls { get; set; }
        public int ConnectingCalls { get; set; }
        public int MissedCalls { get; set; }
        public int AnsweredCalls { get; set; }
        public TimeSpan AverageWaitingTime { get; set; }
        public TimeSpan AverageHandleTime { get; set; }
        public double SLAPercentage { get; set; }
    }

    public class AgentActivitiesResponse
    {
        public List<AgentActivity> Activities { get; set; }
    }

    public class AgentActivity
    {
        public string AgentName { get; set; }
        public string ScheduledTime { get; set; }
        public int IdleTime { get; set; }
        public int Adherence { get; set; }
        public List<ActivitySlot> Timeline { get; set; }
    }

    public class ActivitySlot
    {
        public string ActivityType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
    }
} 