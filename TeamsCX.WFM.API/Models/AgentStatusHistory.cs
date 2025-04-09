using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class AgentStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AgentId { get; set; }

        [ForeignKey("AgentId")]
        public Agent Agent { get; set; }

        [Required]
        public AgentStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}