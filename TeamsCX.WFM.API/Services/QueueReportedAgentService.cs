using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;
using System.Linq;
using System.Text.Json.Serialization;

namespace TeamsCX.WFM.API.Services
{
    public interface IQueueReportedAgentService
    {
        Task SyncReportedAgentsAsync(CancellationToken cancellationToken = default);
    }

    public class QueueReportedAgentService : IQueueReportedAgentService
    {
        private readonly ILogger<QueueReportedAgentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly object _syncLock = new object();
        private bool _isSyncing;

        public QueueReportedAgentService(
            ILogger<QueueReportedAgentService> logger,
            IConfiguration configuration,
            HttpClient httpClient,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _serviceScopeFactory = serviceScopeFactory;
            _isSyncing = false;
        }

        public async Task SyncReportedAgentsAsync(CancellationToken cancellationToken = default)
        {
            lock (_syncLock)
            {
                if (_isSyncing)
                {
                    _logger.LogWarning("Sync operation is already in progress");
                    return;
                }
                _isSyncing = true;
            }

            try
            {
                // Step 1: Get Teams token
                var token = await GetTeamsTokenAsync();

                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Step 2: Get all queues from database
                var queues = await context.Queues.ToListAsync(cancellationToken);

                foreach (var queue in queues)
                {
                    try
                    {
                        // Step 3: Get queue information from Microsoft Teams API
                        var queueInfo = await GetQueueInformationAsync(queue.MicrosoftQueueId, token);

                        if (queueInfo?.CallQueue?.Users == null)
                        {
                            continue;
                        }

                        // Step 4: Update reported agents in database
                        await UpdateReportedAgentsAsync(context, queue.Id, queueInfo.CallQueue.Users, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing queue {QueueId}", queue.MicrosoftQueueId);
                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private async Task<string> GetTeamsTokenAsync()
        {
            var tokenEndpoint = $"https://login.microsoftonline.com/{_configuration["MicrosoftGraph:TenantId"]}/oauth2/v2.0/token";

            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _configuration["MicrosoftGraph:ClientId"] },
                { "client_secret", _configuration["MicrosoftGraph:ClientSecret"] },
                { "scope", "48ac35b8-9aa8-4d74-927d-1f4a14a0b239/.default" }
            };

            var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return tokenResponse.AccessToken;
        }

        private async Task<QueueResponse> GetQueueInformationAsync(string queueId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://api.interfaces.records.teams.microsoft.com/Teams.VoiceApps/callqueues/{queueId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<QueueResponse>();
        }

        private async Task UpdateReportedAgentsAsync(ApplicationDbContext dbContext, int queueId, List<string> reportedAgentIds, CancellationToken cancellationToken)
        {
            // Get current reported agents for the queue
            var currentReportedAgents = await dbContext.QueueReportedAgents
                .Where(qra => qra.QueueId == queueId && qra.IsActive)
                .ToListAsync(cancellationToken);

            // Get or create agents from the reported IDs
            var agents = await dbContext.Agents
                .Where(a => reportedAgentIds.Contains(a.MicrosoftUserId))
                .ToListAsync(cancellationToken);

            var existingAgentIds = agents.Select(a => a.MicrosoftUserId).ToHashSet();

            // Update IsReported status for all agents
            var agentsToUpdate = await dbContext.Agents
                .Where(a => reportedAgentIds.Contains(a.MicrosoftUserId))
                .ToListAsync(cancellationToken);

            foreach (var agent in agentsToUpdate)
            {
                agent.IsReported = true;
            }

            // Deactivate agents no longer in the queue
            var agentsToDeactivate = currentReportedAgents
                .Where(cra => !reportedAgentIds.Contains(agents.First(a => a.Id == cra.AgentId).MicrosoftUserId));

            foreach (var agent in agentsToDeactivate)
            {
                agent.IsActive = false;
                agent.UpdatedAt = DateTime.UtcNow;
            }

            // Add new reported agents
            var newReportedAgents = agents
                .Where(a => !currentReportedAgents.Any(cra => cra.AgentId == a.Id))
                .Select(a => new QueueReportedAgent
                {
                    QueueId = queueId,
                    AgentId = a.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            await dbContext.QueueReportedAgents.AddRangeAsync(newReportedAgents, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class QueueResponse
    {
        public CallQueue CallQueue { get; set; }
    }

    public class CallQueue
    {
        public string Identity { get; set; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public List<string> Users { get; set; }
        public List<Agent> Agents { get; set; }
    }
}