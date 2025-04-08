using System;
using System.ComponentModel.DataAnnotations;

namespace TeamsCX.WFM.API.Models
{
    public class Agent
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MicrosoftUserId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string DisplayName { get; set; }
        
        [MaxLength(255)]
        public string Email { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
} 