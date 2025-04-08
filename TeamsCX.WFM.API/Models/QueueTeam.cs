using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class QueueTeam
    {
        [Key]
        [Column(Order = 0)]
        public int QueueId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int TeamId { get; set; }

        [ForeignKey("QueueId")]
        public virtual Queue Queue { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
} 