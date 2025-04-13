using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Services;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdherenceController : ControllerBase
    {
        private readonly IAdherenceService _adherenceService;
        private readonly IAgentPerformanceService _agentPerformanceService;
        private readonly ILogger<AdherenceController> _logger;

        public AdherenceController(
            IAdherenceService adherenceService,
            IAgentPerformanceService agentPerformanceService,
            ILogger<AdherenceController> logger)
        {
            _adherenceService = adherenceService;
            _agentPerformanceService = agentPerformanceService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard summary including agent stats and status
        /// </summary>
        /// <param name="queueMicrosoftIds">Optional queue Microsoft IDs filter (comma-separated)</param>
        [HttpGet("summary")]
        public async Task<ActionResult<Models.DTOs.AdherenceResponse>> GetDashboardSummary([FromQuery] string[] queueMicrosoftIds = null)
        {
            try
            {
                var response = await _adherenceService.GetDashboardSummaryAsync(queueMicrosoftIds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get agent performance data
        /// </summary>
        /// <param name="queueMicrosoftId">Optional queue Microsoft IDs filter (comma-separated)</param>
        [HttpGet("agent-performance")]
        public async Task<ActionResult<AgentPerformanceResponseDTO>> GetAgentPerformance([FromQuery] List<string> queueMicrosoftId = null)
        {
            try
            {
                var response = await _agentPerformanceService.GetAgentPerformanceAsync(queueMicrosoftId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent performance");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get current call queue metrics
        /// </summary>
        /// <param name="queueMicrosoftIds">Optional queue Microsoft IDs filter (comma-separated)</param>
        [HttpGet("queue-metrics")]
        public async Task<ActionResult<QueueMetricsResponse>> GetQueueMetrics([FromQuery] string[] queueMicrosoftIds = null)
        {
            try
            {
                var response = await _adherenceService.GetQueueMetricsAsync(queueMicrosoftIds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue metrics");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get agent activities timeline
        /// </summary>
        /// <param name="queueId">Optional queue ID filter</param>
        [HttpGet("agent-activities")]
        public async Task<ActionResult<AgentActivitiesResponse>> GetAgentActivities(string queueId = null)
        {
            var agentSchedule = new AgentActivitiesResponse
            {
                AgentDisplayName = "John Doe",
                CurrentStatus = "Active",
                Adherence = "90%",
                Scheduled = new AgentScheduled
                {
                    From = DateTimeOffset.UtcNow.AddHours(-8),
                    To = DateTimeOffset.UtcNow,
                    TotalHours = 8.0,
                    Shifts = new List<AgentShift>
                {
                    new AgentShift
                    {
                        From = DateTimeOffset.UtcNow.AddHours(-8),
                        To = DateTimeOffset.UtcNow.AddHours(-4),
                        DisplayText = "Morning Shift",
                        OnQueue = "Queue1",
                        Activities = new List<AgentShiftActivity>
                        {
                            new AgentShiftActivity
                            {
                                From = DateTimeOffset.UtcNow.AddHours(-8),
                                To = DateTimeOffset.UtcNow.AddHours(-6),
                                DisplayText = "Customer Call",
                                Theme = "call"
                            }
                        }
                    }
                }
                },
                Actual = new AgentActual
                {
                    Timelines = new List<AgentTimeline>
                {
                    new AgentTimeline
                    {
                        From = DateTimeOffset.UtcNow.AddHours(-8),
                        To = DateTimeOffset.UtcNow.AddHours(-7),
                        OnQueue = "Queue1",
                        Status = "Available"
                    }
                },
                    ActiveTimelines = new List<ActiveTimeline>
                {
                    new ActiveTimeline
                    {
                        Action = "Login",
                        Timestamp = DateTime.UtcNow,
                        OnQueue = "Queue1"
                    }
                }
                }
            };

            return Ok(agentSchedule);
        }
    }
}