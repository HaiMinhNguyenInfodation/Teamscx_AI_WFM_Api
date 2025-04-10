using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Services;

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

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HttpClient for Microsoft Graph API
builder.Services.AddHttpClient<IMicrosoftGraphService, MicrosoftGraphService>();
builder.Services.AddHttpClient<AgentActiveQueueMonitorService>();
builder.Services.AddHttpClient<AgentStatusMonitorService>();

// Register services
builder.Services.AddSingleton<IMicrosoftGraphService, MicrosoftGraphService>();
builder.Services.AddSingleton<SyncService>();
builder.Services.AddHostedService<SyncBackgroundService>();
builder.Services.AddHostedService<AgentActiveQueueMonitorService>();
builder.Services.AddHostedService<AgentStatusMonitorService>();
builder.Services.AddScoped<IRealTimeService, RealTimeService>();
builder.Services.AddScoped<IUpToNowService, UpToNowService>();
builder.Services.AddScoped<IAgentStatusService, AgentStatusService>();

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
