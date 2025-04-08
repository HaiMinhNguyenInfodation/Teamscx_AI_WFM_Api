using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeamsCX.WFM.API.Services;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly SyncService _syncService;

        public SyncController(SyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncData()
        {
            try
            {
                await _syncService.SyncAllDataAsync();
                return Ok(new { message = "Data sync completed successfully" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error during data sync", error = ex.Message });
            }
        }
    }
} 