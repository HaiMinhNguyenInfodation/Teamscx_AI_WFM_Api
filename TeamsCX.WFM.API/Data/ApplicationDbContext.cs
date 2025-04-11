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
        public DbSet<Call> Calls { get; set; }
        public DbSet<CallUser> CallUsers { get; set; }

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

            // Configure Call entity
            modelBuilder.Entity<Call>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CallId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CallerId).HasMaxLength(100);
                entity.Property(e => e.CallerName).HasMaxLength(200);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.StartedAt).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
                entity.Property(e => e.WaitingDuration).IsRequired();
                entity.Property(e => e.ConnectedDuration).IsRequired();
                entity.Property(e => e.CallDuration).IsRequired();
                entity.Property(e => e.Direction).IsRequired();
                entity.Property(e => e.CallStatus).IsRequired();
                entity.Property(e => e.CallOutcome).IsRequired();
                entity.Property(e => e.CallQueues).HasMaxLength(500);
                entity.Property(e => e.AutoAttendants).HasMaxLength(500);
                entity.Property(e => e.ResourceAccounts).HasMaxLength(500);
                entity.Property(e => e.IsForceEnded).IsRequired();

                entity.HasMany(e => e.CallUsers)
                    .WithOne(e => e.Call)
                    .HasForeignKey(e => e.CallId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CallUser entity
            modelBuilder.Entity<CallUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CallId).IsRequired();
                entity.Property(e => e.AgentId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                entity.HasOne(e => e.Call)
                    .WithMany(e => e.CallUsers)
                    .HasForeignKey(e => e.CallId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Agent)
                    .WithMany()
                    .HasForeignKey(e => e.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}