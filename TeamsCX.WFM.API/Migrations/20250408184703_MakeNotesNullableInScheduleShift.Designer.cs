﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeamsCX.WFM.API.Data;

#nullable disable

namespace TeamsCX.WFM.API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250408184703_MakeNotesNullableInScheduleShift")]
    partial class MakeNotesNullableInScheduleShift
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TeamsCX.WFM.API.Models.Agent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("MicrosoftUserId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("MicrosoftUserId")
                        .IsUnique();

                    b.ToTable("Agents");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.AgentActiveHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AgentId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActived")
                        .HasColumnType("bit");

                    b.Property<int?>("QueueId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AgentId");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("QueueId");

                    b.ToTable("AgentActiveHistories");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.AgentStatusHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AgentId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("AgentId");

                    b.HasIndex("CreatedAt");

                    b.ToTable("AgentStatusHistories");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.Queue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MicrosoftQueueId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("MicrosoftQueueId")
                        .IsUnique();

                    b.ToTable("Queues");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.QueueTeam", b =>
                {
                    b.Property<int>("QueueId")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("TeamId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("QueueId", "TeamId");

                    b.HasIndex("TeamId");

                    b.ToTable("QueueTeams");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.ScheduleShift", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AgentId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("EndDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("MicrosoftShiftId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SchedulingGroupId")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Theme")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AgentId");

                    b.HasIndex("MicrosoftShiftId")
                        .IsUnique();

                    b.HasIndex("SchedulingGroupId");

                    b.ToTable("ScheduleShifts");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.SchedulingGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("MicrosoftGroupId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("MicrosoftGroupId")
                        .IsUnique();

                    b.ToTable("SchedulingGroups");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("MicrosoftTeamId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("OwnerId")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("MicrosoftTeamId")
                        .IsUnique();

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.TeamAgent", b =>
                {
                    b.Property<int>("TeamId")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("AgentId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("TeamId", "AgentId");

                    b.HasIndex("AgentId");

                    b.ToTable("TeamAgents");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.TeamSchedulingGroup", b =>
                {
                    b.Property<int>("TeamId")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("SchedulingGroupId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("TeamId", "SchedulingGroupId");

                    b.HasIndex("SchedulingGroupId");

                    b.ToTable("TeamSchedulingGroups");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.AgentActiveHistory", b =>
                {
                    b.HasOne("TeamsCX.WFM.API.Models.Agent", "Agent")
                        .WithMany()
                        .HasForeignKey("AgentId");

                    b.HasOne("TeamsCX.WFM.API.Models.Queue", "Queue")
                        .WithMany()
                        .HasForeignKey("QueueId");

                    b.Navigation("Agent");

                    b.Navigation("Queue");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.AgentStatusHistory", b =>
                {
                    b.HasOne("TeamsCX.WFM.API.Models.Agent", "Agent")
                        .WithMany()
                        .HasForeignKey("AgentId");

                    b.Navigation("Agent");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.QueueTeam", b =>
                {
                    b.HasOne("TeamsCX.WFM.API.Models.Queue", "Queue")
                        .WithMany()
                        .HasForeignKey("QueueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamsCX.WFM.API.Models.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Queue");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.ScheduleShift", b =>
                {
                    b.HasOne("TeamsCX.WFM.API.Models.Agent", "Agent")
                        .WithMany()
                        .HasForeignKey("AgentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamsCX.WFM.API.Models.SchedulingGroup", "SchedulingGroup")
                        .WithMany()
                        .HasForeignKey("SchedulingGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Agent");

                    b.Navigation("SchedulingGroup");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.TeamAgent", b =>
                {
                    b.HasOne("TeamsCX.WFM.API.Models.Agent", "Agent")
                        .WithMany()
                        .HasForeignKey("AgentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamsCX.WFM.API.Models.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Agent");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("TeamsCX.WFM.API.Models.TeamSchedulingGroup", b =>
                {
                    b.HasOne("TeamsCX.WFM.API.Models.SchedulingGroup", "SchedulingGroup")
                        .WithMany()
                        .HasForeignKey("SchedulingGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamsCX.WFM.API.Models.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SchedulingGroup");

                    b.Navigation("Team");
                });
#pragma warning restore 612, 618
        }
    }
}
