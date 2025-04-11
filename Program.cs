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
builder.Services.AddScoped<IGraphQLCallService, GraphQLCallService>();
//builder.Services.AddHostedService<CallSyncService>();

// Register background service
builder.Services.AddHostedService<QueueReportedAgentsSyncJob>();

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