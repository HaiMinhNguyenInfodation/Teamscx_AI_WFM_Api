using System;
using System.ComponentModel.DataAnnotations;

namespace TeamsCX.WFM.API.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MicrosoftTeamId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string DisplayName { get; set; }
        
        public string Description { get; set; }
        
        [MaxLength(100)]
        public string OwnerId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
} 