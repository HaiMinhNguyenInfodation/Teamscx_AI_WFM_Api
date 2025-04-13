using System.Text.Json.Serialization;

namespace TeamsCX.WFM.API.Models.DTOs
{
    public class AdherenceResponse
    {
        public AgentsSummary AgentsSummary { get; set; }
        public AgentStatusDistribution AgentStatusDistribution { get; set; }
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

    public class AgentStatusDistribution
    {
        public int TotalAgents { get; set; }
        public StatusData StatusData { get; set; }
    }

    public class StatusData
    {
        public int Available { get; set; }
        public int Busy { get; set; }
        public int DoNotDisturb { get; set; }
        public int Away { get; set; }
        public int Offline { get; set; }
        public int InACall { get; set; }
        public int Presenting { get; set; }
        public int Inactive { get; set; }
        public int BeRightBack { get; set; }
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
        public required string AgentDisplayName { get; set; }
        public required string CurrentStatus { get; set; }
        public AgentScheduled Scheduled { get; set; } = new AgentScheduled();
        public AgentActual Actual { get; set; } = new AgentActual();
        public required string Adherence { get; set; }
    }

    public class AgentScheduled
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AgentShift> Shifts { get; set; } = new List<AgentShift>();
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public double TotalHours { get; set; }
    }

    public class AgentShift
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public double TotalSeconds { get { return From.HasValue && To.HasValue ? To.Value.Subtract(From.Value).TotalSeconds : 0; } }
        public string? DisplayText { get; set; }
        public string? OnQueue { get; set; }
        public List<AgentShiftActivity> Activities { get; set; } = new List<AgentShiftActivity>();
    }

    public class AgentShiftActivity
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public string? DisplayText { get; set; }
        public double TotalSeconds { get { return From.HasValue && To.HasValue ? To.Value.Subtract(From.Value).TotalSeconds : 0; } }
        public string? Theme { get; set; }
    }

    public class AgentActual
    {
        public List<AgentTimeline>? Timelines { get; set; } = new List<AgentTimeline>();
        public List<ActiveTimeline> ActiveTimelines { get; set; } = new List<ActiveTimeline>();
    }

    public class AgentTimeline
    {
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public string? OnQueue { get; set; }
        public string? Status { get; set; }
    }

    public class ActiveTimeline
    {
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string OnQueue { get; set; } = string.Empty;
    }
}