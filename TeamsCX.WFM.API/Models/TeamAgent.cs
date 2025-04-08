using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class TeamAgent
    {
        [Key]
        [Column(Order = 0)]
        public int TeamId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int AgentId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        [ForeignKey("AgentId")]
        public virtual Agent Agent { get; set; }
    }
} 