using SysScore.Models;

namespace SysScore.Services
{
    public class ScoreService
    {
        public int CalculateScore(SystemData data)
        {
            double averageUsage = (data.CpuUsage + data.RamUsage + data.DiskUsage) / 3;
            int score = 100 - (int)Math.Round(averageUsage);

            return Math.Clamp(score, 0, 100);
        }
    }
}
