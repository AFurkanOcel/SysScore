using Microsoft.AspNetCore.Mvc;
using SysScore.Models;
using SysScore.Services;

namespace SysScore.Controllers
{
    [ApiController]
    [Route("api/system-data")]
    public class SystemController : ControllerBase
    {
        private static readonly List<SystemData> SystemDataHistory = new();
        private static int nextId = 1;

        private readonly ScoreService scoreService;

        public SystemController(ScoreService scoreService)
        {
            this.scoreService = scoreService;
        }

        [HttpPost]
        public ActionResult<SystemData> Create(SystemData systemData)
        {
            systemData.Id = nextId++;
            systemData.Timestamp = systemData.Timestamp == default
                ? DateTime.UtcNow
                : systemData.Timestamp;
            systemData.SecurityScore = scoreService.CalculateScore(systemData);

            SystemDataHistory.Add(systemData);

            return Ok(systemData);
        }

        [HttpGet("latest")]
        public ActionResult<SystemData> GetLatest()
        {
            SystemData? latestData = SystemDataHistory
                .OrderByDescending(data => data.Timestamp)
                .FirstOrDefault();

            if (latestData is null)
            {
                return NotFound();
            }

            return Ok(latestData);
        }

        [HttpGet("history")]
        public ActionResult<IEnumerable<SystemData>> GetHistory()
        {
            IEnumerable<SystemData> history = SystemDataHistory
                .OrderByDescending(data => data.Timestamp);

            return Ok(history);
        }
    }
}
