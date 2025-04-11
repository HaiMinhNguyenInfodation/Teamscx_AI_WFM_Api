using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Threading;
using TeamsCX.WFM.API.Services;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IQueueReportedAgentService _queueReportedAgentService;

        public JobsController(IQueueReportedAgentService queueReportedAgentService)
        {
            _queueReportedAgentService = queueReportedAgentService;
        }

        /// <summary>
        /// Manually triggers the Queue Reported Agents synchronization job
        /// </summary>
        /// <returns>Status of the job execution</returns>
        [HttpGet("sync-reported-agents")]
        public async Task<IActionResult> SyncReportedAgents()
        {
            try
            {
                await _queueReportedAgentService.SyncReportedAgentsAsync();
                return Ok(new { message = "Queue reported agents synchronization completed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while syncing queue reported agents", error = ex.Message });
            }
        }
    }
}