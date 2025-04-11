using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

public class QueueReportedAgentsSyncJob : BackgroundService
{
    private readonly ILogger<QueueReportedAgentsSyncJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public QueueReportedAgentsSyncJob(
        ILogger<QueueReportedAgentsSyncJob> logger,
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddHours(2); // 2:00 AM UTC
                if (now > nextRun)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);

                await SyncReportedAgentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while syncing reported agents");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retrying
            }
        }
    }

    public async Task SyncReportedAgentsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Step 1: Get Teams token
        var token = await GetTeamsTokenAsync();

        // Step 2: Get all queues from database
        var queues = await dbContext.Queues.ToListAsync(stoppingToken);

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
                await UpdateReportedAgentsAsync(dbContext, queue.Id, queueInfo.CallQueue.Users, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queue {QueueId}", queue.MicrosoftQueueId);
            }
        }
    }

    private async Task<string> GetTeamsTokenAsync()
    {
        var tokenEndpoint = $"https://login.microsoftonline.com/{_configuration["Teams:TenantId"]}/oauth2/v2.0/token";

        var tokenRequest = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _configuration["Teams:ApplicationId"] },
            { "client_secret", _configuration["Teams:ClientSecret"] },
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

    private async Task UpdateReportedAgentsAsync(ApplicationDbContext dbContext, int queueId, List<string> reportedAgentIds, CancellationToken stoppingToken)
    {
        // Get current reported agents for the queue
        var currentReportedAgents = await dbContext.QueueReportedAgents
            .Where(qra => qra.QueueId == queueId && qra.IsActive)
            .ToListAsync(stoppingToken);

        // Get or create agents from the reported IDs
        var agents = await dbContext.Agents
            .Where(a => reportedAgentIds.Contains(a.MicrosoftUserId))
            .ToListAsync(stoppingToken);

        var existingAgentIds = agents.Select(a => a.MicrosoftUserId).ToHashSet();

        // Update IsReported status for all agents
        var agentsToUpdate = await dbContext.Agents
            .Where(a => reportedAgentIds.Contains(a.MicrosoftUserId))
            .ToListAsync(stoppingToken);

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

        await dbContext.QueueReportedAgents.AddRangeAsync(newReportedAgents, stoppingToken);
        await dbContext.SaveChangesAsync(stoppingToken);
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