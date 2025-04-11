using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Teamscx_AI_WFM_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Call entity
            modelBuilder.Entity<Call>(entity =>
            {
                entity.HasKey(e => e.CallId);

                entity.Property(e => e.CallId)
                    .IsRequired()
                    .HasMaxLength(100);

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

                // Configure JSON columns for arrays
                entity.Property(e => e.AutoAttendants)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));

                entity.Property(e => e.CallQueues)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));

                // Configure relationship with CallUser
                entity.HasMany(e => e.CallUsers)
                    .WithOne(e => e.Call)
                    .HasForeignKey(e => e.CallId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CallUser entity
            modelBuilder.Entity<CallUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CallId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UserName)
                    .IsRequired();

                entity.Property(e => e.IsHunted)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.IsConnected)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Configure relationship with Agent
                entity.HasOne(e => e.Agent)
                    .WithMany()
                    .HasForeignKey(e => e.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}