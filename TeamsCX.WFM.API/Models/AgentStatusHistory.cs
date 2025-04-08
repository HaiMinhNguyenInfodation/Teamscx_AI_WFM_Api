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
        public DateTime CreatedAt { get; set; }

        public int? AgentId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Status { get; set; }

        [ForeignKey("AgentId")]
        public virtual Agent Agent { get; set; }
    }
} 