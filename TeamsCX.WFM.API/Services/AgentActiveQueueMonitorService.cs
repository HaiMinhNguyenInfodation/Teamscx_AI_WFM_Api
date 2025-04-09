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
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TeamsCX.WFM.API.Services
{
    public class AgentActiveQueueMonitorService : BackgroundService
    {
        private readonly ILogger<AgentActiveQueueMonitorService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _tokenExpiration;
        private readonly TimeSpan _monitorInterval;
        private readonly int _maxRetries;
        private readonly TimeSpan _retryDelay;
        private readonly TimeSpan _tokenRefreshBuffer = TimeSpan.FromMinutes(5);
        private int _totalStatusChanges = 0;
        private int _totalErrors = 0;

        public AgentActiveQueueMonitorService(
            ILogger<AgentActiveQueueMonitorService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _httpClient = httpClient;

            // Load configuration values with defaults
            _monitorInterval = TimeSpan.FromSeconds(
                _configuration.GetValue("AgentActiveQueueMonitor:MonitorIntervalSeconds", 5));
            _maxRetries = _configuration.GetValue("AgentActiveQueueMonitor:MaxRetries", 3);
            _retryDelay = TimeSpan.FromSeconds(
                _configuration.GetValue("AgentActiveQueueMonitor:RetryDelaySeconds", 5));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Agent Active Queue Monitor Service is starting with configuration: " +
                $"MonitorInterval={_monitorInterval}, MaxRetries={_maxRetries}");

            // Initialize token on startup
            await GetAccessTokenAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessQueuesAsync();
                    LogMetrics();
                }
                catch (Exception ex)
                {
                    _totalErrors++;
                    _logger.LogError(ex, "Error occurred while processing agent active queues");
                }

                await Task.Delay(_monitorInterval, stoppingToken);
            }

            _logger.LogInformation("Agent Active Queue Monitor Service is stopping");
        }

        private async Task ProcessQueuesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var queues = await context.Queues.ToListAsync();
            var statusChanges = new List<AgentActiveHistory>();

            foreach (var queue in queues)
            {
                try
                {
                    var queueData = await GetQueueDataWithRetryAsync(queue.MicrosoftQueueId);
                    if (queueData != null)
                    {
                        var changes = await ProcessQueueAgentsAsync(context, queue, queueData.Value);
                        statusChanges.AddRange(changes);
                    }
                }
                catch (Exception ex)
                {
                    _totalErrors++;
                    _logger.LogError(ex, $"Error processing queue {queue.Name} ({queue.MicrosoftQueueId})");
                }
            }

            if (statusChanges.Any())
            {
                await context.AgentActiveHistories.AddRangeAsync(statusChanges);
                await context.SaveChangesAsync();
                _totalStatusChanges += statusChanges.Count;
            }
        }

        private async Task<JsonElement?> GetQueueDataWithRetryAsync(string queueId)
        {
            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    var token = await GetAccessTokenAsync();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.GetAsync(
                        $"https://api.interfaces.records.teams.microsoft.com/Teams.VoiceApps/agentcallqueues/{queueId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);
                    }

                    if (attempt < _maxRetries)
                    {
                        _logger.LogWarning($"Attempt {attempt} failed for queue {queueId}. Status: {response.StatusCode}. Retrying...");
                        await Task.Delay(_retryDelay);
                    }
                    else
                    {
                        _logger.LogError($"Failed to get queue data for {queueId} after {_maxRetries} attempts. Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    if (attempt < _maxRetries)
                    {
                        _logger.LogWarning(ex, $"Attempt {attempt} failed for queue {queueId}. Retrying...");
                        await Task.Delay(_retryDelay);
                    }
                    else
                    {
                        _logger.LogError(ex, $"Failed to get queue data for {queueId} after {_maxRetries} attempts");
                    }
                }
            }

            return null;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // Check if token is valid and won't expire in the next 5 minutes
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow.Add(_tokenRefreshBuffer) < _tokenExpiration)
            {
                return _accessToken;
            }

            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    var tenantId = _configuration["MicrosoftGraph:TenantId"];
                    var clientId = _configuration["MicrosoftGraph:ClientId"];
                    var clientSecret = _configuration["MicrosoftGraph:ClientSecret"];

                    var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    var content = new StringContent(
                        $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&scope=48ac35b8-9aa8-4d74-927d-1f4a14a0b239/.default",
                        Encoding.UTF8,
                        "application/x-www-form-urlencoded");

                    var response = await _httpClient.PostAsync(tokenEndpoint, content);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseContent);

                    _accessToken = tokenResponse.GetProperty("access_token").GetString();
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.GetProperty("expires_in").GetInt32());

                    _logger.LogInformation($"Successfully refreshed access token. Expires at: {_tokenExpiration}");
                    return _accessToken;
                }
                catch (Exception ex)
                {
                    if (attempt < _maxRetries)
                    {
                        _logger.LogWarning(ex, $"Attempt {attempt} failed to get access token. Retrying...");
                        await Task.Delay(_retryDelay);
                    }
                    else
                    {
                        _logger.LogError(ex, $"Failed to get access token after {_maxRetries} attempts");
                        throw;
                    }
                }
            }

            throw new Exception("Failed to get access token after all retry attempts");
        }

        private async Task<List<AgentActiveHistory>> ProcessQueueAgentsAsync(ApplicationDbContext context, Queue queue, JsonElement queueData)
        {
            var statusChanges = new List<AgentActiveHistory>();
            var agents = queueData.GetProperty("CallQueue").GetProperty("Agents").EnumerateArray();
            var microsoftUserIds = agents.Select(a => a.GetProperty("ObjectId").GetString()).ToList();

            // Get all agents in one query
            var dbAgents = await context.Agents
                .Where(a => microsoftUserIds.Contains(a.MicrosoftUserId))
                .ToDictionaryAsync(a => a.MicrosoftUserId);

            // Get last statuses in one query
            var agentIds = dbAgents.Values.Select(a => a.Id).ToList();
            var lastStatuses = await context.AgentActiveHistories
                .Include(h => h.Agent)  // Include the Agent navigation property
                .Where(h => h.QueueId == queue.Id && h.AgentId.HasValue && agentIds.Contains(h.AgentId.Value))
                .GroupBy(h => h.AgentId!.Value)  // Using null-forgiving operator since we've filtered for HasValue
                .Select(g => new { AgentId = g.Key, History = g.OrderByDescending(h => h.CreatedAt).First() })
                .ToDictionaryAsync(x => x.AgentId, x => x.History);

            foreach (var agent in agents)
            {
                try
                {
                    var microsoftUserId = agent.GetProperty("ObjectId").GetString();
                    var isOptIn = agent.GetProperty("OptIn").GetBoolean();
                    _logger.LogInformation($"Agent {microsoftUserId} isOptIn: {isOptIn}");

                    if (dbAgents.TryGetValue(microsoftUserId, out var dbAgent))
                    {
                        var lastStatus = lastStatuses.GetValueOrDefault(dbAgent.Id);
                        _logger.LogInformation($"Agent {microsoftUserId} lastStatus: {lastStatus?.IsActived}");
                        if (lastStatus == null || lastStatus.IsActived != isOptIn)
                        {
                            statusChanges.Add(new AgentActiveHistory
                            {
                                CreatedAt = DateTime.UtcNow,
                                AgentId = dbAgent.Id,
                                QueueId = queue.Id,
                                IsActived = isOptIn
                            });
                            _logger.LogInformation($"Status changed for agent {dbAgent.DisplayName} in queue {queue.Name}: {(isOptIn ? "Active" : "Inactive")} (OptIn: {isOptIn})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _totalErrors++;
                    _logger.LogError(ex, "Error processing agent status");
                }
            }

            return statusChanges;
        }

        private void LogMetrics()
        {
            _logger.LogInformation($"Active Queue Monitor - Status Active Changes: {_totalStatusChanges}, Errors: {_totalErrors}");
        }
    }
}