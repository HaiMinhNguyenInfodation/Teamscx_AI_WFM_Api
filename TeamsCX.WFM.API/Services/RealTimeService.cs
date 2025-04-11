using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Models.RealTime;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Data;
using Microsoft.Extensions.Logging;

namespace TeamsCX.WFM.API.Services
{
    public interface IRealTimeService
    {
        Task<RealTimeOverview> GetRealTimeOverview(List<string> callQueues);
    }

    public class RealTimeService : IRealTimeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RealTimeService> _logger;

        public RealTimeService(ApplicationDbContext context, ILogger<RealTimeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RealTimeOverview> GetRealTimeOverview(List<string> callQueues)
        {
            try
            {
                var overview = new RealTimeOverview
                {
                    AgentStatus = await GetAgentStatusSummary(callQueues),
                    AgentMetrics = await CalculateAgentMetrics(callQueues),
                    AgentDetails = await GetAgentDetails(callQueues)
                };

                return overview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting real-time overview");
                throw;
            }
        }

        private async Task<AgentStatusSummary> GetAgentStatusSummary(List<string> callQueues)
        {
            // Get queues and their IDs
            _logger.LogDebug("callQueues: {callQueues}", callQueues);
            var queues = await _context.Queues
                .Where(q => callQueues.Contains(q.MicrosoftQueueId))
                .ToListAsync();
            var queueIds = queues.Select(q => q.Id).ToList();
            _logger.LogDebug("queueIds: {queueIds}", queueIds);

            // Get all reported agents that belong to these queues through QueueReportedAgent
            var agentIds = await _context.QueueReportedAgents
                .Where(qra => queueIds.Contains(qra.QueueId) && qra.IsActive)
                .Select(qra => qra.AgentId)
                .Distinct()
                .ToListAsync();

            // Get latest status for each agent
            var latestStatuses = await _context.AgentStatusHistories
                .Where(h => agentIds.Contains(h.AgentId))
                .GroupBy(h => h.AgentId)
                .Select(g => new
                {
                    AgentId = g.Key,
                    Status = g.OrderByDescending(h => h.CreatedAt).First().Status
                })
                .ToListAsync();
            var statusDistribution = latestStatuses
                .GroupBy(s => s.Status)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Count()
                );
            _logger.LogDebug("statusDistribution: {statusDistribution}", statusDistribution);
            return new AgentStatusSummary
            {
                TotalAgents = agentIds.Count,
                StatusDistribution = statusDistribution
            };
        }

        private async Task<AgentMetrics> CalculateAgentMetrics(List<string> callQueues)
        {
            // Get queues and their IDs
            var queues = await _context.Queues
                .Where(q => callQueues.Contains(q.MicrosoftQueueId))
                .ToListAsync();
            var queueIds = queues.Select(q => q.Id).ToList();

            // Get all reported agents that belong to these queues through QueueReportedAgent
            var agentIds = await _context.QueueReportedAgents
                .Where(qra => queueIds.Contains(qra.QueueId) && qra.IsActive)
                .Select(qra => qra.AgentId)
                .Distinct()
                .ToListAsync();

            // Get active agents (logged in)
            var activeAgents = await _context.AgentActiveHistories
                .Where(h => h.AgentId.HasValue && agentIds.Contains(h.AgentId.Value))
                .GroupBy(h => h.AgentId.Value)
                .Select(g => new
                {
                    AgentId = g.Key,
                    IsActived = g.OrderByDescending(h => h.CreatedAt).First().IsActived
                })
                .Where(h => h.IsActived)
                .ToListAsync();

            // Get latest status for calculating idle metrics
            var latestStatuses = await _context.AgentStatusHistories
                .Where(h => agentIds.Contains(h.AgentId))
                .GroupBy(h => h.AgentId)
                .Select(g => new
                {
                    AgentId = g.Key,
                    Status = g.OrderByDescending(h => h.CreatedAt).First().Status
                })
                .ToListAsync();

            var availableAgents = latestStatuses.Count(s => s.Status == AgentStatus.Available);
            var inCallAgents = latestStatuses.Count(s => s.Status == AgentStatus.InACall);

            double idleRate = (inCallAgents + availableAgents) == 0 ? 0 :
                (double)availableAgents / (inCallAgents + availableAgents) * 100;

            return new AgentMetrics
            {
                AgentLoggedInRatio = $"{activeAgents.Count}/{agentIds.Count}",
                AgentIdleRatio = $"{availableAgents}/{inCallAgents + availableAgents}",
                AgentIdleRate = Math.Round(idleRate, 2)
            };
        }

        private async Task<List<AgentRealTimeStatus>> GetAgentDetails(List<string> callQueues)
        {
            var currentTime = DateTime.UtcNow;

            // Get queues first
            var queues = await _context.Queues
                .Where(q => callQueues.Contains(q.MicrosoftQueueId))
                .ToListAsync();

            // Get teams associated with these queues
            var queueTeams = await _context.QueueTeams
                .Where(qt => queues.Select(q => q.Id).Contains(qt.QueueId))
                .Select(qt => qt.TeamId)
                .Distinct()
                .ToListAsync();

            // Get scheduling groups for these teams
            var schedulingGroups = await _context.TeamSchedulingGroups
                .Where(tsg => queueTeams.Contains(tsg.TeamId))
                .Select(tsg => tsg.SchedulingGroupId)
                .ToListAsync();
            _logger.LogDebug("schedulingGroups: {schedulingGroups}", schedulingGroups);

            // Get reported agents with scheduled shifts in the scheduling groups
            var agentsWithShifts = await _context.ScheduleShifts
                .Include(s => s.Agent)
                .Where(s => schedulingGroups.Contains(s.SchedulingGroupId)
                    && s.StartDateTime <= currentTime
                    && s.EndDateTime > currentTime
                    && s.Agent.IsReported)
                .Select(s => new
                {
                    Agent = s.Agent,
                    Shift = s
                })
                .ToListAsync();
            _logger.LogDebug("agentsWithShifts: {agentsWithShifts}", agentsWithShifts);

            var result = new List<AgentRealTimeStatus>();

            foreach (var item in agentsWithShifts)
            {
                // Get agent's latest status
                var latestStatus = await _context.AgentStatusHistories
                    .Where(h => h.AgentId == item.Agent.Id)
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestStatus != null)
                {
                    // Find the queue this agent is currently active in
                    var activeQueue = await _context.AgentActiveHistories
                        .Where(h => h.AgentId == item.Agent.Id && h.IsActived)
                        .Join(
                            _context.Queues,
                            h => h.QueueId,
                            q => q.Id,
                            (h, q) => new { Queue = q, History = h })
                        .OrderByDescending(x => x.History.CreatedAt)
                        .FirstOrDefaultAsync();

                    // Find the scheduled queue through the scheduling group
                    var scheduledQueue = await _context.SchedulingGroups
                        .Where(sg => sg.Id == item.Shift.SchedulingGroupId)
                        .Join(
                            _context.TeamSchedulingGroups,
                            sg => sg.Id,
                            tsg => tsg.SchedulingGroupId,
                            (sg, tsg) => tsg)
                        .Join(
                            _context.QueueTeams,
                            tsg => tsg.TeamId,
                            qt => qt.TeamId,
                            (tsg, qt) => qt)
                        .Join(
                            _context.Queues,
                            qt => qt.QueueId,
                            q => q.Id,
                            (qt, q) => q)
                        .Where(q => callQueues.Contains(q.MicrosoftQueueId))
                        .FirstOrDefaultAsync();

                    if (scheduledQueue != null)
                    {
                        result.Add(new AgentRealTimeStatus
                        {
                            AgentDisplayName = item.Agent.DisplayName,
                            ScheduledCallQueue = scheduledQueue.Name,
                            Adherence = IsAdherent(latestStatus.Status, activeQueue?.Queue.Id == scheduledQueue.Id),
                            StatusTime = latestStatus.CreatedAt,
                            Status = latestStatus.Status.ToString(),
                            ScheduledTime = item.Shift.StartDateTime,
                            ActiveInCallQueue = activeQueue?.Queue.Name
                        });
                    }
                }
            }

            return result;
        }

        private bool IsAdherent(AgentStatus status, bool isActiveInQueue)
        {
            return (status == AgentStatus.Available || status == AgentStatus.InACall) && isActiveInQueue;
        }
    }
}