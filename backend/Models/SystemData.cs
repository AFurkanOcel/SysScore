namespace SysScore.Models
{
    public class SystemData
    {
        public int Id { get; set; }

        public double CpuUsage { get; set; }

        public double RamUsage { get; set; }

        public double DiskUsage { get; set; }

        public int ProcessCount { get; set; }

        public DateTime Timestamp { get; set; }

        public int SecurityScore { get; set; }

        public string? Explanation { get; set; }
    }
}
