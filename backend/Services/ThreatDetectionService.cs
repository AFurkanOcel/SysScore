using SysScore.Models;

namespace SysScore.Services
{
    public class ThreatDetectionService
    {
        private const string NetworkThreatType = "Port Scan / Worm-like Network Activity";
        private const string ExposedServiceThreatType = "Exposed Service Surface Increase";
        private const string NoThreatLevel = "None";

        public void Analyze(SystemData data, SystemData? previousData)
        {
            List<string> networkEvidence = new();
            List<string> exposureEvidence = new();
            int networkThreatScore = 0;
            int exposureThreatScore = 0;

            int connectionDelta = data.NetworkConnectionDelta;
            if (previousData is not null && connectionDelta == 0)
            {
                connectionDelta = data.NetworkConnectionCount - previousData.NetworkConnectionCount;
            }

            int listeningPortDelta = previousData is null
                ? 0
                : data.ListeningPortCount - previousData.ListeningPortCount;

            if (connectionDelta >= 30)
            {
                networkThreatScore += 20;
                networkEvidence.Add($"Ağ bağlantı sayısı kısa sürede {connectionDelta} arttı.");
            }
            else if (connectionDelta >= 15)
            {
                networkThreatScore += 10;
                networkEvidence.Add($"Ağ bağlantı sayısında olağandışı artış görüldü: +{connectionDelta}.");
            }

            if (data.SynSentConnectionCount >= 10)
            {
                networkThreatScore += 25;
                networkEvidence.Add($"SYN_SENT durumunda {data.SynSentConnectionCount} bağlantı gözlendi.");
            }

            if (data.TimeWaitConnectionCount >= 40)
            {
                networkThreatScore += 20;
                networkEvidence.Add($"Kısa ömürlü bağlantıları gösteren TIME_WAIT sayısı yüksek: {data.TimeWaitConnectionCount}.");
            }

            if (data.UniqueRemotePortCount >= 20)
            {
                networkThreatScore += 25;
                networkEvidence.Add($"Kısa aralıkta {data.UniqueRemotePortCount} farklı uzak porta bağlantı davranışı tespit edildi.");
            }
            else if (data.UniqueRemotePortCount >= 10)
            {
                networkThreatScore += 12;
                networkEvidence.Add($"Farklı uzak port sayısı normal seviyenin üzerinde: {data.UniqueRemotePortCount}.");
            }

            if (data.UniqueRemoteAddressCount >= 10)
            {
                networkThreatScore += 20;
                networkEvidence.Add($"Birden fazla uzak adrese bağlantı davranışı var: {data.UniqueRemoteAddressCount} hedef.");
            }

            if (data.NetworkConnectionCount >= 150)
            {
                networkThreatScore += 10;
                networkEvidence.Add($"Toplam ağ bağlantı sayısı yüksek: {data.NetworkConnectionCount}.");
            }

            if (data.ListeningPortCount >= 20 && listeningPortDelta >= 5)
            {
                exposureThreatScore += data.ListeningPortCount >= 25 && listeningPortDelta >= 8
                    ? 35
                    : 20;
                exposureEvidence.Add($"Dinleyen port sayısı önceki kayda göre belirgin arttı: +{listeningPortDelta}.");
                exposureEvidence.Add($"Toplam dinleyen port sayısı yüksek seviyeye ulaştı: {data.ListeningPortCount} port.");
            }

            if (previousData is not null &&
                previousData.ThreatScore >= 35 &&
                networkThreatScore >= 30 &&
                string.Equals(previousData.ThreatType, NetworkThreatType, StringComparison.OrdinalIgnoreCase))
            {
                networkThreatScore += 10;
                networkEvidence.Add("Benzer ağ tabanlı risk önceki kayıtta da devam ediyordu.");
            }

            if (networkThreatScore >= 15)
            {
                ApplyThreat(
                    data,
                    NetworkThreatType,
                    Math.Clamp(networkThreatScore, 0, 100),
                    networkEvidence,
                    new[]
                    {
                        "ss -tulpen ile aktif servisleri ve bağlantıları inceleyin.",
                        "Gereksiz açık portları kapatın veya firewall ile sınırlandırın.",
                        "Şüpheli uzak adresleri doğrulayıp gerekiyorsa ağ seviyesinde engelleyin.",
                        "Firewall durumunu kontrol edin: sudo ufw status"
                    });
                return;
            }

            if (exposureThreatScore >= 20)
            {
                ApplyThreat(
                    data,
                    ExposedServiceThreatType,
                    Math.Clamp(exposureThreatScore, 0, 45),
                    exposureEvidence,
                    new[]
                    {
                        "Yeni açılan servisleri ss -tulpen ile doğrulayın.",
                        "Gereksiz servisleri kapatın veya sadece gerekli arayüzlerde dinleyecek şekilde sınırlandırın.",
                        "Firewall kurallarını ve dışa açık portları gözden geçirin."
                    });
                return;
            }

            ApplyNoThreat(data);
        }

        private static void ApplyThreat(
            SystemData data,
            string threatType,
            int threatScore,
            IEnumerable<string> evidence,
            IEnumerable<string> recommendedActions)
        {
            data.ThreatScore = threatScore;
            data.ThreatLevel = GetThreatLevel(threatType, threatScore);

            if (data.ThreatLevel == NoThreatLevel)
            {
                ApplyNoThreat(data);
                return;
            }

            data.ThreatType = threatType;
            data.ThreatEvidence = string.Join(" | ", evidence);
            data.RecommendedActions = string.Join(" | ", recommendedActions);
            data.ThreatDetectedAt = data.Timestamp;
        }

        private static void ApplyNoThreat(SystemData data)
        {
            data.ThreatType = null;
            data.ThreatLevel = NoThreatLevel;
            data.ThreatScore = 0;
            data.ThreatEvidence = "Aktif ağ tabanlı saldırı davranışı tespit edilmedi.";
            data.RecommendedActions = "Normal izlemeye devam edin.";
            data.ThreatDetectedAt = null;
        }

        private static string GetThreatLevel(string threatType, int threatScore)
        {
            if (threatType == ExposedServiceThreatType)
            {
                return threatScore >= 35
                    ? "Medium"
                    : threatScore >= 20
                        ? "Low"
                        : NoThreatLevel;
            }

            if (threatScore >= 80)
            {
                return "Critical";
            }

            if (threatScore >= 60)
            {
                return "High";
            }

            if (threatScore >= 35)
            {
                return "Medium";
            }

            if (threatScore >= 15)
            {
                return "Low";
            }

            return NoThreatLevel;
        }
    }
}
