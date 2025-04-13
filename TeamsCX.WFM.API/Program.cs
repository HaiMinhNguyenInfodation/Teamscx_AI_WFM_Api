using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Services;
using TeamsCX.WFM.API.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamsCX.WFM.API.Services.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Add DbContextFactory with scoped lifetime
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// Configure HttpClient for Microsoft Graph API
builder.Services.AddHttpClient<IMicrosoftGraphService, MicrosoftGraphService>();
builder.Services.AddHttpClient<AgentActiveQueueMonitorService>();
builder.Services.AddHttpClient<AgentStatusMonitorService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddSingleton<IMicrosoftGraphService, MicrosoftGraphService>();
builder.Services.AddSingleton<SyncService>();
builder.Services.AddHostedService<SyncBackgroundService>();
builder.Services.AddHostedService<AgentActiveQueueMonitorService>();
builder.Services.AddHostedService<AgentStatusMonitorService>();
builder.Services.AddScoped<IRealTimeService, RealTimeService>();
builder.Services.AddScoped<IUpToNowService, UpToNowService>();
builder.Services.AddScoped<IAgentStatusService, AgentStatusService>();
builder.Services.AddScoped<IAdherenceService, AdherenceService>();
builder.Services.AddScoped<IAdherenceRepository, AdherenceRepository>();

// Register Agent Performance services
builder.Services.AddScoped<IAgentPerformanceService, AgentPerformanceService>();
builder.Services.AddScoped<IAgentPerformanceRepository, AgentPerformanceRepository>();

// Register QueueReportedAgentService as singleton
builder.Services.AddSingleton<IQueueReportedAgentService, QueueReportedAgentService>();

// Register QueueReportedAgentsSyncJob as hosted service
builder.Services.AddHostedService<QueueReportedAgentsSyncJob>();

// Add GraphQL call service
builder.Services.AddSingleton<GraphQLCallService>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<GraphQLCallService>>();

    return new GraphQLCallService(httpClient, configuration["GraphQL:Endpoint"] ?? "https://tcx-teamsv2-demo-datasource.azurewebsites.net/graphql", logger);
});

// Add call sync services
builder.Services.AddScoped<CallSyncService>();
builder.Services.AddScoped<CallSyncBackgroundService>();

// Register Call_CX Retrieval Service
builder.Services.AddSingleton<ICallRetrievalService, CallRetrievalService>();

// Register background services
builder.Services.AddHostedService<QueueReportedAgentsSyncJob>();

// Register sync jobs
builder.Services.AddSingleton<HistoricalCallSyncJob>(sp =>
    new HistoricalCallSyncJob(
        sp.GetRequiredService<ICallRetrievalService>(),
        sp.GetRequiredService<ILogger<HistoricalCallSyncJob>>(),
        builder.Configuration["ResourceAccounts"]));

builder.Services.AddHostedService<HistoricalCallSyncJob>(sp => sp.GetRequiredService<HistoricalCallSyncJob>());

// Register RealTimeCallSyncJob as a hosted service with proper scoping
builder.Services.AddHostedService<RealTimeCallSyncJob>(sp =>
{
    var scope = sp.CreateScope();
    return new RealTimeCallSyncJob(
        scope.ServiceProvider.GetRequiredService<ICallRetrievalService>(),
        scope.ServiceProvider.GetRequiredService<ILogger<RealTimeCallSyncJob>>(),
        builder.Configuration["ResourceAccounts"],
        scope.ServiceProvider);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(); // Add CORS middleware
app.UseAuthorization();
app.MapControllers();
app.Run();

//public class GraphQLCallService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _endpoint;

//    public GraphQLCallService(HttpClient httpClient, IConfiguration configuration)
//    {
//        _httpClient = httpClient;
//        _endpoint = configuration["GraphQL:Endpoint"] ??
//            throw new ArgumentNullException("GraphQL:Endpoint", "GraphQL:Endpoint configuration is missing");
//    }

//    // Rest of the class implementation
//}
