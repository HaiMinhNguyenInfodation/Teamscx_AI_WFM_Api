using System;
using System.Collections.Generic;
using System.Linq;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Models.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;

namespace TeamsCX.WFM.API.Services
{
    public class UpToNowService : IUpToNowService
    {
        private readonly IAgentStatusService _agentStatusService;
        private readonly ILogger<UpToNowService> _logger;
        private readonly ApplicationDbContext _context;

        public UpToNowService(
            IAgentStatusService agentStatusService,
            ILogger<UpToNowService> logger,
            ApplicationDbContext context)
        {
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

            // Get agent status history and schedules
            var agentStatusHistory = await _agentStatusService.GetAgentStatusHistoryAsync(callQueues, startTime, endTime);
            var agentSchedules = await _agentStatusService.GetAgentSchedulesAsync(callQueues, startTime.Date);

            return new Models.UpToNowResponse
            {
                AgentStatusHistory = agentStatusHistory,
                AgentSchedules = agentSchedules
            };
        }
    }
}