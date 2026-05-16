using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SysScore.Models;

namespace SysScore.Services
{
    public class AIService
    {
        private const double HighCpuThreshold = 80;
        private const double HighRamThreshold = 80;
        private const double HighDiskThreshold = 85;
        private const double HighSwapThreshold = 25;
        private const int HighProcessThreshold = 300;
        private const int HighListeningPortThreshold = 12;
        private const int HighConnectionThreshold = 120;
        private const double HighUnnecessaryFileSizeMbThreshold = 1024;
        private const int ScoreDropThreshold = 10;
        private const int MaxExplanationLength = 1000;

        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;

        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
        }

        public async Task<string> GenerateExplanationAsync(SystemData currentData, SystemData? previousData)
        {
            string fallbackExplanation = GenerateFallbackExplanation(currentData, previousData);

            if (!configuration.GetValue<bool>("AI:UseOllama"))
            {
                return fallbackExplanation;
            }

            try
            {
                using CancellationTokenSource timeout = new(GetOllamaTimeout());
                string? ollamaExplanation = await GenerateOllamaExplanationAsync(
                    currentData,
                    previousData,
                    fallbackExplanation,
                    timeout.Token);

                return LimitExplanationLength(string.IsNullOrWhiteSpace(ollamaExplanation)
                    ? fallbackExplanation
                    : ollamaExplanation);
            }
            catch
            {
                return fallbackExplanation;
            }
        }

        private string GenerateFallbackExplanation(SystemData currentData, SystemData? previousData)
        {
            List<string> findings = new()
            {
                GetSeverityLead(currentData.SecurityScore)
            };

            if (previousData is not null &&
                previousData.SecurityScore - currentData.SecurityScore >= ScoreDropThreshold)
            {
                findings.Add("Security score decreased compared with the previous record, mainly due to increased resource pressure.");
            }

            if (currentData.RamUsage >= HighRamThreshold && currentData.SwapUsage >= HighSwapThreshold)
            {
                findings.Add("Combined RAM and swap pressure suggests sustained memory stress rather than a short resource spike.");
            }

            if (currentData.ListeningPortCount >= HighListeningPortThreshold &&
                currentData.NetworkConnectionCount >= HighConnectionThreshold)
            {
                findings.Add("Network exposure is elevated because listening ports and active connections are both above the expected baseline.");
            }

            if (currentData.HighCpuProcessCount > 0 && currentData.HighMemoryProcessCount > 0)
            {
                findings.Add("Process-level anomaly is present because high CPU and high memory consumers are active at the same time.");
            }

            if (previousData is not null &&
                currentData.UnnecessaryFileSizeMb >= HighUnnecessaryFileSizeMbThreshold &&
                previousData.UnnecessaryFileSizeMb >= HighUnnecessaryFileSizeMbThreshold)
            {
                findings.Add("Storage hygiene risk appears persistent because temporary, cache, or trash file size remains high across records.");
            }

            if (previousData is not null &&
                currentData.ProcessCount >= HighProcessThreshold &&
                previousData.ProcessCount >= HighProcessThreshold)
            {
                findings.Add("Process count remains persistently high and should be reviewed for unnecessary background activity.");
            }

            if (currentData.CpuUsage >= HighCpuThreshold)
            {
                findings.Add("CPU usage is elevated, which may indicate heavy workload or a process consuming unusual resources.");
            }

            if (currentData.RamUsage >= HighRamThreshold)
            {
                findings.Add("Memory usage is high and may reduce system stability if it continues.");
            }

            if (currentData.DiskUsage >= HighDiskThreshold)
            {
                findings.Add("Disk usage is high; low free space can affect reliability and logging.");
            }

            if (currentData.SwapUsage >= HighSwapThreshold)
            {
                findings.Add("Swap usage is elevated, which may indicate memory pressure beyond normal RAM usage.");
            }

            if (currentData.ProcessCount >= HighProcessThreshold)
            {
                findings.Add("Process count is above the expected baseline and should be reviewed.");
            }

            if (currentData.HighCpuProcessCount > 0 || currentData.HighMemoryProcessCount > 0)
            {
                findings.Add("One or more processes are consuming unusually high CPU or memory resources.");
            }

            if (currentData.ListeningPortCount >= HighListeningPortThreshold)
            {
                findings.Add("The number of listening ports is higher than expected and exposed services should be reviewed.");
            }

            if (currentData.NetworkConnectionCount >= HighConnectionThreshold)
            {
                findings.Add("Network connection count is elevated, which may indicate heavy service activity.");
            }

            if (currentData.UnnecessaryFileSizeMb >= HighUnnecessaryFileSizeMbThreshold ||
                currentData.UnnecessaryFileCount >= 1000)
            {
                findings.Add("Temporary, cache, or trash files are accumulating and should be reviewed for storage hygiene.");
            }

            if (previousData is not null &&
                currentData.ListeningPortCount - previousData.ListeningPortCount >= 5)
            {
                findings.Add("Listening ports increased compared with the previous record.");
            }

            if (previousData is not null &&
                currentData.UnnecessaryFileSizeMb - previousData.UnnecessaryFileSizeMb >= 512)
            {
                findings.Add("Unnecessary file size increased noticeably compared with the previous record.");
            }

            return LimitExplanationLength(string.Join(" ", findings));
        }

        private static string GetSeverityLead(int securityScore)
        {
            if (securityScore >= 90)
            {
                return "Excellent posture: system indicators are healthy and no immediate risk pattern is detected.";
            }

            if (securityScore >= 75)
            {
                return "System appears stable: monitored indicators remain within acceptable ranges.";
            }

            if (securityScore >= 60)
            {
                return "Moderate risk detected: review the highlighted indicators before they become persistent.";
            }

            if (securityScore >= 40)
            {
                return "High risk indicators are present: prioritize investigation of the affected areas.";
            }

            return "Critical system pressure detected: immediate review is recommended.";
        }

        private async Task<string?> GenerateOllamaExplanationAsync(
            SystemData currentData,
            SystemData? previousData,
            string fallbackExplanation,
            CancellationToken cancellationToken)
        {
            string ollamaUrl = configuration.GetValue<string>("AI:OllamaUrl")
                ?? "http://localhost:11434/api/generate";
            string ollamaModel = configuration.GetValue<string>("AI:OllamaModel")
                ?? "llama3.2";

            OllamaRequest request = new(
                ollamaModel,
                BuildPrompt(currentData, previousData, fallbackExplanation),
                false);

            HttpResponseMessage httpResponse = await httpClient.PostAsJsonAsync(
                ollamaUrl,
                request,
                cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            OllamaResponse? response = await httpResponse.Content
                .ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);

            return response?.Response?.Trim();
        }

        private string BuildPrompt(SystemData currentData, SystemData? previousData, string fallbackExplanation)
        {
            string previousScore = previousData is null
                ? "not available"
                : previousData.SecurityScore.ToString();

            return $"""
                You are explaining a Linux security monitoring score to an administrator.
                Keep the answer under 70 words, professional, clear, and action-oriented.
                Current metrics:
                CPU: {currentData.CpuUsage:F1}%
                RAM: {currentData.RamUsage:F1}%
                Disk: {currentData.DiskUsage:F1}%
                Swap: {currentData.SwapUsage:F1}%
                Process count: {currentData.ProcessCount}
                High CPU processes: {currentData.HighCpuProcessCount}
                High memory processes: {currentData.HighMemoryProcessCount}
                Listening ports: {currentData.ListeningPortCount}
                Network connections: {currentData.NetworkConnectionCount}
                Unnecessary file count: {currentData.UnnecessaryFileCount}
                Unnecessary file size MB: {currentData.UnnecessaryFileSizeMb:F1}
                Security score: {currentData.SecurityScore}
                Previous score: {previousScore}
                Deterministic fallback analysis: {fallbackExplanation}
                """;
        }

        private TimeSpan GetOllamaTimeout()
        {
            int timeoutSeconds = configuration.GetValue("AI:TimeoutSeconds", 3);

            return TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
        }

        private static string LimitExplanationLength(string explanation)
        {
            if (explanation.Length <= MaxExplanationLength)
            {
                return explanation;
            }

            return explanation[..(MaxExplanationLength - 3)].TrimEnd() + "...";
        }

        private sealed record OllamaRequest(
            [property: JsonPropertyName("model")] string Model,
            [property: JsonPropertyName("prompt")] string Prompt,
            [property: JsonPropertyName("stream")] bool Stream);

        private sealed record OllamaResponse(
            [property: JsonPropertyName("response")] string? Response);
    }
}
