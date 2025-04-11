using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeamsCX.WFM.API.Models
{
    public class Call
    {
        [Key]
        public string CallId { get; set; }
        public string? CallerId { get; set; }
        public string? CallerName { get; set; }
        public string? CompanyName { get; set; }
        [Required]
        public DateTime StartedAt { get; set; }
        [Required]
        public DateTime LastUpdated { get; set; }
        public double WaitingDuration { get; set; }
        public double ConnectedDuration { get; set; }
        public double CallDuration { get; set; }
        [Required]
        public Direction Direction { get; set; }
        [Required]
        public CallStatus CallStatus { get; set; }
        [Required]
        public CallOutcome CallOutcome { get; set; }
        public List<string> CallQueues { get; set; } = new List<string>();
        public List<string> AutoAttendants { get; set; } = new List<string>();
        public string? ResourceAccounts { get; set; }
        public bool IsForceEnded { get; set; }
        public List<CallUser> CallUsers { get; set; } = new List<CallUser>();
    }

    public enum CallStatus
    {
        InProgress = 0,
        Ended = 1
    }

    public enum CallOutcome
    {
        InProgress = 0,
        Answered = 1,
        Missed = 2,
        Abandoned = 3
    }

    public enum Direction
    {
        Inbound = 0,
        Outbound = 1
    }
}