using System.Text.Json.Serialization;

namespace TeamsCX.WFM.API.Models.GraphQL
{
    public class GraphQLResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class CallDetailsData
    {
        [JsonPropertyName("callDetails")]
        public List<CallDetail> CallDetails { get; set; }
    }

    public class CallDetail
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("statusEnd")]
        public string StatusEnd { get; set; }

        [JsonPropertyName("waitingDuration")]
        public double WaitingDuration { get; set; }

        [JsonPropertyName("answerDuration")]
        public double AnswerDuration { get; set; }

        [JsonPropertyName("callDuration")]
        public double CallDuration { get; set; }
    }
}