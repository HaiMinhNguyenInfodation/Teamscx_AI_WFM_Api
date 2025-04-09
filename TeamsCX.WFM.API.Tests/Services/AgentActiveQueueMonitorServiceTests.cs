using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Services;
using Xunit;

namespace TeamsCX.WFM.API.Tests.Services
{
    public class AgentActiveQueueMonitorServiceTests
    {
        private readonly Mock<ILogger<AgentActiveQueueMonitorService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly AgentActiveQueueMonitorService _service;

        public AgentActiveQueueMonitorServiceTests()
        {
            _loggerMock = new Mock<ILogger<AgentActiveQueueMonitorService>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _serviceProviderMock = new Mock<IServiceProvider>();

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            // Setup service provider
            var serviceScope = new Mock<IServiceScope>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(_dbContext);

            // Setup configuration
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(x => x.Value).Returns("5");
            _configurationMock.Setup(x => x.GetSection("AgentActiveQueueMonitor:MonitorIntervalSeconds"))
                .Returns(configurationSectionMock.Object);
            _configurationMock.Setup(x => x.GetSection("AgentActiveQueueMonitor:MaxRetries"))
                .Returns(configurationSectionMock.Object);
            _configurationMock.Setup(x => x.GetSection("AgentActiveQueueMonitor:RetryDelaySeconds"))
                .Returns(configurationSectionMock.Object);
            _configurationMock.Setup(x => x["MicrosoftGraph:TenantId"]).Returns("test-tenant");
            _configurationMock.Setup(x => x["MicrosoftGraph:ClientId"]).Returns("test-client");
            _configurationMock.Setup(x => x["MicrosoftGraph:ClientSecret"]).Returns("test-secret");

            _service = new AgentActiveQueueMonitorService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                _configurationMock.Object,
                _httpClient);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidQueue_ShouldUpdateAgentStatus()
        {
            // Arrange
            var queue = new Queue
            {
                MicrosoftQueueId = "test-queue-1",
                Name = "Test Queue",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbContext.Queues.AddAsync(queue);
            await _dbContext.SaveChangesAsync();

            var agent = new Agent
            {
                MicrosoftUserId = "test-agent-1",
                DisplayName = "Test Agent",
                Email = "test@example.com"
            };
            await _dbContext.Agents.AddAsync(agent);
            await _dbContext.SaveChangesAsync();

            // Setup HTTP client to return queue data
            var queueData = JsonDocument.Parse(@"{
                ""CallQueue"": {
                    ""Agents"": [
                        {
                            ""ObjectId"": ""test-agent-1"",
                            ""OptIn"": true
                        }
                    ]
                }
            }").RootElement;

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("test-queue-1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(queueData.ToString())
                });

            // Setup token response
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("oauth2/v2.0/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""access_token"": ""test-token"",
                        ""expires_in"": 3600
                    }")
                });

            // Act
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            await _service.StartAsync(cancellationToken);
            await Task.Delay(100); // Give some time for the service to process
            await _service.StopAsync(cancellationToken);

            // Assert
            var history = await _dbContext.AgentActiveHistories
                .FirstOrDefaultAsync(h => h.AgentId == agent.Id && h.QueueId == queue.Id);

            Assert.NotNull(history);
            Assert.True(history.IsActived);
            Assert.Equal(agent.Id, history.AgentId);
            Assert.Equal(queue.Id, history.QueueId);
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidQueue_ShouldLogWarning()
        {
            // Arrange
            var queue = new Queue
            {
                MicrosoftQueueId = "invalid-queue",
                Name = "Invalid Queue",
                Description = "Invalid Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbContext.Queues.AddAsync(queue);
            await _dbContext.SaveChangesAsync();

            // Setup HTTP client to return error
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("invalid-queue")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

            // Setup token response
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("oauth2/v2.0/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""access_token"": ""test-token"",
                        ""expires_in"": 3600
                    }")
                });

            // Act
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            await _service.StartAsync(cancellationToken);
            await Task.Delay(100); // Give some time for the service to process
            await _service.StopAsync(cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Attempt 1 failed for queue invalid-queue")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}