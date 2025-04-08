using System;
using System.ComponentModel.DataAnnotations;

namespace TeamsCX.WFM.API.Models
{
    public class SchedulingGroup
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MicrosoftGroupId { get; set; }
        
        [MaxLength(255)]
        public string DisplayName { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
} 