using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Services
{
    public interface IGraphQLCallService
    {
        Task<List<Call>> GetCallDetailsAsync(DateTime from, DateTime to, string resourceAccounts);
    }

    public class GraphQLCallService : IGraphQLCallService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://tcx-teamsv2-demo-datasource.azurewebsites.net/api/graphql";

        public GraphQLCallService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Call>> GetCallDetailsAsync(DateTime from, DateTime to, string resourceAccounts)
        {
            var query = new
            {
                query = $@"{{
                    callDetails(
                        from: ""{from:yyyy-MM-ddTHH:mm:ss.fffZ}""
                        to: ""{to:yyyy-MM-ddTHH:mm:ss.fffZ}""
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
                        autoAttendants
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
                "application/json"
            );

            var response = await _httpClient.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GraphQLResponse>(responseContent);

            return MapToCalls(result.Data.CallDetails);
        }

        private List<Call> MapToCalls(List<GraphQLCallDetail> details)
        {
            return details.Select(detail => new Call
            {
                Id = detail.Id,
                Direction = Enum.Parse<Direction>(detail.Direction),
                CallStatus = Enum.Parse<CallStatus>(detail.StatusLive),
                CallOutcome = Enum.Parse<CallOutcome>(detail.StatusEnd),
                StartedAt = DateTime.Parse(detail.StartTime),
                LastUpdated = DateTime.Parse(detail.EndTime),
                WaitingDuration = detail.WaitingDuration,
                ConnectedDuration = detail.AnswerDuration,
                CallDuration = detail.CallDuration,
                CallerId = detail.Caller,
                CallerName = detail.CallerName,
                CallerCompany = detail.CompanyName,
                CallQueues = new List<string> { detail.CallQueues },
                AutoAttendants = new List<string> { detail.AutoAttendants },
                CallUsers = new List<CallUser>
                {
                    // Add connected user if exists
                    !string.IsNullOrEmpty(detail.ConnectedUser) ? new CallUser
                    {
                        UserName = detail.ConnectedUser,
                        IsConnected = true,
                        IsHunted = false
                    } : null,
                    // Add hunted user if exists
                    !string.IsNullOrEmpty(detail.HuntedUser) ? new CallUser
                    {
                        UserName = detail.HuntedUser,
                        IsConnected = false,
                        IsHunted = true
                    } : null
                }.Where(cu => cu != null).ToList()
            }).ToList();
        }
    }

    public class GraphQLResponse
    {
        public GraphQLData Data { get; set; }
    }

    public class GraphQLData
    {
        public List<GraphQLCallDetail> CallDetails { get; set; }
    }

    public class GraphQLCallDetail
    {
        public string Id { get; set; }
        public string Caller { get; set; }
        public string CallerName { get; set; }
        public string CompanyName { get; set; }
        public string StartTime { get; set; }
        public string StatusEnd { get; set; }
        public string StatusLive { get; set; }
        public string CallQueues { get; set; }
        public string AutoAttendants { get; set; }
        public string ResourceAccounts { get; set; }
        public double WaitingDuration { get; set; }
        public double AnswerDuration { get; set; }
        public double CallDuration { get; set; }
        public string HuntedUser { get; set; }
        public string Callee { get; set; }
        public string EndTime { get; set; }
        public string Direction { get; set; }
        public string ConnectedUser { get; set; }
        public string CalleeName { get; set; }
        public string CalleeCompany { get; set; }
        public double FirstAcceptedTime { get; set; }
        public string Classification { get; set; }
    }
}