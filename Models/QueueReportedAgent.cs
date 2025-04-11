using System;

public class QueueReportedAgent
{
    public int Id { get; set; }
    public int QueueId { get; set; }
    public int AgentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Queue Queue { get; set; }
    public virtual Agent Agent { get; set; }
}