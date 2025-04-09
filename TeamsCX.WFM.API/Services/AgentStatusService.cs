using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Models.DTOs;
using TeamsCX.WFM.API.Data;

namespace TeamsCX.WFM.API.Services
{
    public class AgentStatusService : IAgentStatusService
    {
        private readonly ApplicationDbContext _context;

        public AgentStatusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AgentStatusHistoryDTO>> GetAgentStatusHistoryAsync(List<string> callQueues, DateTime startTime, DateTime endTime)
        {
            var statusHistory = await _context.AgentStatusHistories
                .Where(h => h.CreatedAt >= startTime && h.CreatedAt <= endTime)
                .Include(h => h.Agent)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            return statusHistory.Select(h => new AgentStatusHistoryDTO
            {
                Id = h.Id.ToString(),
                AgentId = h.AgentId,
                AgentName = h.Agent.DisplayName,
                StartTime = h.CreatedAt,
                EndTime = h.CreatedAt.AddMinutes(5),
                Status = h.Status,
                CallQueueId = string.Join(",", callQueues)
            }).ToList();
        }

        public async Task<List<AgentScheduleDTO>> GetAgentSchedulesAsync(List<string> callQueues, DateTime date)
        {
            var activeAgents = await _context.AgentActiveHistories
                .Where(h => h.IsActived)
                .Include(h => h.Agent)
                .ToListAsync();

            return activeAgents.Select(a => new AgentScheduleDTO
            {
                Id = a.Id.ToString(),
                AgentId = a.AgentId.Value,
                AgentName = a.Agent.DisplayName,
                StartTime = date.Date,
                EndTime = date.Date.AddDays(1).AddSeconds(-1),
                CallQueueId = string.Join(",", callQueues)
            }).ToList();
        }

        public async Task<List<int>> GetAgentsInCallQueuesAsync(List<string> callQueues)
        {
            return await _context.AgentActiveHistories
                .Where(h => h.IsActived)
                .Select(h => h.AgentId.Value)
                .Distinct()
                .ToListAsync();
        }
    }
}