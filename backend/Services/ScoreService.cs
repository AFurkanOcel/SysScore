using SysScore.Models;

namespace SysScore.Services
{
    public class ScoreService
    {
        public int CalculateScore(SystemData data)
        {
            // basit skor: ne kadar düşük kullanım o kadar iyi
            int score = 100 - (int)((data.CpuUsage + data.RamUsage) / 2);
            return score;
        }
    }
}