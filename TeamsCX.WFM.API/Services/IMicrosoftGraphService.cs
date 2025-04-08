using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeamsCX.WFM.API.Services
{
    public interface IMicrosoftGraphService
    {
        Task<HttpClient> GetAuthenticatedClientAsync();
        Task<JsonElement> GetTeamsAsync();
        Task<JsonElement> GetTeamMembersAsync(string teamId);
        Task<JsonElement> GetTeamShiftsAsync(string teamId, string ownerId);
        Task<JsonElement> GetSchedulingGroupsAsync(string teamId, string ownerId);
    }
} 