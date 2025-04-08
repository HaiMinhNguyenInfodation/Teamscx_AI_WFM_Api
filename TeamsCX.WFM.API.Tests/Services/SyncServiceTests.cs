using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Services;
using Xunit;

namespace TeamsCX.WFM.API.Tests.Services
{
    public class SyncServiceTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IMicrosoftGraphService> _graphServiceMock;
        private readonly SyncService _syncService;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public SyncServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _graphServiceMock = new Mock<IMicrosoftGraphService>();
            _syncService = new SyncService(_serviceProviderMock.Object, _graphServiceMock.Object);
            
            // Use in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Setup empty responses for other service calls
            var emptyResponse = JsonSerializer.Deserialize<JsonElement>(@"{""value"":[]}");
            _graphServiceMock.Setup(x => x.GetTeamMembersAsync(It.IsAny<string>()))
                .ReturnsAsync(emptyResponse);
            _graphServiceMock.Setup(x => x.GetTeamShiftsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(emptyResponse);
            _graphServiceMock.Setup(x => x.GetSchedulingGroupsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(emptyResponse);
        }

        [Fact]
        public async Task SyncTeamsAsync_ShouldCreateNewTeam_WhenTeamDoesNotExist()
        {
            // Arrange
            var teamId = "test-team-id";
            var teamName = "Test Team";
            var teamDescription = "Test Description";

            var teamsResponse = JsonSerializer.Deserialize<JsonElement>($@"{{
                ""value"": [{{
                    ""id"": ""{teamId}"",
                    ""displayName"": ""{teamName}"",
                    ""description"": ""{teamDescription}""
                }}]
            }}");

            _graphServiceMock.Setup(x => x.GetTeamsAsync())
                .ReturnsAsync(teamsResponse);

            var serviceScope = new Mock<IServiceScope>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var context = new ApplicationDbContext(_dbContextOptions);

            serviceScope.Setup(x => x.ServiceProvider)
                .Returns(new ServiceCollection()
                    .AddScoped<ApplicationDbContext>(_ => context)
                    .BuildServiceProvider());

            serviceScopeFactory.Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            // Act
            await _syncService.SyncAllDataAsync();

            // Assert
            var savedTeam = await context.Teams.FirstOrDefaultAsync(t => t.MicrosoftTeamId == teamId);
            Assert.NotNull(savedTeam);
            Assert.Equal(teamName, savedTeam.DisplayName);
            Assert.Equal(teamDescription, savedTeam.Description);
        }

        [Fact]
        public async Task SyncTeamsAsync_ShouldUpdateExistingTeam_WhenTeamExists()
        {
            // Arrange
            var teamId = "test-team-id";
            var initialName = "Initial Team";
            var initialDescription = "Initial Description";
            var updatedName = "Updated Team";
            var updatedDescription = "Updated Description";

            var context = new ApplicationDbContext(_dbContextOptions);
            context.Teams.Add(new Team
            {
                MicrosoftTeamId = teamId,
                DisplayName = initialName,
                Description = initialDescription,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var teamsResponse = JsonSerializer.Deserialize<JsonElement>($@"{{
                ""value"": [{{
                    ""id"": ""{teamId}"",
                    ""displayName"": ""{updatedName}"",
                    ""description"": ""{updatedDescription}""
                }}]
            }}");

            _graphServiceMock.Setup(x => x.GetTeamsAsync())
                .ReturnsAsync(teamsResponse);

            var serviceScope = new Mock<IServiceScope>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();

            serviceScope.Setup(x => x.ServiceProvider)
                .Returns(new ServiceCollection()
                    .AddScoped<ApplicationDbContext>(_ => context)
                    .BuildServiceProvider());

            serviceScopeFactory.Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            // Act
            await _syncService.SyncAllDataAsync();

            // Assert
            var updatedTeam = await context.Teams.FirstOrDefaultAsync(t => t.MicrosoftTeamId == teamId);
            Assert.NotNull(updatedTeam);
            Assert.Equal(updatedName, updatedTeam.DisplayName);
            Assert.Equal(updatedDescription, updatedTeam.Description);
        }
    }
} 