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
                CalculateTrendPenalty(data, previousData);

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
