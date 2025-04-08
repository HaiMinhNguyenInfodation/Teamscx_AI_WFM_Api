using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class AgentActiveHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public int? AgentId { get; set; }

        [Required]
        public bool IsActived { get; set; }

        public int? QueueId { get; set; }

        [ForeignKey("AgentId")]
        public virtual Agent Agent { get; set; }

        [ForeignKey("QueueId")]
        public virtual Queue Queue { get; set; }
    }
} 