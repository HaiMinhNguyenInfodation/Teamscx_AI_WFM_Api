using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Services
{
    public class SyncService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMicrosoftGraphService _graphService;
        private readonly ILogger<SyncService> _logger;

        public SyncService(IServiceProvider serviceProvider, IMicrosoftGraphService graphService, ILogger<SyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _graphService = graphService;
            _logger = logger;
        }

        public async Task SyncAllDataAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await SyncTeamsAsync(context);
                await SyncAgentsAsync(context);
                await SyncSchedulingGroupsAsync(context);
                await SyncScheduleShiftsAsync(context);
                await SyncRelationshipsAsync(context);
            }
        }

        private async Task SyncTeamsAsync(ApplicationDbContext context)
        {
            var teamsResponse = await _graphService.GetTeamsAsync();
            var teams = teamsResponse.GetProperty("value").EnumerateArray();

            foreach (var team in teams)
            {
                var microsoftTeamId = team.GetProperty("id").GetString();
                var existingTeam = await context.Teams
                    .FirstOrDefaultAsync(t => t.MicrosoftTeamId == microsoftTeamId);

                if (existingTeam == null)
                {
                    existingTeam = new Team
                    {
                        MicrosoftTeamId = microsoftTeamId,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Teams.Add(existingTeam);
                }

                existingTeam.DisplayName = team.GetProperty("displayName").GetString();
                existingTeam.Description = team.GetProperty("description").GetString();
                existingTeam.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        private async Task SyncAgentsAsync(ApplicationDbContext context)
        {
            var teams = await context.Teams.ToListAsync();

            foreach (var team in teams)
            {
                var membersResponse = await _graphService.GetTeamMembersAsync(team.MicrosoftTeamId);
                var members = membersResponse.GetProperty("value").EnumerateArray();

                // Reset owner ID before processing members
                team.OwnerId = null;
                context.Entry(team).State = EntityState.Modified;

                foreach (var member in members)
                {
                    var microsoftUserId = member.GetProperty("userId").GetString();
                    var existingAgent = await context.Agents
                        .FirstOrDefaultAsync(a => a.MicrosoftUserId == microsoftUserId);

                    if (existingAgent == null)
                    {
                        existingAgent = new Agent
                        {
                            MicrosoftUserId = microsoftUserId,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.Agents.Add(existingAgent);
                    }

                    existingAgent.DisplayName = member.GetProperty("displayName").GetString();
                    existingAgent.Email = member.GetProperty("email").GetString();
                    existingAgent.UpdatedAt = DateTime.UtcNow;

                    // Ensure the agent is tracked by the context
                    if (context.Entry(existingAgent).State == EntityState.Detached)
                    {
                        context.Agents.Attach(existingAgent);
                    }

                    // Create TeamAgent relationship
                    var teamAgent = await context.TeamAgents
                        .FirstOrDefaultAsync(ta => ta.TeamId == team.Id && ta.AgentId == existingAgent.Id);

                    if (teamAgent == null)
                    {
                        teamAgent = new TeamAgent
                        {
                            TeamId = team.Id,
                            AgentId = existingAgent.Id,
                            Team = team,
                            Agent = existingAgent
                        };
                        context.TeamAgents.Add(teamAgent);
                    }

                    // Check if member is owner and update team's OwnerId
                    var roles = member.GetProperty("roles").EnumerateArray();
                    var rolesList = roles.Select(r => r.GetString()).ToList();
                    if (rolesList.Contains("owner"))
                    {
                        _logger.LogInformation($"Setting owner for team {team.DisplayName} to {existingAgent.DisplayName}");
                        team.OwnerId = microsoftUserId;
                        team.UpdatedAt = DateTime.UtcNow;
                        context.Entry(team).State = EntityState.Modified;
                    }
                }

                // Ensure changes are saved for each team
                await context.SaveChangesAsync();
            }
        }

        private async Task SyncSchedulingGroupsAsync(ApplicationDbContext context)
        {
            var teams = await context.Teams.ToListAsync();
            _logger.LogInformation($"Syncing scheduling groups for {teams.Count} teams");
            foreach (var team in teams)
            {
                _logger.LogInformation($"Syncing scheduling groups for team {team.DisplayName}, owner: {team.OwnerId}, id: {team.Id}, microsoftTeamId: {team.MicrosoftTeamId}");
                if (string.IsNullOrEmpty(team.OwnerId))
                {
                    _logger.LogInformation($"Skipping scheduling group sync for team {team.DisplayName} as it has no owner");
                    continue;
                }

                var groupsResponse = await _graphService.GetSchedulingGroupsAsync(team.MicrosoftTeamId, team.OwnerId);
                var groups = groupsResponse.GetProperty("value").EnumerateArray();

                _logger.LogInformation($"Processing scheduling groups for team {team.DisplayName} with owner {team.OwnerId}");

                foreach (var group in groups)
                {
                    var microsoftGroupId = group.GetProperty("id").GetString();
                    var existingGroup = await context.SchedulingGroups
                        .FirstOrDefaultAsync(sg => sg.MicrosoftGroupId == microsoftGroupId);

                    if (existingGroup == null)
                    {
                        existingGroup = new SchedulingGroup
                        {
                            MicrosoftGroupId = microsoftGroupId,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.SchedulingGroups.Add(existingGroup);
                        _logger.LogInformation($"Created new scheduling group with ID {microsoftGroupId}");
                    }

                    existingGroup.DisplayName = group.GetProperty("displayName").GetString();
                    existingGroup.IsActive = group.GetProperty("isActive").GetBoolean();
                    existingGroup.UpdatedAt = DateTime.UtcNow;

                    // Create TeamSchedulingGroup relationship
                    var teamSchedulingGroup = await context.TeamSchedulingGroups
                        .FirstOrDefaultAsync(tsg => tsg.TeamId == team.Id && tsg.SchedulingGroupId == existingGroup.Id);

                    if (teamSchedulingGroup == null)
                    {
                        teamSchedulingGroup = new TeamSchedulingGroup
                        {
                            TeamId = team.Id,
                            SchedulingGroupId = existingGroup.Id,
                            Team = team,
                            SchedulingGroup = existingGroup
                        };
                        context.TeamSchedulingGroups.Add(teamSchedulingGroup);
                        _logger.LogInformation($"Created new team-scheduling group relationship for team {team.DisplayName}");
                    }

                    // Save changes for each team's scheduling groups
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task SyncScheduleShiftsAsync(ApplicationDbContext context)
        {
            var teams = await context.Teams.ToListAsync();
            var agents = await context.Agents.ToListAsync();
            var schedulingGroups = await context.SchedulingGroups.ToListAsync();

            foreach (var team in teams)
            {
                if (string.IsNullOrEmpty(team.OwnerId))
                {
                    _logger.LogInformation($"Skipping shifts sync for team {team.DisplayName} as it has no owner");
                    continue;
                }

                var shiftsResponse = await _graphService.GetTeamShiftsAsync(team.MicrosoftTeamId, team.OwnerId);
                var shifts = shiftsResponse.GetProperty("value").EnumerateArray();

                _logger.LogInformation($"Processing shifts for team {team.DisplayName} with owner {team.OwnerId}");

                foreach (var shift in shifts)
                {
                    var microsoftShiftId = shift.GetProperty("id").GetString();
                    var existingShift = await context.ScheduleShifts
                        .FirstOrDefaultAsync(ss => ss.MicrosoftShiftId == microsoftShiftId);

                    if (existingShift == null)
                    {
                        existingShift = new ScheduleShift
                        {
                            MicrosoftShiftId = microsoftShiftId,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.ScheduleShifts.Add(existingShift);
                        _logger.LogInformation($"Created new shift with ID {microsoftShiftId}");
                    }

                    var userId = shift.GetProperty("userId").GetString();
                    var agent = agents.FirstOrDefault(a => a.MicrosoftUserId == userId);
                    if (agent != null)
                    {
                        existingShift.AgentId = agent.Id;
                    }
                    else
                    {
                        _logger.LogWarning($"Agent not found for userId {userId}");
                    }

                    var schedulingGroupId = shift.GetProperty("schedulingGroupId").GetString();
                    var group = schedulingGroups.FirstOrDefault(sg => sg.MicrosoftGroupId == schedulingGroupId);
                    if (group != null)
                    {
                        existingShift.SchedulingGroupId = group.Id;
                    }
                    else
                    {
                        _logger.LogWarning($"Scheduling group not found for groupId {schedulingGroupId}");
                    }

                    var sharedShift = shift.GetProperty("sharedShift");
                    if (sharedShift.ValueKind != JsonValueKind.Null)
                    {
                        existingShift.StartDateTime = DateTime.Parse(sharedShift.GetProperty("startDateTime").GetString());
                        existingShift.EndDateTime = DateTime.Parse(sharedShift.GetProperty("endDateTime").GetString());
                        existingShift.Theme = sharedShift.GetProperty("theme").GetString();
                        existingShift.Notes = sharedShift.GetProperty("notes").GetString();
                    }
                    existingShift.UpdatedAt = DateTime.UtcNow;

                    // Save changes after each shift to ensure proper tracking
                    await context.SaveChangesAsync();
                }

                _logger.LogInformation($"Saved shifts for team {team.DisplayName}");
            }
        }

        private async Task SyncRelationshipsAsync(ApplicationDbContext context)
        {
            // Sync Queue-Team relationships
            var teams = await context.Teams.ToListAsync();
            var queues = await context.Queues.ToListAsync();

            foreach (var team in teams)
            {
                foreach (var queue in queues)
                {
                    var queueTeam = await context.QueueTeams
                        .FirstOrDefaultAsync(qt => qt.QueueId == queue.Id && qt.TeamId == team.Id);

                    if (queueTeam == null)
                    {
                        context.QueueTeams.Add(new QueueTeam
                        {
                            QueueId = queue.Id,
                            TeamId = team.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}