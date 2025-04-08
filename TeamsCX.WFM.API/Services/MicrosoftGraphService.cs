using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Services
{
    public class MicrosoftGraphService : IMicrosoftGraphService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _accessToken;
        private DateTime _tokenExpiration;

        public MicrosoftGraphService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            {
                return _accessToken;
            }

            var tenantId = _configuration["MicrosoftGraph:TenantId"];
            var clientId = _configuration["MicrosoftGraph:ClientId"];
            var clientSecret = _configuration["MicrosoftGraph:ClientSecret"];

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var content = new StringContent(
                $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&scope=https://graph.microsoft.com/.default",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            _accessToken = tokenResponse.GetProperty("access_token").GetString();
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.GetProperty("expires_in").GetInt32() - 300);

            return _accessToken;
        }

        public async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _httpClient;
        }

        public async Task<JsonElement> GetTeamsAsync()
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/teams");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }

        public async Task<JsonElement> GetTeamMembersAsync(string teamId)
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/teams/{teamId}/members");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }

        public async Task<JsonElement> GetTeamShiftsAsync(string teamId, string ownerId)
        {
            var client = await GetAuthenticatedClientAsync();
            client.DefaultRequestHeaders.Add("MS-APP-ACTS-AS", ownerId);
            if (client.DefaultRequestHeaders.TryGetValues("MS-APP-ACTS-AS", out var values))
            {
                Console.WriteLine($"MS-APP-ACTS-AS header value: {string.Join(", ", values)}");
            }
            var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/teams/{teamId}/schedule/shifts");
            client.DefaultRequestHeaders.Remove("MS-APP-ACTS-AS");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return JsonSerializer.Deserialize<JsonElement>("{\"value\":[]}");
            }
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }

        public async Task<JsonElement> GetSchedulingGroupsAsync(string teamId, string ownerId)
        {
            var client = await GetAuthenticatedClientAsync();
            client.DefaultRequestHeaders.Add("MS-APP-ACTS-AS", ownerId);
            if (client.DefaultRequestHeaders.TryGetValues("MS-APP-ACTS-AS", out var values))
            {
                Console.WriteLine($"MS-APP-ACTS-AS header value: {string.Join(", ", values)}");
            }
            var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/teams/{teamId}/schedule/schedulingGroups");
            client.DefaultRequestHeaders.Remove("MS-APP-ACTS-AS");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return JsonSerializer.Deserialize<JsonElement>("{\"value\":[]}");
            }
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }
    }
} 