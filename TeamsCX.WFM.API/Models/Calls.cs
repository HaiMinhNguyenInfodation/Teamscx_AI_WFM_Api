using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class CallDirection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Direction { get; set; } = string.Empty;
    }

    public class Caller
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string? CallerPhoneNumber { get; set; }

        [StringLength(255)]
        public string? CallerPrincipalName { get; set; }

        [StringLength(255)]
        public string? CallerDisplayName { get; set; }

        [StringLength(255)]
        public string? CallerName { get; set; }

        [StringLength(255)]
        public string? CallerCompany { get; set; }
    }

    public class CallStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }

    public class CallOutcome
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Outcome { get; set; } = string.Empty;
    }

    public class Calls
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(24)]
        public string CallId { get; set; } = string.Empty;

        public int CallStatusId { get; set; }
        [ForeignKey("CallStatusId")]
        public CallStatus CallStatus { get; set; } = null!;

        public int CallOutcomeId { get; set; }
        [ForeignKey("CallOutcomeId")]
        public CallOutcome CallOutcome { get; set; } = null!;

        public DateTime StartedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public double WaitingDuration { get; set; }
        public double ConnectedDuration { get; set; }
        public double CallDuration { get; set; }
        public bool Hunted { get; set; }
        public bool Connected { get; set; }

        public int? CallerId { get; set; }
        [ForeignKey("CallerId")]
        public Caller? Caller { get; set; }

        public bool IsForceEnded { get; set; }

        public int CallDirectionId { get; set; }
        [ForeignKey("CallDirectionId")]
        public CallDirection CallDirection { get; set; } = null!;

        public ICollection<CallQueueReported> CallQueues { get; set; } = new List<CallQueueReported>();
        public ICollection<CallConnectedUser> CallConnectedUsers { get; set; } = new List<CallConnectedUser>();
        public ICollection<CallHuntedUser> CallHuntedUsers { get; set; } = new List<CallHuntedUser>();
        public ICollection<CallActivityMapping> CallActivityMappings { get; set; } = new List<CallActivityMapping>();
        public ICollection<Classification> Classifications { get; set; } = new List<Classification>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }

    public class CallQueueReported
    {
        [Key]
        public int Id { get; set; }

        public int CallId { get; set; }
        [ForeignKey("CallId")]
        public Calls Call { get; set; } = null!;

        public int QueueId { get; set; }
        [ForeignKey("QueueId")]
        public Queue Queue { get; set; } = null!;

        public DateTime AccessTime { get; set; }
    }

    public class CallConnectedUser
    {
        [Key]
        public int Id { get; set; }

        public int CallId { get; set; }
        [ForeignKey("CallId")]
        public Calls Call { get; set; } = null!;

        public int AgentId { get; set; }
        [ForeignKey("AgentId")]
        public Agent Agent { get; set; } = null!;

        [StringLength(255)]
        public string? ExternalUserPhoneNumber { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class CallHuntedUser
    {
        [Key]
        public int Id { get; set; }

        public int CallId { get; set; }
        [ForeignKey("CallId")]
        public Calls Call { get; set; } = null!;

        public int AgentId { get; set; }
        [ForeignKey("AgentId")]
        public Agent Agent { get; set; } = null!;

        [StringLength(255)]
        public string? ExternalUserPhoneNumber { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class CallActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Activity { get; set; } = string.Empty;

        public ICollection<CallActivityMapping> CallActivityMappings { get; set; } = new List<CallActivityMapping>();
    }

    public class CallActivityMapping
    {
        [Key]
        public int Id { get; set; }

        public int CallId { get; set; }
        [ForeignKey("CallId")]
        public Calls Call { get; set; } = null!;

        public int ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public CallActivity CallActivity { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class Classification
    {
        [Key]
        public int Id { get; set; }

        public int CallId { get; set; }
        [ForeignKey("CallId")]
        public Calls Call { get; set; } = null!;

        public DateTime Timestamp { get; set; }

        public int AgentId { get; set; }
        [ForeignKey("AgentId")]
        public Agent Agent { get; set; } = null!;

        [StringLength(255)]
        public string? Tags { get; set; }
    }

    public class Note
    {
        [Key]
        public int Id { get; set; }

        public int CallId { get; set; }
        [ForeignKey("CallId")]
        public Calls Call { get; set; } = null!;

        public DateTime Timestamp { get; set; }

        public int AgentId { get; set; }
        [ForeignKey("AgentId")]
        public Agent Agent { get; set; } = null!;

        public string NoteContent { get; set; } = string.Empty;
    }
}