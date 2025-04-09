using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamsCX.WFM.API.Models.RealTime;
using TeamsCX.WFM.API.Services;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RealTimeController : ControllerBase
    {
        private readonly IRealTimeService _realTimeService;

        public RealTimeController(IRealTimeService realTimeService)
        {
            _realTimeService = realTimeService;
        }

        /// <summary>
        /// Get real-time overview of agents status and metrics for specified call queues
        /// </summary>
        /// <param name="callQueues">List of call queue IDs to filter by</param>
        /// <returns>Real-time overview including agent status, metrics, and details</returns>
        [HttpGet]
        public async Task<ActionResult<RealTimeOverview>> GetRealTimeOverview([FromQuery] List<string> callQueues)
        {
            if (callQueues == null || callQueues.Count == 0)
            {
                return BadRequest("At least one call queue must be specified");
            }

            var overview = await _realTimeService.GetRealTimeOverview(callQueues);
            return Ok(overview);
        }
    }
}