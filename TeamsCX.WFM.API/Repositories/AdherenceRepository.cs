using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Models.DTOs;

namespace TeamsCX.WFM.API.Repositories
{
    public class AdherenceRepository : IAdherenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdherenceRepository> _logger;

        public AdherenceRepository(ApplicationDbContext context, ILogger<AdherenceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Models.DTOs.AdherenceResponse> GetDashboardSummaryAsync(string[] queueMicrosoftIds = null)
        {
            try
            {
                var agentStatusDistribution = await GetAgentStatusDistributionAsync(queueMicrosoftIds);
                var agentsSummary = await GetAgentsSummaryAsync(queueMicrosoftIds);

                return new Models.DTOs.AdherenceResponse
                {
                    AgentStatusDistribution = agentStatusDistribution,
                    AgentsSummary = agentsSummary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                throw;
            }
        }

        public async Task<Models.DTOs.AgentStatusDistribution> GetAgentStatusDistributionAsync(string[] queueMicrosoftIds = null)
        {
            try
            {
                var currentTime = DateTime.UtcNow;

                // First get scheduling groups that have active shifts
                var activeSchedulingGroups = await _context.ScheduleShifts
                    .Where(ss => ss.StartDateTime <= currentTime && ss.EndDateTime >= currentTime)
                    .Select(ss => ss.SchedulingGroupId)
                    .Distinct()
                    .ToListAsync();

                if (!activeSchedulingGroups.Any())
                {
                    return new Models.DTOs.AgentStatusDistribution
                    {
                        TotalAgents = 0,
                        StatusData = new Models.DTOs.StatusData()
                    };
                }

                // Get queues that belong to these scheduling groups
                var queuesWithActiveShifts = await _context.Queues
                    .Where(q => queueMicrosoftIds.Contains(q.MicrosoftQueueId))
                    .Where(q => _context.TeamSchedulingGroups
                        .Any(tsg => activeSchedulingGroups.Contains(tsg.SchedulingGroupId) &&
                                   _context.QueueTeams
                                       .Any(qt => qt.QueueId == q.Id && qt.TeamId == tsg.TeamId)))
                    .Select(q => q.Id)
                    .ToListAsync();

                if (!queuesWithActiveShifts.Any())
                {
                    return new Models.DTOs.AgentStatusDistribution
                    {
                        TotalAgents = 0,
                        StatusData = new Models.DTOs.StatusData()
                    };
                }

                // First get all reported agents (filtered by queue if provided)
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
                        Status = g.OrderByDescending(h => h.CreatedAt).First().Status
                    })
                    .ToListAsync();

                var statusCounts = latestStatuses
                    .GroupBy(s => s.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                var statusData = new Models.DTOs.StatusData();
                foreach (var statusCount in statusCounts)
                {
                    switch (statusCount.Status)
                    {
                        case AgentStatus.Available:
                            statusData.Available = statusCount.Count;
                            break;
                        case AgentStatus.Busy:
                            statusData.Busy = statusCount.Count;
                            break;
                        case AgentStatus.DoNotDisturb:
                            statusData.DoNotDisturb = statusCount.Count;
                            break;
                        case AgentStatus.Away:
                            statusData.Away = statusCount.Count;
                            break;
                        case AgentStatus.Offline:
                            statusData.Offline = statusCount.Count;
                            break;
                        case AgentStatus.InACall:
                            statusData.InACall = statusCount.Count;
                            break;
                        case AgentStatus.Presenting:
                            statusData.Presenting = statusCount.Count;
                            break;
                        case AgentStatus.Inactive:
                            statusData.Inactive = statusCount.Count;
                            break;
                        case AgentStatus.BeRightBack:
                            statusData.BeRightBack = statusCount.Count;
                            break;
                    }
                }

                return new Models.DTOs.AgentStatusDistribution
                {
                    TotalAgents = totalAgents,
                    StatusData = statusData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent status distribution");
                throw;
            }
        }

        public async Task<Models.DTOs.AgentsSummary> GetAgentsSummaryAsync(string[] queueMicrosoftIds = null)
        {
            try
            {
                var currentTime = DateTime.UtcNow;

                // First get scheduling groups that have active shifts
                var activeSchedulingGroups = await _context.ScheduleShifts
                    .Where(ss => ss.StartDateTime <= currentTime && ss.EndDateTime >= currentTime)
                    .Select(ss => ss.SchedulingGroupId)
                    .Distinct()
                    .ToListAsync();

                if (!activeSchedulingGroups.Any())
                {
                    return new Models.DTOs.AgentsSummary
                    {
                        TotalAgents = 0,
                        AgentsLoggedIn = 0,
                        AgentsIdlePercentage = 0,
                        AdherencePercentage = 0,
                        ConformancePercentage = 0,
                        EarlyLogOut = 0,
                        LateLogIn = 0
                    };
                }

                // Get queues that belong to these scheduling groups
                var queuesWithActiveShifts = await _context.Queues
                    .Where(q => queueMicrosoftIds.Contains(q.MicrosoftQueueId))
                    .Where(q => _context.TeamSchedulingGroups
                        .Any(tsg => activeSchedulingGroups.Contains(tsg.SchedulingGroupId) &&
                                   _context.QueueTeams
                                       .Any(qt => qt.QueueId == q.Id && qt.TeamId == tsg.TeamId)))
                    .Select(q => q.Id)
                    .ToListAsync();

                if (!queuesWithActiveShifts.Any())
                {
                    return new Models.DTOs.AgentsSummary
                    {
                        TotalAgents = 0,
                        AgentsLoggedIn = 0,
                        AgentsIdlePercentage = 0,
                        AdherencePercentage = 0,
                        ConformancePercentage = 0,
                        EarlyLogOut = 0,
                        LateLogIn = 0
                    };
                }

                // First get all reported agents (filtered by queue if provided)
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
                        Status = g.OrderByDescending(h => h.CreatedAt).First().Status
                    })
                    .ToListAsync();

                var loggedInAgents = latestStatuses.Count(s => s.Status != AgentStatus.Offline);
                var idleAgents = latestStatuses.Count(s => s.Status == AgentStatus.Available);

                // Calculate adherence and conformance based on scheduled vs actual status
                var scheduledShifts = await _context.ScheduleShifts
                    .Where(s => s.StartDateTime <= currentTime &&
                               s.EndDateTime >= currentTime &&
                               reportedAgentsQuery.Select(ra => ra.Id).Contains(s.AgentId))
                    .CountAsync();

                var activeAgentsCount = latestStatuses.Count(s => s.Status == AgentStatus.Available || s.Status == AgentStatus.InACall);

                var adherencePercentage = scheduledShifts > 0 ? (double)activeAgentsCount / scheduledShifts * 100 : 0;
                var conformancePercentage = totalAgents > 0 ? (double)loggedInAgents / totalAgents * 100 : 0;

                // Calculate early logouts and late logins (within last hour)
                var earlyLogouts = await _context.AgentStatusHistories
                    .Where(a => a.Status == AgentStatus.Offline &&
                               a.CreatedAt >= currentTime.AddHours(-1) &&
                               reportedAgentsQuery.Select(ra => ra.Id).Contains(a.AgentId))
                    .Select(a => a.AgentId)
                    .Distinct()
                    .CountAsync();

                var lateLogins = await _context.AgentStatusHistories
                    .Where(a => a.Status != AgentStatus.Offline &&
                               a.CreatedAt >= currentTime.AddHours(-1) &&
                               reportedAgentsQuery.Select(ra => ra.Id).Contains(a.AgentId))
                    .Select(a => a.AgentId)
                    .Distinct()
                    .CountAsync();

                return new Models.DTOs.AgentsSummary
                {
                    TotalAgents = totalAgents,
                    AgentsLoggedIn = loggedInAgents,
                    AgentsIdlePercentage = totalAgents > 0 ? (double)idleAgents / totalAgents * 100 : 0,
                    AdherencePercentage = adherencePercentage,
                    ConformancePercentage = conformancePercentage,
                    EarlyLogOut = earlyLogouts,
                    LateLogIn = lateLogins
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agents summary");
                throw;
            }
        }

        public async Task<QueueMetricsResponse> GetQueueMetricsAsync(string[] queueMicrosoftIds = null)
        {
            try
            {
                var query = _context.Queues.AsQueryable();

                if (queueMicrosoftIds != null && queueMicrosoftIds.Length > 0)
                {
                    query = query.Where(q => queueMicrosoftIds.Contains(q.MicrosoftQueueId));
                }

                var queues = await query.ToListAsync();
                var queueMetrics = new List<QueueMetrics>();

                foreach (var queue in queues)
                {
                    var calls = await _context.Calls
                        .Where(c => c.StartedAt.Date == DateTime.UtcNow.Date)
                        .Where(c => _context.CallQueues.Any(cq => cq.CallId == c.Id && cq.QueueId == queue.Id))
                        .Join(_context.CallStatuses,
                            c => c.Id,
                            cs => cs.Id,
                            (c, cs) => new { Call = c, Status = cs })
                        .Join(_context.CallOutcomes,
                            c => c.Call.Id,
                            co => co.Id,
                            (c, co) => new { c.Call, c.Status, Outcome = co })
                        .ToListAsync();

                    var waitingCalls = calls.Count(c => c.Status.Status == "Waiting");
                    var connectingCalls = calls.Count(c => c.Status.Status == "Connected");
                    var missedCalls = calls.Count(c => c.Outcome.Outcome == "Missed");
                    var answeredCalls = calls.Count(c => c.Outcome.Outcome == "Answered");

                    var waitingTimes = calls
                        .Where(c => c.Call.WaitingDuration > 0)
                        .Select(c => c.Call.WaitingDuration)
                        .ToList();
                    var averageWaitingTime = waitingTimes.Any()
                        ? TimeSpan.FromSeconds(waitingTimes.Average())
                        : TimeSpan.Zero;

                    var handleTimes = calls
                        .Where(c => c.Outcome.Outcome == "Answered" && c.Call.WaitingDuration <= 60)
                        .Select(c => c.Call.ConnectedDuration)
                        .ToList();
                    var averageHandleTime = handleTimes.Any()
                        ? TimeSpan.FromSeconds(handleTimes.Average())
                        : TimeSpan.Zero;

                    var totalCalls = calls.Count();
                    var slaCalls = calls.Count(c => c.Outcome.Outcome == "Answered");
                    var slaPercentage = totalCalls > 0 ? (double)slaCalls / totalCalls * 100 : 0;

                    queueMetrics.Add(new QueueMetrics
                    {
                        Queue = queue.Name,
                        WaitingCalls = waitingCalls,
                        ConnectingCalls = connectingCalls,
                        MissedCalls = missedCalls,
                        AnsweredCalls = answeredCalls,
                        AverageWaitingTime = averageWaitingTime,
                        AverageHandleTime = averageHandleTime,
                        SLAPercentage = Math.Round(slaPercentage, 2)
                    });
                }

                return new QueueMetricsResponse
                {
                    Queues = queueMetrics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue metrics");
                throw;
            }
        }
    }
}