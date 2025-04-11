using Microsoft.EntityFrameworkCore;
using System;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Queue> Queues { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<QueueReportedAgent> QueueReportedAgents { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<CallUser> CallUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QueueReportedAgent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Configure relationships
                entity.HasOne(e => e.Queue)
                    .WithMany()
                    .HasForeignKey(e => e.QueueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Agent)
                    .WithMany()
                    .HasForeignKey(e => e.AgentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure IsReported property for Agent
            modelBuilder.Entity<Agent>()
                .Property(e => e.IsReported)
                .HasDefaultValue(false);

            // Configure Call entity
            modelBuilder.Entity<Call>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CallId).IsRequired();
                entity.Property(e => e.StartDateTime).IsRequired();
                entity.Property(e => e.EndDateTime).IsRequired();
                entity.Property(e => e.Duration).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Direction).IsRequired();
                entity.Property(e => e.Outcome).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                entity.HasMany(e => e.CallUsers)
                    .WithOne(e => e.Call)
                    .HasForeignKey(e => e.CallId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Direction)
                    .HasConversion<int>();

                entity.Property(e => e.CallStatus)
                    .HasConversion<int>();

                entity.Property(e => e.CallOutcome)
                    .HasConversion<int>();

                entity.Property(e => e.StartedAt)
                    .IsRequired();

                entity.Property(e => e.LastUpdated)
                    .IsRequired();

                entity.Property(e => e.CallerId)
                    .IsRequired();

                // Configure relationships with CallUser
                entity.HasMany(e => e.HuntedUsers)
                    .WithOne(e => e.Call)
                    .HasForeignKey(e => e.CallId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ConnectedUsers)
                    .WithOne(e => e.Call)
                    .HasForeignKey(e => e.CallId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CallUser entity
            modelBuilder.Entity<CallUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired();

                // Configure relationship with Agent
                entity.HasOne(e => e.Agent)
                    .WithMany()
                    .HasForeignKey(e => e.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}