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
                findings.Add("Güvenlik skoru önceki kayda göre düştü; sistemde artan risk göstergeleri incelenmelidir.");
            }

            if (!string.Equals(currentData.ThreatLevel, "None", StringComparison.OrdinalIgnoreCase) &&
                currentData.ThreatScore > 0)
            {
                findings.Add($"{currentData.ThreatType} tespit edildi. Tehdit seviyesi: {TranslateThreatLevel(currentData.ThreatLevel)}, tehdit skoru: {currentData.ThreatScore}.");

                if (!string.IsNullOrWhiteSpace(currentData.ThreatEvidence))
                {
                    findings.Add($"Kanıtlar: {currentData.ThreatEvidence.Replace(" | ", " ")}");
                }

                if (!string.IsNullOrWhiteSpace(currentData.RecommendedActions))
                {
                    findings.Add($"Önerilen müdahale: {currentData.RecommendedActions.Replace(" | ", " ")}");
                }
            }

            if (currentData.RamUsage >= HighRamThreshold && currentData.SwapUsage >= HighSwapThreshold)
            {
                findings.Add("RAM ve swap kullanımının birlikte yükselmesi geçici bir yükten çok kalıcı bellek baskısına işaret edebilir.");
            }

            if (currentData.ListeningPortCount >= HighListeningPortThreshold &&
                currentData.NetworkConnectionCount >= HighConnectionThreshold)
            {
                findings.Add("Dinleyen port ve aktif bağlantı sayıları birlikte yüksek olduğu için ağ yüzeyi gözden geçirilmelidir.");
            }

            if (currentData.HighCpuProcessCount > 0 && currentData.HighMemoryProcessCount > 0)
            {
                findings.Add("Yüksek CPU ve yüksek bellek tüketen process'ler aynı anda görüldüğü için process davranışı incelenmelidir.");
            }

            if (previousData is not null &&
                currentData.UnnecessaryFileSizeMb >= HighUnnecessaryFileSizeMbThreshold &&
                previousData.UnnecessaryFileSizeMb >= HighUnnecessaryFileSizeMbThreshold)
            {
                findings.Add("Geçici, cache veya çöp dosyalarının boyutu ardışık kayıtlarda yüksek kaldığı için storage hygiene riski süreklilik gösteriyor.");
            }

            if (previousData is not null &&
                currentData.ProcessCount >= HighProcessThreshold &&
                previousData.ProcessCount >= HighProcessThreshold)
            {
                findings.Add("Process sayısı ardışık kayıtlarda yüksek kaldığı için gereksiz arka plan süreçleri kontrol edilmelidir.");
            }

            if (currentData.CpuUsage >= HighCpuThreshold)
            {
                findings.Add("CPU kullanımı yüksek; yoğun iş yükü veya olağandışı kaynak tüketen bir process olabilir.");
            }

            if (currentData.RamUsage >= HighRamThreshold)
            {
                findings.Add("Bellek kullanımı yüksek ve devam ederse sistem kararlılığını azaltabilir.");
            }

            if (currentData.DiskUsage >= HighDiskThreshold)
            {
                findings.Add("Disk kullanımı yüksek; düşük boş alan loglama ve sistem güvenilirliğini etkileyebilir.");
            }

            if (currentData.SwapUsage >= HighSwapThreshold)
            {
                findings.Add("Swap kullanımı yüksek; bu durum RAM baskısının normal seviyeyi aştığını gösterebilir.");
            }

            if (currentData.ProcessCount >= HighProcessThreshold)
            {
                findings.Add("Process sayısı beklenen seviyenin üzerinde ve incelenmelidir.");
            }

            if (currentData.HighCpuProcessCount > 0 || currentData.HighMemoryProcessCount > 0)
            {
                findings.Add("Bir veya daha fazla process olağandışı CPU ya da bellek kullanıyor.");
            }

            if (currentData.ListeningPortCount >= HighListeningPortThreshold)
            {
                findings.Add("Dinleyen port sayısı beklenenden yüksek; dışa açık servisler kontrol edilmelidir.");
            }

            if (currentData.NetworkConnectionCount >= HighConnectionThreshold)
            {
                findings.Add("Ağ bağlantı sayısı yüksek; yoğun servis aktivitesi veya şüpheli bağlantı davranışı olabilir.");
            }

            if (currentData.UnnecessaryFileSizeMb >= HighUnnecessaryFileSizeMbThreshold ||
                currentData.UnnecessaryFileCount >= 1000)
            {
                findings.Add("Geçici, cache veya çöp dosyaları birikiyor; storage hygiene açısından gözden geçirilmelidir.");
            }

            if (previousData is not null &&
                currentData.ListeningPortCount - previousData.ListeningPortCount >= 5)
            {
                findings.Add("Dinleyen port sayısı önceki kayda göre arttı.");
            }

            if (previousData is not null &&
                currentData.UnnecessaryFileSizeMb - previousData.UnnecessaryFileSizeMb >= 512)
            {
                findings.Add("Gereksiz dosya boyutu önceki kayda göre belirgin şekilde arttı.");
            }

            return LimitExplanationLength(string.Join(" ", findings));
        }

        private static string GetSeverityLead(int securityScore)
        {
            if (securityScore >= 90)
            {
                return "Mükemmel durum: sistem göstergeleri sağlıklı ve acil bir risk paterni tespit edilmedi.";
            }

            if (securityScore >= 75)
            {
                return "Sistem stabil görünüyor: izlenen göstergeler kabul edilebilir aralıkta.";
            }

            if (securityScore >= 60)
            {
                return "Orta seviye risk tespit edildi: kalıcı hale gelmeden ilgili göstergeler incelenmelidir.";
            }

            if (securityScore >= 40)
            {
                return "Yüksek risk göstergeleri mevcut: etkilenen alanlar öncelikli olarak incelenmelidir.";
            }

            return "Kritik sistem riski tespit edildi: acil inceleme önerilir.";
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
                You are explaining a Pardus/Linux security monitoring score to a Turkish administrator.
                Answer in Turkish. Keep the answer under 70 words, professional, clear, and action-oriented.
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
                Threat type: {currentData.ThreatType ?? "None"}
                Threat level: {currentData.ThreatLevel ?? "None"}
                Threat score: {currentData.ThreatScore}
                Threat evidence: {currentData.ThreatEvidence ?? "None"}
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

        private static string TranslateThreatLevel(string? threatLevel)
        {
            return threatLevel switch
            {
                "Critical" => "Kritik",
                "High" => "Yüksek",
                "Medium" => "Orta",
                "Low" => "Düşük",
                _ => "Yok"
            };
        }

        private sealed record OllamaRequest(
            [property: JsonPropertyName("model")] string Model,
            [property: JsonPropertyName("prompt")] string Prompt,
            [property: JsonPropertyName("stream")] bool Stream);

        private sealed record OllamaResponse(
            [property: JsonPropertyName("response")] string? Response);
    }
}
