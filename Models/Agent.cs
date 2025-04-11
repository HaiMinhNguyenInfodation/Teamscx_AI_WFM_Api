using System;

namespace TeamsCX.Models
{
    public class Agent
    {
        public int Id { get; set; }
        public string MicrosoftUserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsReported { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}