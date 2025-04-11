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

        public GraphQLCallService(HttpClient httpClient, string graphqlEndpoint)
        {
            _httpClient = httpClient;
            _graphqlEndpoint = graphqlEndpoint;
        }

        public async Task<GraphQLResponse> GetCallDetailsAsync(DateTime from, DateTime to, string resourceAccounts)
        {
            var query = new
            {
                query = @"
                    query($from: String!, $to: String!, $resourceAccounts: String!) {
                        callDetails(from: $from, to: $to, resourceAccounts: $resourceAccounts) {
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
                        }
                    }",
                variables = new
                {
                    from = from.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    to = to.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    resourceAccounts
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(query),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_graphqlEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GraphQLResponse>(responseContent);
        }
    }

    public class GraphQLResponse
    {
        public GraphQLData Data { get; set; }
    }

    public class GraphQLData
    {
        public CallDetail[] CallDetails { get; set; }
    }

    public class CallDetail
    {
        public string Id { get; set; }
        public string Caller { get; set; }
        public string CallerName { get; set; }
        public string CompanyName { get; set; }
        public DateTime StartTime { get; set; }
        public string StatusEnd { get; set; }
        public string StatusLive { get; set; }
        public string CallQueues { get; set; }
        public string ResourceAccounts { get; set; }
        public double WaitingDuration { get; set; }
        public double AnswerDuration { get; set; }
        public double CallDuration { get; set; }
        public string HuntedUser { get; set; }
        public string Callee { get; set; }
        public DateTime EndTime { get; set; }
        public string Direction { get; set; }
        public string ConnectedUser { get; set; }
        public string CalleeName { get; set; }
        public string CalleeCompany { get; set; }
        public double FirstAcceptedTime { get; set; }
        public string Classification { get; set; }
    }
}