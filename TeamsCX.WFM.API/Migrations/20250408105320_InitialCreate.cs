using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamsCX.WFM.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicrosoftUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicrosoftQueueId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulingGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicrosoftGroupId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicrosoftTeamId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentStatusHistories_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AgentActiveHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: false),
                    QueueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentActiveHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentActiveHistories_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgentActiveHistories_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicrosoftShiftId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    SchedulingGroupId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleShifts_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleShifts_SchedulingGroups_SchedulingGroupId",
                        column: x => x.SchedulingGroupId,
                        principalTable: "SchedulingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueueTeams",
                columns: table => new
                {
                    QueueId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueTeams", x => new { x.QueueId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_QueueTeams_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamAgents",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamAgents", x => new { x.TeamId, x.AgentId });
                    table.ForeignKey(
                        name: "FK_TeamAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamAgents_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamSchedulingGroups",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    SchedulingGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSchedulingGroups", x => new { x.TeamId, x.SchedulingGroupId });
                    table.ForeignKey(
                        name: "FK_TeamSchedulingGroups_SchedulingGroups_SchedulingGroupId",
                        column: x => x.SchedulingGroupId,
                        principalTable: "SchedulingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamSchedulingGroups_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentActiveHistories_AgentId",
                table: "AgentActiveHistories",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentActiveHistories_CreatedAt",
                table: "AgentActiveHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AgentActiveHistories_QueueId",
                table: "AgentActiveHistories",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_MicrosoftUserId",
                table: "Agents",
                column: "MicrosoftUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentStatusHistories_AgentId",
                table: "AgentStatusHistories",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentStatusHistories_CreatedAt",
                table: "AgentStatusHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_MicrosoftQueueId",
                table: "Queues",
                column: "MicrosoftQueueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueTeams_TeamId",
                table: "QueueTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleShifts_AgentId",
                table: "ScheduleShifts",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleShifts_MicrosoftShiftId",
                table: "ScheduleShifts",
                column: "MicrosoftShiftId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleShifts_SchedulingGroupId",
                table: "ScheduleShifts",
                column: "SchedulingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulingGroups_MicrosoftGroupId",
                table: "SchedulingGroups",
                column: "MicrosoftGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamAgents_AgentId",
                table: "TeamAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_MicrosoftTeamId",
                table: "Teams",
                column: "MicrosoftTeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamSchedulingGroups_SchedulingGroupId",
                table: "TeamSchedulingGroups",
                column: "SchedulingGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentActiveHistories");

            migrationBuilder.DropTable(
                name: "AgentStatusHistories");

            migrationBuilder.DropTable(
                name: "QueueTeams");

            migrationBuilder.DropTable(
                name: "ScheduleShifts");

            migrationBuilder.DropTable(
                name: "TeamAgents");

            migrationBuilder.DropTable(
                name: "TeamSchedulingGroups");

            migrationBuilder.DropTable(
                name: "Queues");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "SchedulingGroups");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
