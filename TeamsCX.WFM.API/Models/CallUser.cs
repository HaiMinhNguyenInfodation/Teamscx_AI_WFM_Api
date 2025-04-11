using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class CallUser
    {
        [Key]
        public int? Id { get; set; }
        public int? CallId { get; set; }
        public int? AgentId { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UserName { get; set; }
        public bool? IsHunted { get; set; }
        public bool? IsConnected { get; set; }

        [ForeignKey("CallId")]
        public Call? Call { get; set; }
        [ForeignKey("AgentId")]
        public Agent? Agent { get; set; }
    }
}