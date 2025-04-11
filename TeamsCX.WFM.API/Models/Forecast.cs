using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class Forecast
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QueueId { get; set; }

        [Required]
        public DateTime DayOfForecast { get; set; }

        [Required]
        [MaxLength(10)]
        public required string ForecastPeriod { get; set; }

        public int CallForecastMorning { get; set; }
        public int CallForecastAfternoon { get; set; }
        public int MaxCallForecastOnHourMorning { get; set; }
        public int MaxCallForecastOnHourAfternoon { get; set; }
        public decimal CallPerAgentMorning { get; set; }
        public decimal CallPerAgentAfternoon { get; set; }
        public int AgentCapacityMorning { get; set; }
        public int AgentCapacityAfternoon { get; set; }
        public int OpenShiftNeedMorning { get; set; }
        public int OpenShiftNeedAfternoon { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("QueueId")]
        public virtual Queue Queue { get; set; }
    }
}
