using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Queue> Queues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<SchedulingGroup> SchedulingGroups { get; set; }
        public DbSet<ScheduleShift> ScheduleShifts { get; set; }
        public DbSet<QueueTeam> QueueTeams { get; set; }
        public DbSet<TeamAgent> TeamAgents { get; set; }
        public DbSet<TeamSchedulingGroup> TeamSchedulingGroups { get; set; }
        public DbSet<AgentActiveHistory> AgentActiveHistories { get; set; }
        public DbSet<AgentStatusHistory> AgentStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and indexes
            modelBuilder.Entity<Queue>()
                .HasIndex(q => q.MicrosoftQueueId)
                .IsUnique();

            modelBuilder.Entity<Team>()
                .HasIndex(t => t.MicrosoftTeamId)
                .IsUnique();

            modelBuilder.Entity<Team>()
                .Property(t => t.OwnerId)
                .IsRequired(false);

            modelBuilder.Entity<Agent>()
                .HasIndex(a => a.MicrosoftUserId)
                .IsUnique();

            modelBuilder.Entity<Agent>()
                .Property(a => a.Email)
                .IsRequired(false);

            modelBuilder.Entity<SchedulingGroup>()
                .HasIndex(sg => sg.MicrosoftGroupId)
                .IsUnique();

            modelBuilder.Entity<ScheduleShift>()
                .HasIndex(ss => ss.MicrosoftShiftId)
                .IsUnique();

            modelBuilder.Entity<ScheduleShift>()
                .Property(ss => ss.Notes)
                .IsRequired(false);

            modelBuilder.Entity<ScheduleShift>()
                .Property(ss => ss.Theme)
                .IsRequired(false);

            // Configure many-to-many relationships
            modelBuilder.Entity<QueueTeam>()
                .HasKey(qt => new { qt.QueueId, qt.TeamId });

            modelBuilder.Entity<TeamAgent>()
                .HasKey(ta => new { ta.TeamId, ta.AgentId });

            modelBuilder.Entity<TeamSchedulingGroup>()
                .HasKey(tsg => new { tsg.TeamId, tsg.SchedulingGroupId });

            // Configure history tables
            modelBuilder.Entity<AgentActiveHistory>()
                .HasIndex(aah => aah.CreatedAt);

            modelBuilder.Entity<AgentStatusHistory>()
                .HasIndex(ash => ash.CreatedAt);
        }
    }
} 