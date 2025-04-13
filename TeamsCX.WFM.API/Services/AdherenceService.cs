using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeamsCX.WFM.API.Models.DTOs;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Repositories;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;

namespace TeamsCX.WFM.API.Services
{
    public class AdherenceService : IAdherenceService
    {
        private readonly ILogger<AdherenceService> _logger;
        private readonly IAdherenceRepository _adherenceRepository;

        public AdherenceService(ILogger<AdherenceService> logger, IAdherenceRepository adherenceRepository)
        {
            _logger = logger;
            _adherenceRepository = adherenceRepository;
        }

        public async Task<Models.DTOs.AdherenceResponse> GetDashboardSummaryAsync(string[] queueMicrosoftIds = null)
        {
            try
            {
                // Convert array to comma-separated string for repository
                return await _adherenceRepository.GetDashboardSummaryAsync(queueMicrosoftIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                throw;
            }
        }

        public async Task<QueueMetricsResponse> GetQueueMetricsAsync(string[] queueMicrosoftIds = null)
        {
            try
            {
                return await _adherenceRepository.GetQueueMetricsAsync(queueMicrosoftIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue metrics");
                throw;
            }
        }
    }
}