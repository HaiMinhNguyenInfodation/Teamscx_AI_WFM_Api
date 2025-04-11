using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Services;
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

// Add DbContext with scoped lifetime
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

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

// Register QueueReportedAgentService as singleton
builder.Services.AddSingleton<IQueueReportedAgentService, QueueReportedAgentService>();

// Register QueueReportedAgentsSyncJob as hosted service
builder.Services.AddHostedService<QueueReportedAgentsSyncJob>();

// Register GraphQL and Call Sync services
builder.Services.AddScoped<GraphQLCallService>(sp =>
    new GraphQLCallService(
        sp.GetRequiredService<HttpClient>(),
        builder.Configuration["GraphQL:Endpoint"] ?? "https://tcx-teamsv2-demo-datasource.azurewebsites.net/api/graphql"
    ));

// Register Call Retrieval Service
builder.Services.AddScoped<ICallRetrievalService, CallRetrievalService>();

// Register background services
builder.Services.AddHostedService<QueueReportedAgentsSyncJob>();

// Create a service provider for the sync jobs
var serviceProvider = builder.Services.BuildServiceProvider();

// Register sync jobs as singletons
builder.Services.AddSingleton<HistoricalCallSyncJob>(sp =>
    new HistoricalCallSyncJob(
        serviceProvider.GetRequiredService<ICallRetrievalService>(),
        sp.GetRequiredService<ILogger<HistoricalCallSyncJob>>(),
        builder.Configuration["CallSync:ResourceAccounts"] ?? "US - CQ - Demo - Sales"
    ));

builder.Services.AddSingleton<RealTimeCallSyncJob>(sp =>
    new RealTimeCallSyncJob(
        serviceProvider.GetRequiredService<ICallRetrievalService>(),
        sp.GetRequiredService<ILogger<RealTimeCallSyncJob>>(),
        builder.Configuration["CallSync:ResourceAccounts"] ?? "US - CQ - Demo - Sales"
    ));

// Register the hosted services
builder.Services.AddHostedService<HistoricalCallSyncJob>(sp => sp.GetRequiredService<HistoricalCallSyncJob>());
builder.Services.AddHostedService<RealTimeCallSyncJob>(sp => sp.GetRequiredService<RealTimeCallSyncJob>());

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
