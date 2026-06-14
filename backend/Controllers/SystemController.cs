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
            if (HasThreat(systemData))
            {
                dbContext.ThreatEvents.Add(CreateThreatEvent(systemData));
            }

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

        private static bool HasThreat(SystemData systemData)
        {
            return !string.Equals(systemData.ThreatLevel, "None", StringComparison.OrdinalIgnoreCase) &&
                systemData.ThreatScore > 0;
        }

        private static ThreatEvent CreateThreatEvent(SystemData systemData)
        {
            return new ThreatEvent
            {
                SystemData = systemData,
                ThreatType = systemData.ThreatType ?? "Unknown Threat",
                ThreatLevel = systemData.ThreatLevel ?? "Unknown",
                ThreatScore = systemData.ThreatScore,
                Evidence = systemData.ThreatEvidence ?? string.Empty,
                RecommendedActions = systemData.RecommendedActions ?? string.Empty,
                Status = GetThreatEventStatus(systemData.ThreatLevel),
                DetectedAt = systemData.ThreatDetectedAt ?? systemData.Timestamp
            };
        }

        private static string GetThreatEventStatus(string? threatLevel)
        {
            return threatLevel?.ToLowerInvariant() switch
            {
                "critical" => "Admin Action Required",
                "high" => "Review Recommended",
                _ => "Detection Recorded"
            };
        }
    }
}
