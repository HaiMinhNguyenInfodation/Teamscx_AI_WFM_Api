using Microsoft.Extensions.DependencyInjection;
using TeamsCX.WFM.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient
builder.Services.AddHttpClient();

// Register GraphQL and Call Sync services
builder.Services.AddScoped<GraphQLCallService>(sp =>
    new GraphQLCallService(
        sp.GetRequiredService<HttpClient>(),
        builder.Configuration["GraphQL:Endpoint"] ?? "https://tcx-teamsv2-demo-datasource.azurewebsites.net/api/graphql"
    ));

// Register background services
builder.Services.AddHostedService<QueueReportedAgentsSyncJob>();

// Register sync jobs as singletons so they can be injected into controllers
builder.Services.AddSingleton<HistoricalCallSyncJob>(sp =>
    new HistoricalCallSyncJob(
        sp.GetRequiredService<GraphQLCallService>(),
        sp.GetRequiredService<ILogger<HistoricalCallSyncJob>>(),
        builder.Configuration["CallSync:ResourceAccounts"] ?? "US - CQ - Demo - Sales"
    ));

builder.Services.AddSingleton<RealTimeCallSyncJob>(sp =>
    new RealTimeCallSyncJob(
        sp.GetRequiredService<GraphQLCallService>(),
        sp.GetRequiredService<ILogger<RealTimeCallSyncJob>>(),
        builder.Configuration["CallSync:ResourceAccounts"] ?? "US - CQ - Demo - Sales"
    ));

// Register the hosted services
builder.Services.AddHostedService<HistoricalCallSyncJob>(sp => sp.GetRequiredService<HistoricalCallSyncJob>());
builder.Services.AddHostedService<RealTimeCallSyncJob>(sp => sp.GetRequiredService<RealTimeCallSyncJob>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();