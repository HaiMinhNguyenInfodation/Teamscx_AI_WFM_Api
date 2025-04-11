using Microsoft.AspNetCore.Mvc;
using TeamsCX.WFM.API.Services;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallSyncController : ControllerBase
    {
        private readonly HistoricalCallSyncJob _historicalSyncJob;
        private readonly CallSyncService _callSyncService;
        private readonly RealTimeCallSyncJob _realTimeSyncJob;
        private readonly ILogger<CallSyncController> _logger;

        public CallSyncController(
            HistoricalCallSyncJob historicalSyncJob,
            CallSyncService callSyncService,
            RealTimeCallSyncJob realTimeSyncJob,
            ILogger<CallSyncController> logger)
        {
            _historicalSyncJob = historicalSyncJob;
            _realTimeSyncJob = realTimeSyncJob;
            _callSyncService = callSyncService;
            _logger = logger;
        }

        [HttpPost("historical")]
        public async Task<IActionResult> TriggerHistoricalSync()
        {
            try
            {
                _logger.LogInformation("Triggering historical call sync");
                //await _historicalSyncJob.TriggerSyncAsync();
                await _callSyncService.SyncHistoricalCallsAsync("US - CQ - Demo - Sales,US - CQ - Demo - Service,US - CQ - Demo - Support,US - CQ - Demo - Tech");
                return Ok(new { message = "Historical sync triggered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering historical sync");
                return StatusCode(500, new { error = "Failed to trigger historical sync" });
            }
        }

        [HttpPost("realtime")]
        public async Task<IActionResult> TriggerRealTimeSync()
        {
            try
            {
                _logger.LogInformation("Triggering real-time call sync");
                await _realTimeSyncJob.TriggerSyncAsync();
                return Ok(new { message = "Real-time sync triggered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering real-time sync");
                return StatusCode(500, new { error = "Failed to trigger real-time sync" });
            }
        }
    }
}