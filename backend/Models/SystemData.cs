namespace SysScore.Models
{
    public class SystemData
    {
        public int Id { get; set; }

        public double CpuUsage { get; set; }

        public double RamUsage { get; set; }

        public double DiskUsage { get; set; }

        public double SwapUsage { get; set; }

        public double DiskFreeGb { get; set; }

        public int ProcessCount { get; set; }

        public int HighCpuProcessCount { get; set; }

        public int HighMemoryProcessCount { get; set; }

        public int NetworkConnectionCount { get; set; }

        public int ListeningPortCount { get; set; }

        public int EstablishedConnectionCount { get; set; }

        public int SynSentConnectionCount { get; set; }

        public int TimeWaitConnectionCount { get; set; }

        public int UniqueRemoteAddressCount { get; set; }

        public int UniqueRemotePortCount { get; set; }

        public int NetworkConnectionDelta { get; set; }

        public double OutboundPacketRate { get; set; }

        public double InboundPacketRate { get; set; }

        public long SystemUptimeSeconds { get; set; }

        public DateTime BootTime { get; set; }

        public int UnnecessaryFileCount { get; set; }

        public double UnnecessaryFileSizeMb { get; set; }

        public string? UnnecessaryFileLocations { get; set; }

        public string? LargestUnnecessaryFiles { get; set; }

        public DateTime Timestamp { get; set; }

        public int SecurityScore { get; set; }

        public string? Explanation { get; set; }

        public string? ThreatType { get; set; }

        public string? ThreatLevel { get; set; }

        public int ThreatScore { get; set; }

        public string? ThreatEvidence { get; set; }

        public string? RecommendedActions { get; set; }

        public DateTime? ThreatDetectedAt { get; set; }
    }
}
