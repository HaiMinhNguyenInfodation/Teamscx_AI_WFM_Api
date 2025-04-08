using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsCX.WFM.API.Models
{
    public class TeamSchedulingGroup
    {
        [Key]
        [Column(Order = 0)]
        public int TeamId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SchedulingGroupId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        [ForeignKey("SchedulingGroupId")]
        public virtual SchedulingGroup SchedulingGroup { get; set; }
    }
} 