using Microsoft.AspNetCore.Mvc;
using TeamsCX.WFM.API.Models;
using TeamsCX.WFM.API.Services;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpToNowController : ControllerBase
    {
        private readonly IUpToNowService _upToNowService;

        public UpToNowController(IUpToNowService upToNowService)
        {
            _upToNowService = upToNowService;
        }

        [HttpGet]
        public async Task<ActionResult<UpToNowResponse>> GetUpToNowData([FromQuery] List<string> callQueues)
        {
            if (callQueues == null || !callQueues.Any())
            {
                return BadRequest("At least one call queue must be specified");
            }

            var response = await _upToNowService.GetUpToNowDataAsync(callQueues);
            return Ok(response);
        }
    }
}