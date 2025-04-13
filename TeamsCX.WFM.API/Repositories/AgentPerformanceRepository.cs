using System.Collections.Generic;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;

namespace TeamsCX.WFM.API.Repositories
{
    public class AgentPerformanceRepository : IAgentPerformanceRepository
    {
        private readonly ApplicationDbContext _context;

        public AgentPerformanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AgentPerformanceResponseDTO> GetAgentPerformanceAsync(List<string> queueMicrosoftId)
        {
            var response = new AgentPerformanceResponseDTO
            {
                Agents = new List<AgentPerformanceDTO>()
            };

            var currentTime = DateTime.UtcNow;

            // First get scheduling groups that have active shifts
            var activeSchedulingGroups = await _context.ScheduleShifts
                .Where(ss => ss.StartDateTime <= currentTime && ss.EndDateTime >= currentTime)
                .Select(ss => ss.SchedulingGroupId)
                .Distinct()
                .ToListAsync();

            if (!activeSchedulingGroups.Any())
            {
                return response; // Return empty response if no active shifts
            }

            // Get queues that belong to these scheduling groups
            var queuesWithActiveShifts = await _context.Queues
                .Where(q => queueMicrosoftId.Contains(q.MicrosoftQueueId))
                .Where(q => _context.TeamSchedulingGroups
                    .Any(tsg => activeSchedulingGroups.Contains(tsg.SchedulingGroupId) &&
                               _context.QueueTeams
                                   .Any(qt => qt.QueueId == q.Id && qt.TeamId == tsg.TeamId)))
                .Select(q => q.Id)
                .ToListAsync();

            if (!queuesWithActiveShifts.Any())
            {
                return response; // Return empty response if no queues have active shifts
            }

            var reportedAgentsQuery = _context.Agents
                    .Where(a => a.IsReported)
                    .Where(a => _context.QueueReportedAgents
                        .Any(qra => qra.AgentId == a.Id &&
                                   qra.IsActive &&
                                   queuesWithActiveShifts.Contains(qra.QueueId)));

            var totalAgents = await reportedAgentsQuery.CountAsync();

            // Get latest status for each agent
            var latestStatuses = await _context.AgentStatusHistories
                .Where(a => reportedAgentsQuery.Select(ra => ra.Id).Contains(a.AgentId))
                .GroupBy(a => a.AgentId)
                .Select(g => new
                {
                    AgentId = g.Key,
                    Status = g.OrderByDescending(h => h.CreatedAt).First().Status,
                    StartedTime = g.OrderByDescending(h => h.CreatedAt).First().CreatedAt
                })
                .ToListAsync();

            // Get active queues for each agent
            var activeQueues = await _context.QueueReportedAgents
                .Include(qra => qra.Queue)
                .Where(qra => qra.IsActive &&
                            reportedAgentsQuery.Select(a => a.Id).Contains(qra.AgentId))
                .Select(qra => new
                {
                    qra.AgentId,
                    QueueName = qra.Queue.Name
                })
                .ToListAsync();

            // Get current schedule for each agent
            var currentSchedules = await _context.ScheduleShifts
                .Include(ss => ss.SchedulingGroup)
                .Where(ss => reportedAgentsQuery.Select(a => a.Id).Contains(ss.AgentId) &&
                            ss.StartDateTime <= currentTime &&
                            ss.EndDateTime >= currentTime)
                .Select(ss => new
                {
                    ss.AgentId,
                    GroupName = ss.SchedulingGroup.DisplayName
                })
                .ToListAsync();

            // Get agent details
            var agents = await reportedAgentsQuery.ToListAsync();

            // Build response
            foreach (var agent in agents)
            {
                var latestStatus = latestStatuses.FirstOrDefault(s => s.AgentId == agent.Id);
                var agentQueues = activeQueues.Where(aq => aq.AgentId == agent.Id)
                                            .Select(aq => aq.QueueName)
                                            .ToList();
                var schedule = currentSchedules.FirstOrDefault(s => s.AgentId == agent.Id);

                response.Agents.Add(new AgentPerformanceDTO
                {
                    AgentName = agent.DisplayName,
                    CurrentStatus = latestStatus?.Status.ToString() ?? "Unknown",
                    StartedTime = latestStatus?.StartedTime ?? DateTime.MinValue,
                    ActiveCQ = agentQueues,
                    ScheduledCQ = schedule?.GroupName ?? "Not Scheduled"
                });
            }

            return response;
        }
    }
}