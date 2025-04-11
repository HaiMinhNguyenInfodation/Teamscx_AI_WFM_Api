using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Models;
using System.Text.Json;

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
        public DbSet<QueueReportedAgent> QueueReportedAgents { get; set; }
        public DbSet<CallDirection> CallDirections { get; set; } = null!;
        public DbSet<Caller> Callers { get; set; } = null!;
        public DbSet<CallStatus> CallStatuses { get; set; } = null!;
        public DbSet<CallOutcome> CallOutcomes { get; set; } = null!;
        public DbSet<Calls> Calls { get; set; } = null!;
        public DbSet<CallQueueReported> CallQueues { get; set; } = null!;
        public DbSet<CallConnectedUser> CallConnectedUsers { get; set; } = null!;
        public DbSet<CallHuntedUser> CallHuntedUsers { get; set; } = null!;
        public DbSet<CallActivity> CallActivities { get; set; } = null!;
        public DbSet<CallActivityMapping> CallActivityMappings { get; set; } = null!;
        public DbSet<Classification> Classifications { get; set; } = null!;
        public DbSet<Note> Notes { get; set; } = null!;

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

            modelBuilder.Entity<QueueReportedAgent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Queue)
                    .WithMany()
                    .HasForeignKey(e => e.QueueId);

                entity.HasOne(e => e.Agent)
                    .WithMany()
                    .HasForeignKey(e => e.AgentId);
            });

            modelBuilder.Entity<Agent>()
                .Property(e => e.IsReported)
                .HasDefaultValue(false);

            // Configure CallDirection
            modelBuilder.Entity<CallDirection>()
                .HasIndex(cd => cd.Direction)
                .IsUnique();

            // Configure CallStatus
            modelBuilder.Entity<CallStatus>()
                .HasIndex(cs => cs.Status)
                .IsUnique();

            // Configure CallOutcome
            modelBuilder.Entity<CallOutcome>()
                .HasIndex(co => co.Outcome)
                .IsUnique();

            // Configure CallQueueReported
            modelBuilder.Entity<CallQueueReported>()
                .HasIndex(cq => cq.CallId);
            modelBuilder.Entity<CallQueueReported>()
                .HasIndex(cq => cq.QueueId);

            // Configure CallConnectedUser
            modelBuilder.Entity<CallConnectedUser>()
                .HasIndex(ccu => ccu.CallId);
            modelBuilder.Entity<CallConnectedUser>()
                .HasIndex(ccu => ccu.AgentId);

            // Configure CallHuntedUser
            modelBuilder.Entity<CallHuntedUser>()
                .HasIndex(chu => chu.CallId);
            modelBuilder.Entity<CallHuntedUser>()
                .HasIndex(chu => chu.AgentId);

            // Configure CallActivityMapping
            modelBuilder.Entity<CallActivityMapping>()
                .HasIndex(cam => cam.CallId);
            modelBuilder.Entity<CallActivityMapping>()
                .HasIndex(cam => cam.ActivityId);

            // Configure Classification
            modelBuilder.Entity<Classification>()
                .HasIndex(c => c.CallId);
            modelBuilder.Entity<Classification>()
                .HasIndex(c => c.AgentId);

            // Configure Note
            modelBuilder.Entity<Note>()
                .HasIndex(n => n.CallId);
            modelBuilder.Entity<Note>()
                .HasIndex(n => n.AgentId);

            // Seed initial data
            modelBuilder.Entity<CallStatus>().HasData(
                new CallStatus { Id = 1, Status = "Waiting" },
                new CallStatus { Id = 2, Status = "Connected" },
                new CallStatus { Id = 3, Status = "Ended" }
            );

            modelBuilder.Entity<CallOutcome>().HasData(
                new CallOutcome { Id = 1, Outcome = "Answered" },
                new CallOutcome { Id = 2, Outcome = "Missed" },
                new CallOutcome { Id = 3, Outcome = "Abandoned" },
                new CallOutcome { Id = 4, Outcome = "Inprogress" }
            );

            modelBuilder.Entity<CallDirection>().HasData(
                new CallDirection { Id = 1, Direction = "Inbound" },
                new CallDirection { Id = 2, Direction = "Outbound" }
            );
        }
    }
}