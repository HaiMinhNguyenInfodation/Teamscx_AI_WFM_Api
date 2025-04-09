namespace TeamsCX.WFM.API.Models
{
    public enum AgentStatus
    {
        Offline = 0,        // Agent is not available
        Available = 1,      // Agent is available and ready
        Busy = 2,          // Agent is busy with a call
        Away = 3,          // Agent is away
        DoNotDisturb = 4,  // Agent is in Do Not Disturb mode
        InACall = 5,       // Agent is in a call
        Presenting = 6,    // Agent is presenting
        Inactive = 7       // Agent is inactive in the queue
    }
}