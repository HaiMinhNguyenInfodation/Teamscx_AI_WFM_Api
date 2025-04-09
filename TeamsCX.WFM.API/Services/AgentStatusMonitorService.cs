using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Services
{
    public class AgentStatusMonitorService : BackgroundService
    {
        private readonly ILogger<AgentStatusMonitorService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _tokenExpiration;
        private readonly TimeSpan _monitorInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _tokenRefreshBuffer = TimeSpan.FromMinutes(5);

        public AgentStatusMonitorService(
            ILogger<AgentStatusMonitorService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Agent Status Monitor Service is starting");

            // Initialize token on startup
            await GetAccessTokenAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAgentStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing agent statuses");
                }

                await Task.Delay(_monitorInterval, stoppingToken);
            }

            _logger.LogInformation("Agent Status Monitor Service is stopping");
        }

        private async Task ProcessAgentStatusesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get all agents
            var agents = await context.Agents.ToListAsync();
            if (!agents.Any())
            {
                _logger.LogDebug("No agents found in the database");
                return;
            }

            // Get Microsoft user IDs
            var microsoftUserIds = agents.Select(a => a.MicrosoftUserId).ToList();

            // Get current statuses from Microsoft
            var statuses = await GetAgentStatusesAsync(microsoftUserIds);
            if (statuses == null)
            {
                return;
            }

            // Process each agent's status
            var statusChanges = new List<AgentStatusHistory>();
            foreach (var status in statuses)
            {
                var agent = agents.FirstOrDefault(a => a.MicrosoftUserId == status.Id);
                if (agent != null)
                {
                    // Get the last status for this agent
                    var lastStatus = await context.AgentStatusHistories
                        .Where(h => h.AgentId == agent.Id)
                        .OrderByDescending(h => h.CreatedAt)
                        .FirstOrDefaultAsync();

                    // Only create new history if status has changed
                    if (lastStatus == null || lastStatus.Status != MapStatus(status.Availability, status.Activity))
                    {
                        statusChanges.Add(new AgentStatusHistory
                        {
                            CreatedAt = DateTime.UtcNow,
                            AgentId = agent.Id,
                            Status = MapStatus(status.Availability, status.Activity)
                        });
                        _logger.LogDebug($"Status change detected for agent {agent.DisplayName}: {status.Availability}/{status.Activity}");
                    }
                }
            }

            // Save changes if any
            if (statusChanges.Any())
            {
                await context.AgentStatusHistories.AddRangeAsync(statusChanges);
                await context.SaveChangesAsync();
                _logger.LogInformation($"Saved {statusChanges.Count} agent status changes");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // Check if token is valid and won't expire in the next 5 minutes
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow.Add(_tokenRefreshBuffer) < _tokenExpiration)
            {
                _logger.LogDebug("Using cached access token");
                return _accessToken;
            }

            try
            {
                var tenantId = _configuration["MicrosoftGraph:TenantId"];
                var clientId = _configuration["MicrosoftGraph:ClientId"];
                var clientSecret = _configuration["MicrosoftGraph:ClientSecret"];

                _logger.LogDebug("Requesting new access token from Microsoft");
                _logger.LogDebug($"Token request details - TenantId: {tenantId}, ClientId: {clientId}");

                // Create form-urlencoded content instead of JSON
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default")
                });

                var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                _logger.LogDebug($"Sending token request to: {tokenUrl}");

                var response = await _httpClient.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Token response received: {responseContent}");

                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    _accessToken = tokenResponse.GetProperty("access_token").GetString();
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.GetProperty("expires_in").GetInt32());

                    _logger.LogDebug("Successfully obtained new access token");
                    _logger.LogDebug($"Token expiration: {_tokenExpiration}");
                    return _accessToken;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to get access token. Status: {response.StatusCode}, Response: {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access token");
                return null;
            }
        }

        private async Task<List<AgentStatusResponse>> GetAgentStatusesAsync(List<string> userIds)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Cannot get agent statuses: Access token is null or empty");
                    return null;
                }

                _logger.LogDebug($"Requesting status for {userIds.Count} agents");
                _logger.LogDebug($"Agent IDs: {string.Join(", ", userIds)}");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request = new { ids = userIds };
                var requestJson = JsonSerializer.Serialize(request);
                _logger.LogDebug($"Sending presence request: {requestJson}");

                var response = await _httpClient.PostAsync(
                    "https://graph.microsoft.com/v1.0/communications/getPresencesByUserId",
                    new StringContent(requestJson, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Presence response received: {content}");

                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    var statuses = result.GetProperty("value").EnumerateArray()
                        .Select(item => new AgentStatusResponse
                        {
                            Id = item.GetProperty("id").GetString(),
                            Availability = item.GetProperty("availability").GetString(),
                            Activity = item.GetProperty("activity").GetString()
                        })
                        .ToList();

                    _logger.LogDebug($"Successfully retrieved status for {statuses.Count} agents");
                    foreach (var status in statuses)
                    {
                        _logger.LogDebug($"Agent {status.Id}: Availability={status.Availability}, Activity={status.Activity}");
                    }

                    return statuses;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to get agent statuses. Status: {response.StatusCode}, Response: {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent statuses");
                return null;
            }
        }

        private AgentStatus MapStatus(string availability, string activity)
        {
            // Map Microsoft Teams status to our AgentStatus enum
            return (availability.ToLower(), activity.ToLower()) switch
            {
                ("available", _) => AgentStatus.Available,
                ("busy", "inacall") => AgentStatus.InACall,
                ("busy", "inameeting") => AgentStatus.Busy,
                ("busy", "presenting") => AgentStatus.Presenting,
                ("busy", "focusing") => AgentStatus.Busy,
                ("berightback", "berightback") => AgentStatus.BeRightBack,
                ("away", "away") => AgentStatus.Away,
                ("donotdisturb", "presenting") => AgentStatus.Presenting,
                ("donotdisturb", "focusing") => AgentStatus.DoNotDisturb,
                ("donotdisturb", _) => AgentStatus.DoNotDisturb,
                ("outofoffice", _) => AgentStatus.Away,
                ("unknown", _) => AgentStatus.Offline,
                ("offline", _) => AgentStatus.Offline,
                _ => AgentStatus.Offline
            };
        }

        private class AgentStatusResponse
        {
            public string Id { get; set; }
            public string Availability { get; set; }
            public string Activity { get; set; }
        }
    }
}