using System;
using System.Collections.Generic;

namespace TeamsCX.WFM.API.Models
{
    public class AgentStatusRow
    {
        public string AgentName { get; set; }
        public ScheduleInfo Scheduled { get; set; }
        public TimeSpan IdleTime { get; set; }
        public double Adherence { get; set; }
        public List<HourlyStatus> Actual { get; set; }
    }

    public class ScheduleInfo
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class HourlyStatus
    {
        public DateTime Hour { get; set; }
        public List<StatusDuration> StatusTimes { get; set; }
    }

    public class StatusDuration
    {
        public AgentStatus Status { get; set; }
        public string StatusDescription { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public static class AgentStatusExtensions
    {
        public static string GetDescription(this AgentStatus status)
        {
            return status switch
            {
                AgentStatus.Offline => "Offline",
                AgentStatus.Available => "Available",
                AgentStatus.Busy => "Busy",
                AgentStatus.Away => "Away",
                AgentStatus.DoNotDisturb => "DoNotDisturb",
                AgentStatus.InACall => "InACall",
                AgentStatus.Presenting => "Presenting",
                AgentStatus.Inactive => "Inactive",
                AgentStatus.BeRightBack => "BeRightBack",
                _ => status.ToString()
            };
        }
    }
}