using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SysScore.Data;
using SysScore.Models;

namespace SysScore.Controllers
{
    [ApiController]
    [Route("api/threat-events")]
    public class ThreatEventsController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public ThreatEventsController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<ThreatEvent>>> GetRecent([FromQuery] int limit = 20)
        {
            int safeLimit = Math.Clamp(limit, 1, 100);

            List<ThreatEvent> threatEvents = await dbContext.ThreatEvents
                .AsNoTracking()
                .OrderByDescending(threatEvent => threatEvent.DetectedAt)
                .ThenByDescending(threatEvent => threatEvent.Id)
                .Take(safeLimit)
                .ToListAsync();

            return Ok(threatEvents);
        }
    }
}
