using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamsCX.WFM.API.Data;
using TeamsCX.WFM.API.Models;

namespace TeamsCX.WFM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueuesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QueuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("call-queues")]
        public async Task<ActionResult<IEnumerable<object>>> GetCallQueues()
        {
            var queues = await _context.Queues
                .Select(q => new
                {
                    id = q.MicrosoftQueueId,
                    name = q.Name
                })
                .ToListAsync();

            return Ok(queues);
        }
    }
}