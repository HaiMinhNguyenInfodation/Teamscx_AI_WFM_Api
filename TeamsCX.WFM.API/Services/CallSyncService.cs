using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Services
{
    public class CallSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly GraphQLCallService _graphQLCallService;
        private readonly ILogger<CallSyncService> _logger;

        public CallSyncService(
            ApplicationDbContext context,
            GraphQLCallService graphQLCallService,
            ILogger<CallSyncService> logger)
        {
            _context = context;
            _graphQLCallService = graphQLCallService;
            _logger = logger;
        }

        public async Task SyncRecentCallsAsync(string resourceAccounts)
        {
            var from = DateTime.UtcNow.AddMinutes(-10);
            var to = DateTime.UtcNow;

            await SyncCallsAsync(from, to, resourceAccounts);
        }

        public async Task SyncHistoricalCallsAsync(string resourceAccounts)
        {
            var from = DateTime.UtcNow.AddMonths(-3);
            var to = DateTime.UtcNow;

            await SyncCallsAsync(from, to, resourceAccounts);
        }

        private async Task SyncCallsAsync(DateTime from, DateTime to, string resourceAccounts)
        {
            try
            {
                var response = await _graphQLCallService.GetCallDetailsAsync(from, to, resourceAccounts);
                if (response?.Data?.CallDetails == null)
                {
                    _logger.LogWarning("No call details received from GraphQL service");
                    return;
                }

                foreach (var callDetail in response.Data.CallDetails)
                {
                    await ProcessCallDetailAsync(callDetail);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing calls from {From} to {To}", from, to);
                throw;
            }
        }

        public async Task ProcessCallDetailAsync(CallDetail callDetail)
        {
            // Check if call already exists
            var existingCall = await _context.Calls
                .FirstOrDefaultAsync(c => c.CallId == callDetail.Id);

            if (existingCall != null)
            {
                await UpdateExistingCallAsync(existingCall, callDetail);
            }
            else
            {
                await CreateNewCallAsync(callDetail);
            }
        }

        private async Task UpdateExistingCallAsync(Calls existingCall, CallDetail callDetail)
        {
            // Update basic call information
            existingCall.LastUpdated = DateTime.UtcNow;
            existingCall.WaitingDuration = callDetail.WaitingDuration;
            existingCall.ConnectedDuration = callDetail.AnswerDuration;
            existingCall.CallDuration = callDetail.CallDuration;
            existingCall.Hunted = !string.IsNullOrEmpty(callDetail.HuntedUser);
            existingCall.Connected = !string.IsNullOrEmpty(callDetail.ConnectedUser);

            // Update status and outcome
            existingCall.CallStatusId = await GetOrCreateCallStatusIdAsync(callDetail.StatusLive);
            existingCall.CallOutcomeId = await GetOrCreateCallOutcomeIdAsync(callDetail.StatusEnd);

            // Update connected users
            await UpdateConnectedUsersAsync(existingCall, callDetail);
            await UpdateHuntedUsersAsync(existingCall, callDetail);
            await UpdateCallQueuesAsync(existingCall, callDetail);
        }

        private async Task CreateNewCallAsync(CallDetail callDetail)
        {
            var call = new Calls
            {
                CallId = callDetail.Id,
                StartedAt = callDetail.StartTime,
                LastUpdated = DateTime.UtcNow,
                WaitingDuration = callDetail.WaitingDuration,
                ConnectedDuration = callDetail.AnswerDuration,
                CallDuration = callDetail.CallDuration,
                Hunted = !string.IsNullOrEmpty(callDetail.HuntedUser),
                Connected = !string.IsNullOrEmpty(callDetail.ConnectedUser),
                IsForceEnded = false,
                CallStatusId = await GetOrCreateCallStatusIdAsync(callDetail.StatusLive),
                CallOutcomeId = await GetOrCreateCallOutcomeIdAsync(callDetail.StatusEnd),
                CallDirectionId = await GetOrCreateCallDirectionIdAsync(callDetail.Direction),
                Caller = await GetOrCreateCallerAsync(callDetail)
            };

            _context.Calls.Add(call);
            await _context.SaveChangesAsync();

            await UpdateConnectedUsersAsync(call, callDetail);
            await UpdateHuntedUsersAsync(call, callDetail);
            await UpdateCallQueuesAsync(call, callDetail);
        }

        private async Task<int> GetOrCreateCallStatusIdAsync(string status)
        {
            var callStatus = await _context.CallStatuses
                .FirstOrDefaultAsync(cs => cs.Status == status);

            if (callStatus == null)
            {
                callStatus = new CallStatus { Status = status };
                _context.CallStatuses.Add(callStatus);
                await _context.SaveChangesAsync();
            }

            return callStatus.Id;
        }

        private async Task<int> GetOrCreateCallOutcomeIdAsync(string outcome)
        {
            var callOutcome = await _context.CallOutcomes
                .FirstOrDefaultAsync(co => co.Outcome == outcome);

            if (callOutcome == null)
            {
                callOutcome = new CallOutcome { Outcome = outcome };
                _context.CallOutcomes.Add(callOutcome);
                await _context.SaveChangesAsync();
            }

            return callOutcome.Id;
        }

        private async Task<int> GetOrCreateCallDirectionIdAsync(string direction)
        {
            var callDirection = await _context.CallDirections
                .FirstOrDefaultAsync(cd => cd.Direction == direction);

            if (callDirection == null)
            {
                callDirection = new CallDirection { Direction = direction };
                _context.CallDirections.Add(callDirection);
                await _context.SaveChangesAsync();
            }

            return callDirection.Id;
        }

        private async Task<Caller> GetOrCreateCallerAsync(CallDetail callDetail)
        {
            var caller = await _context.Callers
                .FirstOrDefaultAsync(c => c.CallerPhoneNumber == callDetail.Caller);

            if (caller == null)
            {
                caller = new Caller
                {
                    CallerPhoneNumber = callDetail.Caller,
                    CallerName = callDetail.CallerName,
                    CallerCompany = callDetail.CompanyName
                };
                _context.Callers.Add(caller);
                await _context.SaveChangesAsync();
            }

            return caller;
        }

        private async Task UpdateConnectedUsersAsync(Calls call, CallDetail callDetail)
        {
            if (string.IsNullOrEmpty(callDetail.ConnectedUser))
                return;

            var connectedUsers = callDetail.ConnectedUser.Split(',');
            foreach (var user in connectedUsers)
            {
                var agent = await _context.Agents
                    .FirstOrDefaultAsync(a => a.DisplayName == user.Trim());

                if (agent != null)
                {
                    var connectedUser = new CallConnectedUser
                    {
                        CallId = call.Id,
                        AgentId = agent.Id,
                        StartTime = call.StartedAt,
                        EndTime = callDetail.EndTime
                    };
                    _context.CallConnectedUsers.Add(connectedUser);
                }
            }
        }

        private async Task UpdateHuntedUsersAsync(Calls call, CallDetail callDetail)
        {
            if (string.IsNullOrEmpty(callDetail.HuntedUser))
                return;

            var huntedUsers = callDetail.HuntedUser.Split(',');
            foreach (var user in huntedUsers)
            {
                var agent = await _context.Agents
                    .FirstOrDefaultAsync(a => a.DisplayName == user.Trim());

                if (agent != null)
                {
                    var huntedUser = new CallHuntedUser
                    {
                        CallId = call.Id,
                        AgentId = agent.Id,
                        StartTime = call.StartedAt,
                        EndTime = callDetail.EndTime
                    };
                    _context.CallHuntedUsers.Add(huntedUser);
                }
            }
        }

        private async Task UpdateCallQueuesAsync(Calls call, CallDetail callDetail)
        {
            if (string.IsNullOrEmpty(callDetail.CallQueues))
                return;

            var queues = callDetail.CallQueues.Split(',');
            foreach (var queueName in queues)
            {
                var queue = await _context.Queues
                    .FirstOrDefaultAsync(q => q.Name == queueName.Trim());

                if (queue != null)
                {
                    var callQueue = new CallQueueReported
                    {
                        CallId = call.Id,
                        QueueId = queue.Id,
                        AccessTime = call.StartedAt
                    };
                    _context.CallQueues.Add(callQueue);
                }
            }
        }
    }
}