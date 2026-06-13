using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SysScore.Data;
using SysScore.Models;
using SysScore.Services;

namespace SysScore.Controllers
{
    [ApiController]
    [Route("api/system-data")]
    public class SystemController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly ScoreService scoreService;
        private readonly AIService aiService;
        private readonly ThreatDetectionService threatDetectionService;

        public SystemController(
            AppDbContext dbContext,
            ScoreService scoreService,
            AIService aiService,
            ThreatDetectionService threatDetectionService)
        {
            this.dbContext = dbContext;
            this.scoreService = scoreService;
            this.aiService = aiService;
            this.threatDetectionService = threatDetectionService;
        }

        [HttpPost]
        public async Task<ActionResult<SystemData>> Create(SystemData systemData)
        {
            systemData.Timestamp = systemData.Timestamp == default
                ? DateTime.UtcNow
                : systemData.Timestamp;
            SystemData? previousData = await dbContext.SystemDataRecords
                .OrderByDescending(data => data.Timestamp)
                .FirstOrDefaultAsync();
            threatDetectionService.Analyze(systemData, previousData);
            systemData.SecurityScore = scoreService.CalculateScore(systemData, previousData);
            systemData.Explanation = await aiService.GenerateExplanationAsync(systemData, previousData);

            dbContext.SystemDataRecords.Add(systemData);
            await dbContext.SaveChangesAsync();

            return Ok(systemData);
        }

        [HttpGet("latest")]
        public async Task<ActionResult<SystemData>> GetLatest()
        {
            SystemData? latestData = await dbContext.SystemDataRecords
                .OrderByDescending(data => data.Timestamp)
                .FirstOrDefaultAsync();

            if (latestData is null)
            {
                return NotFound();
            }

            return Ok(latestData);
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<SystemData>>> GetHistory()
        {
            List<SystemData> history = await dbContext.SystemDataRecords
                .OrderByDescending(data => data.Timestamp)
                .ToListAsync();

            return Ok(history);
        }
    }
}
