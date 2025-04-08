using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class ScheduleShift
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MicrosoftShiftId { get; set; }
        
        [Required]
        public int AgentId { get; set; }
        
        [Required]
        public int SchedulingGroupId { get; set; }
        
        [Required]
        public DateTime StartDateTime { get; set; }
        
        [Required]
        public DateTime EndDateTime { get; set; }
        
        [MaxLength(50)]
        public string Theme { get; set; }
        
        public string Notes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        [ForeignKey("AgentId")]
        public virtual Agent Agent { get; set; }
        
        [ForeignKey("SchedulingGroupId")]
        public virtual SchedulingGroup SchedulingGroup { get; set; }
    }
} 