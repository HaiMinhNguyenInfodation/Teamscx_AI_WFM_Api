namespace TeamsCX.WFM.API.Models
{
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