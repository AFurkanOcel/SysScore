namespace SysScore.Models
{
    public class ThreatEvent
    {
        public int Id { get; set; }

        public int SystemDataId { get; set; }

        public SystemData? SystemData { get; set; }

        public string ThreatType { get; set; } = string.Empty;

        public string ThreatLevel { get; set; } = string.Empty;

        public int ThreatScore { get; set; }

        public string Evidence { get; set; } = string.Empty;

        public string RecommendedActions { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime DetectedAt { get; set; }
    }
}
