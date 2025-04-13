using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        public bool IsReported { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}