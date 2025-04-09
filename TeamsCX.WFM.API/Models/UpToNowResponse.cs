using System;
using System.Collections.Generic;

namespace TeamsCX.WFM.API.Models
{
    public class UpToNowResponse
    {
        public TimeSpan AverageWaitTime { get; set; }
        public TimeSpan AverageTalkTime { get; set; }
        public double Adherence { get; set; }
        public double Conformance { get; set; }
        public string MissedCalls { get; set; }  // Format: "x/y"
        public double AnsweredCallsPerAgent { get; set; }
        public int EarlyLogOut { get; set; }
        public int LateLogIn { get; set; }
        public List<AgentStatusRow> AgentStatuses { get; set; }
    }
}