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
        private const int HighProcessThreshold = 300;
        private const int ScoreDropThreshold = 10;

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

                return string.IsNullOrWhiteSpace(ollamaExplanation)
                    ? fallbackExplanation
                    : ollamaExplanation;
            }
            catch
            {
                return fallbackExplanation;
            }
        }

        private string GenerateFallbackExplanation(SystemData currentData, SystemData? previousData)
        {
            List<string> findings = new();

            if (previousData is not null &&
                previousData.SecurityScore - currentData.SecurityScore >= ScoreDropThreshold)
            {
                findings.Add("Security score decreased compared with the previous record, mainly due to increased resource pressure.");
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

            if (currentData.ProcessCount >= HighProcessThreshold)
            {
                findings.Add("Process count is above the expected baseline and should be reviewed.");
            }

            if (findings.Count == 0)
            {
                findings.Add("System metrics are stable and no immediate resource-based risk is detected.");
            }

            return string.Join(" ", findings);
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
                Process count: {currentData.ProcessCount}
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

        private sealed record OllamaRequest(
            [property: JsonPropertyName("model")] string Model,
            [property: JsonPropertyName("prompt")] string Prompt,
            [property: JsonPropertyName("stream")] bool Stream);

        private sealed record OllamaResponse(
            [property: JsonPropertyName("response")] string? Response);
    }
}
