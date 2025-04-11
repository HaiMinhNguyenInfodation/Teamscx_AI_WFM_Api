using System;
using System.Collections.Generic;

namespace TeamsCX.WFM.API.Models
{
    public class Call
    {
        public string Id { get; set; }
        public List<string> CallChainIds { get; set; }
        public Direction Direction { get; set; }
        public CallStatus CallStatus { get; set; }
        public CallOutcome CallOutcome { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public double WaitingDuration { get; set; }
        public double ConnectedDuration { get; set; }
        public double CallDuration { get; set; }
        public bool Hunted { get; set; }
        public bool Connected { get; set; }
        public string CallerId { get; set; }
        public string CallerPrincipalName { get; set; }
        public string CallerDisplayName { get; set; }
        public string CallerName { get; set; }
        public string CallerCompany { get; set; }
        public List<string> AutoAttendants { get; set; }
        public List<string> CallQueues { get; set; }
        public bool IsForceEnded { get; set; }
        public List<CallUser> HuntedUsers { get; set; }
        public List<CallUser> ConnectedUsers { get; set; }
    }

    public class CallUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AgentId { get; set; }
        public Agent Agent { get; set; }
        public string CallId { get; set; }
        public Call Call { get; set; }
    }
}