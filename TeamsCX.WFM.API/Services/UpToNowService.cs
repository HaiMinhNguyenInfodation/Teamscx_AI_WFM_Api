using System;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using TeamsCX.WFM.API.Models.GraphQL;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Models.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;

namespace TeamsCX.WFM.API.Services
{
    public class UpToNowService : IUpToNowService
    {
        private readonly HttpClient _httpClient;
        private readonly IAgentStatusService _agentStatusService;
        private readonly ILogger<UpToNowService> _logger;
        private readonly ApplicationDbContext _context;
        private const string GRAPHQL_API = "https://tcx-teamsv2-demo-datasource.azurewebsites.net/api/graphql";

        public UpToNowService(
            HttpClient httpClient,
            IAgentStatusService agentStatusService,
            ILogger<UpToNowService> logger,
            ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _agentStatusService = agentStatusService;
            _logger = logger;
            _context = context;
        }

        public async Task<Models.UpToNowResponse> GetUpToNowDataAsync(List<string> queueIds)
        {
            var startTime = DateTime.UtcNow.Date;
            var endTime = DateTime.UtcNow;

            // Convert queue IDs to display names
            var callQueues = await _context.Queues
                .Where(q => queueIds.Contains(q.MicrosoftQueueId))
                .Select(q => q.Name)
                .ToListAsync();

            if (!callQueues.Any())
            {
                throw new Exception("No valid call queues found for the provided IDs");
            }

            // Format call queue display names for the query
            var formattedCallQueues = string.Join(",", callQueues);

            // Get call details from the GraphQL API
            var query = new
            {
                query = $@"query {{
                    callDetails(
                        from: ""{startTime:yyyy-MM-ddTHH:mm:ss.fffZ}""
                        to: ""{endTime:yyyy-MM-ddTHH:mm:ss.fffZ}""
                        resourceAccounts: ""{formattedCallQueues}""
                    ) {{
                        id
                        direction
                        statusEnd
                        waitingDuration
                        answerDuration
                        callDuration
                        callQueues
                        resourceAccounts
                    }}
                }}",
                variables = new { }
            };

            var response = await _httpClient.PostAsJsonAsync(GRAPHQL_API, query);
            _logger.LogDebug($"Response: {{@response}}", response);
            var data = await response.Content.ReadFromJsonAsync<GraphQLResponse<CallDetailsData>>();

            if (data?.Data?.CallDetails == null)
            {
                _logger.LogError("Failed to get call details from the GraphQL API");
            }

            var calls = data?.Data?.CallDetails ?? new List<CallDetail>();
            var inboundCalls = calls?.Where(c => c.Direction == "Inbound").ToList();

            // Get agent status history and schedules
            var agentStatusHistory = await _agentStatusService.GetAgentStatusHistoryAsync(callQueues, startTime, endTime);
            var agentSchedules = await _agentStatusService.GetAgentSchedulesAsync(callQueues, startTime.Date);

            // Calculate metrics
            var avgWaitTime = CalculateAverageWaitTime(inboundCalls);
            var avgTalkTime = CalculateAverageTalkTime(inboundCalls);
            var adherence = CalculateAdherence(agentStatusHistory, agentSchedules);
            var conformance = CalculateConformance(agentStatusHistory, agentSchedules);
            var missedCalls = CalculateMissedCalls(inboundCalls);
            var answeredCallsPerAgent = CalculateAnsweredCallsPerAgent(inboundCalls, agentSchedules);
            var earlyLogOut = CountEarlyLogOut(agentStatusHistory, agentSchedules);
            var lateLogIn = CountLateLogIn(agentStatusHistory, agentSchedules);
            var agentStatuses = await GetAgentStatusesAsync(callQueues, agentStatusHistory, agentSchedules);

            return new Models.UpToNowResponse
            {
                AverageWaitTime = avgWaitTime,
                AverageTalkTime = avgTalkTime,
                Adherence = adherence,
                Conformance = conformance,
                MissedCalls = missedCalls,
                AnsweredCallsPerAgent = answeredCallsPerAgent,
                EarlyLogOut = earlyLogOut,
                LateLogIn = lateLogIn,
                AgentStatuses = agentStatuses
            };
        }

        private TimeSpan CalculateAverageWaitTime(List<CallDetail> inboundCalls)
        {
            var callsWithWaitTime = inboundCalls.Where(c => c.WaitingDuration > 0).ToList();
            if (!callsWithWaitTime.Any()) return TimeSpan.Zero;

            var totalWaitTime = callsWithWaitTime.Sum(c => c.WaitingDuration);
            return TimeSpan.FromSeconds(totalWaitTime / callsWithWaitTime.Count);
        }

        private TimeSpan CalculateAverageTalkTime(List<CallDetail> inboundCalls)
        {
            var callsWithTalkTime = inboundCalls.Where(c => c.AnswerDuration > 0).ToList();
            if (!callsWithTalkTime.Any()) return TimeSpan.Zero;

            var totalTalkTime = callsWithTalkTime.Sum(c => c.AnswerDuration);
            return TimeSpan.FromSeconds(totalTalkTime / callsWithTalkTime.Count);
        }

        private double CalculateAdherence(List<AgentStatusHistoryDTO> statusHistory, List<AgentScheduleDTO> schedules)
        {
            if (!schedules.Any()) return 0;

            var totalAdherenceTime = statusHistory
                .Where(s => s.Status == AgentStatus.Available || s.Status == AgentStatus.InACall)
                .Sum(s => (s.EndTime - s.StartTime).TotalMinutes);

            var totalScheduledTime = schedules
                .Sum(s => (s.EndTime - s.StartTime).TotalMinutes);

            return totalScheduledTime > 0 ? (totalAdherenceTime / totalScheduledTime) * 100 : 0;
        }

        private double CalculateConformance(List<AgentStatusHistoryDTO> statusHistory, List<AgentScheduleDTO> schedules)
        {
            if (!schedules.Any()) return 0;

            var totalWorkingTime = statusHistory
                .Where(s => s.Status != AgentStatus.Offline)
                .Sum(s => (s.EndTime - s.StartTime).TotalMinutes);

            var totalScheduledTime = schedules
                .Sum(s => (s.EndTime - s.StartTime).TotalMinutes);

            return totalScheduledTime > 0 ? (totalWorkingTime / totalScheduledTime) * 100 : 0;
        }

        private string CalculateMissedCalls(List<CallDetail> inboundCalls)
        {
            var totalInbound = inboundCalls.Count;
            var missedCalls = inboundCalls.Count(c => c.StatusEnd == "Missed");
            return $"{missedCalls}/{totalInbound}";
        }

        private double CalculateAnsweredCallsPerAgent(List<CallDetail> inboundCalls, List<AgentScheduleDTO> schedules)
        {
            var answeredCalls = inboundCalls.Count(c => c.StatusEnd == "Answered");
            var agentCount = schedules.Select(s => s.AgentId).Distinct().Count();
            return agentCount > 0 ? answeredCalls / (double)agentCount : 0;
        }

        private int CountEarlyLogOut(List<AgentStatusHistoryDTO> statusHistory, List<AgentScheduleDTO> schedules)
        {
            return schedules.Count(schedule =>
            {
                var lastStatus = statusHistory
                    .Where(s => s.AgentId == schedule.AgentId)
                    .OrderByDescending(s => s.EndTime)
                    .FirstOrDefault();

                return lastStatus != null &&
                       lastStatus.Status == AgentStatus.Offline &&
                       lastStatus.EndTime < schedule.EndTime;
            });
        }

        private int CountLateLogIn(List<AgentStatusHistoryDTO> statusHistory, List<AgentScheduleDTO> schedules)
        {
            return schedules.Count(schedule =>
            {
                var firstStatus = statusHistory
                    .Where(s => s.AgentId == schedule.AgentId)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault();

                return firstStatus != null &&
                       firstStatus.StartTime > schedule.StartTime;
            });
        }

        private async Task<List<Models.AgentStatusRow>> GetAgentStatusesAsync(
            List<string> callQueues,
            List<AgentStatusHistoryDTO> statusHistory,
            List<AgentScheduleDTO> schedules)
        {
            var result = new List<Models.AgentStatusRow>();

            foreach (var schedule in schedules)
            {
                var agentHistory = statusHistory
                    .Where(s => s.AgentId == schedule.AgentId)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                var hourlyStatuses = new List<Models.HourlyStatus>();
                var currentHour = schedule.StartTime;

                while (currentHour < schedule.EndTime)
                {
                    var nextHour = currentHour.AddHours(1);
                    var hourHistory = agentHistory
                        .Where(s => s.StartTime < nextHour && s.EndTime > currentHour)
                        .ToList();

                    var statusTimes = new List<Models.StatusDuration>();
                    foreach (var status in hourHistory)
                    {
                        var statusStart = status.StartTime > currentHour ? status.StartTime : currentHour;
                        var statusEnd = status.EndTime < nextHour ? status.EndTime : nextHour;
                        var duration = statusEnd - statusStart;

                        statusTimes.Add(CreateStatusDuration(status.Status, duration));
                    }

                    hourlyStatuses.Add(new Models.HourlyStatus
                    {
                        Hour = currentHour,
                        StatusTimes = statusTimes
                    });

                    currentHour = nextHour;
                }

                var idleTime = TimeSpan.FromMinutes(agentHistory
                    .Where(s => s.Status == AgentStatus.Available)
                    .Sum(s => (s.EndTime - s.StartTime).TotalMinutes));

                var adherence = CalculateAdherence(
                    agentHistory,
                    new List<AgentScheduleDTO> { schedule });

                result.Add(new Models.AgentStatusRow
                {
                    AgentName = schedule.AgentName,
                    Scheduled = new Models.ScheduleInfo
                    {
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime
                    },
                    IdleTime = idleTime,
                    Adherence = adherence,
                    Actual = hourlyStatuses
                });
            }

            return result;
        }

        private Models.StatusDuration CreateStatusDuration(AgentStatus status, TimeSpan duration)
        {
            return new Models.StatusDuration
            {
                Status = status,
                StatusDescription = status.GetDescription(),
                Duration = duration
            };
        }
    }
}