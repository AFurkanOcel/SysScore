using SysScore.Models;

namespace SysScore.Services
{
    public class ScoreService
    {
        public int CalculateScore(SystemData data, SystemData? previousData = null)
        {
            double penalty =
                CalculateResourcePenalty(data) +
                CalculateProcessPenalty(data) +
                CalculateNetworkPenalty(data) +
                CalculateStorageHygienePenalty(data) +
                CalculateTrendPenalty(data, previousData) +
                CalculateCompoundRiskPenalty(data) +
                CalculatePersistentRiskPenalty(data, previousData) +
                CalculateThreatPenalty(data);

            penalty = Math.Max(0, penalty - CalculateStabilityBonus(data, previousData));

            int score = 100 - (int)Math.Round(penalty);
            return Math.Clamp(score, 0, 100);
        }

        private static double CalculateResourcePenalty(SystemData data)
        {
            double penalty = 0;

            penalty += ThresholdPenalty(data.CpuUsage, 70, 95, 5);
            penalty += ThresholdPenalty(data.RamUsage, 65, 95, 8);
            penalty += ThresholdPenalty(data.DiskUsage, 75, 95, 7);
            penalty += ThresholdPenalty(data.SwapUsage, 10, 80, 5);

            return Math.Min(penalty, 25);
        }

        private static double CalculateProcessPenalty(SystemData data)
        {
            double penalty = 0;

            penalty += ThresholdPenalty(data.ProcessCount, 250, 600, 7);
            penalty += ThresholdPenalty(data.HighCpuProcessCount, 1, 8, 6);
            penalty += ThresholdPenalty(data.HighMemoryProcessCount, 1, 8, 7);

            return Math.Min(penalty, 20);
        }

        private static double CalculateNetworkPenalty(SystemData data)
        {
            double penalty = 0;

            penalty += ThresholdPenalty(data.ListeningPortCount, 8, 35, 8);
            penalty += ThresholdPenalty(data.NetworkConnectionCount, 80, 300, 7);

            return Math.Min(penalty, 15);
        }

        private static double CalculateStorageHygienePenalty(SystemData data)
        {
            double penalty = 0;

            penalty += ThresholdPenalty(data.UnnecessaryFileCount, 500, 5000, 6);
            penalty += ThresholdPenalty(data.UnnecessaryFileSizeMb, 512, 8192, 9);

            return Math.Min(penalty, 15);
        }

        private static double CalculateTrendPenalty(SystemData data, SystemData? previousData)
        {
            if (previousData is null)
            {
                return 0;
            }

            double penalty = 0;

            if (data.RamUsage - previousData.RamUsage >= 15)
            {
                penalty += 2.5;
            }

            if (data.ProcessCount - previousData.ProcessCount >= 75)
            {
                penalty += 2.5;
            }

            if (data.ListeningPortCount - previousData.ListeningPortCount >= 5)
            {
                penalty += 2.5;
            }

            if (data.UnnecessaryFileSizeMb - previousData.UnnecessaryFileSizeMb >= 512)
            {
                penalty += 2.5;
            }

            return Math.Min(penalty, 10);
        }

        private static double CalculateCompoundRiskPenalty(SystemData data)
        {
            double penalty = 0;

            if (data.RamUsage >= 80 && data.SwapUsage >= 25)
            {
                penalty += 4;
            }

            if (data.ListeningPortCount >= 12 && data.NetworkConnectionCount >= 120)
            {
                penalty += 3;
            }

            if (data.HighCpuProcessCount > 0 && data.HighMemoryProcessCount > 0)
            {
                penalty += 3;
            }

            return Math.Min(penalty, 10);
        }

        private static double CalculatePersistentRiskPenalty(SystemData data, SystemData? previousData)
        {
            if (previousData is null)
            {
                return 0;
            }

            double penalty = 0;

            if (data.RamUsage >= 80 && previousData.RamUsage >= 80)
            {
                penalty += 2;
            }

            if (data.SwapUsage >= 25 && previousData.SwapUsage >= 25)
            {
                penalty += 2;
            }

            if (data.ListeningPortCount >= 12 && previousData.ListeningPortCount >= 12)
            {
                penalty += 2;
            }

            if (data.UnnecessaryFileSizeMb >= 1024 && previousData.UnnecessaryFileSizeMb >= 1024)
            {
                penalty += 2;
            }

            return Math.Min(penalty, 8);
        }

        private static double CalculateStabilityBonus(SystemData data, SystemData? previousData)
        {
            bool metricsAreStable =
                data.CpuUsage < 70 &&
                data.RamUsage < 65 &&
                data.DiskUsage < 75 &&
                data.SwapUsage < 10 &&
                data.ProcessCount < 250 &&
                data.HighCpuProcessCount == 0 &&
                data.HighMemoryProcessCount == 0 &&
                data.ListeningPortCount <= 8 &&
                data.NetworkConnectionCount < 80 &&
                data.UnnecessaryFileSizeMb < 512 &&
                data.ThreatScore < 15;

            if (!metricsAreStable)
            {
                return 0;
            }

            if (previousData is null)
            {
                return 3;
            }

            bool noMeaningfulRegression =
                data.RamUsage - previousData.RamUsage < 5 &&
                data.ProcessCount - previousData.ProcessCount < 25 &&
                data.ListeningPortCount - previousData.ListeningPortCount < 2 &&
                data.UnnecessaryFileSizeMb - previousData.UnnecessaryFileSizeMb < 128;

            return noMeaningfulRegression ? 5 : 2;
        }

        private static double CalculateThreatPenalty(SystemData data)
        {
            return data.ThreatLevel switch
            {
                "Critical" => 25,
                "High" => 18,
                "Medium" => 10,
                "Low" => 4,
                _ => ThresholdPenalty(data.ThreatScore, 15, 80, 20)
            };
        }

        private static double ThresholdPenalty(double value, double warningThreshold, double criticalThreshold, double maxPenalty)
        {
            if (value <= warningThreshold)
            {
                return 0;
            }

            if (value >= criticalThreshold)
            {
                return maxPenalty;
            }

            double ratio = (value - warningThreshold) / (criticalThreshold - warningThreshold);
            return ratio * maxPenalty;
        }
    }
}
