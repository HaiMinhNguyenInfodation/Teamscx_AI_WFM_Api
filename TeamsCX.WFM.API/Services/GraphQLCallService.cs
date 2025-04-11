using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeamsCX.WFM.API.Services
{
    public class GraphQLCallService
    {
        private readonly HttpClient _httpClient;
        private readonly string _graphqlEndpoint;
        private readonly ILogger<GraphQLCallService> _logger;

        public GraphQLCallService(HttpClient httpClient, string graphqlEndpoint, ILogger<GraphQLCallService> logger)
        {
            _httpClient = httpClient;
            _graphqlEndpoint = graphqlEndpoint;
            _logger = logger;
        }

        public async Task<GraphQLResponse> GetCallDetailsAsync(DateTime from, DateTime to, string resourceAccounts)
        {
            var fromString = from.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var toString = to.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var query = new
            {
                query = $@"
                    {{
                        callDetails(
                            from: ""{fromString}""
                            to: ""{toString}""
                            resourceAccounts: ""{resourceAccounts}""
                        ) {{
                            id
                            caller
                            callerName
                            companyName
                            startTime
                            statusEnd
                            statusLive
                            callQueues
                            resourceAccounts
                            waitingDuration
                            answerDuration
                            callDuration
                            huntedUser
                            callee
                            endTime
                            direction
                            connectedUser
                            calleeName
                            calleeCompany
                            firstAcceptedTime
                            classification
                        }}
                    }}"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(query),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_graphqlEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Response: {responseContent}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<GraphQLResponse>(responseContent, options);
            _logger.LogDebug($"Total call details: {result?.Data?.CallDetails?.Length ?? 0}");
            return result ?? new GraphQLResponse();
        }
    }

    public class GraphQLResponse
    {
        public GraphQLData Data { get; set; } = new GraphQLData();
    }

    public class GraphQLData
    {
        public CallDetail[] CallDetails { get; set; } = Array.Empty<CallDetail>();
    }

    public class CallDetail
    {
        public string Id { get; set; } = string.Empty;
        public string Caller { get; set; } = string.Empty;
        public string CallerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string StatusEnd { get; set; } = string.Empty;
        public string StatusLive { get; set; } = string.Empty;
        public string CallQueues { get; set; } = string.Empty;
        public string ResourceAccounts { get; set; } = string.Empty;
        public double WaitingDuration { get; set; }
        public double AnswerDuration { get; set; }
        public double CallDuration { get; set; }
        public string HuntedUser { get; set; } = string.Empty;
        public string Callee { get; set; } = string.Empty;
        public DateTime EndTime { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string ConnectedUser { get; set; } = string.Empty;
        public string CalleeName { get; set; } = string.Empty;
        public string CalleeCompany { get; set; } = string.Empty;
        public double FirstAcceptedTime { get; set; }
        public string Classification { get; set; } = string.Empty;
    }
}