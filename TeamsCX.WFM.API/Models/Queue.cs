using System.ComponentModel.DataAnnotations;

namespace TeamsCX.WFM.API.Models
{
    public class Queue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string MicrosoftQueueId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Forecast> Forecasts { get; set; } = new List<Forecast>();
    }
}