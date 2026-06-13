using SysScore.Models;

namespace SysScore.Services
{
    public class ThreatDetectionService
    {
        private const string NetworkThreatType = "Port Scan / Worm-like Network Activity";
        private const string NoThreatLevel = "None";

        public void Analyze(SystemData data, SystemData? previousData)
        {
            List<string> evidence = new();
            int threatScore = 0;

            int connectionDelta = data.NetworkConnectionDelta;
            if (previousData is not null && connectionDelta == 0)
            {
                connectionDelta = data.NetworkConnectionCount - previousData.NetworkConnectionCount;
            }

            if (connectionDelta >= 30)
            {
                threatScore += 20;
                evidence.Add($"Ağ bağlantı sayısı kısa sürede {connectionDelta} arttı.");
            }
            else if (connectionDelta >= 15)
            {
                threatScore += 10;
                evidence.Add($"Ağ bağlantı sayısında olağandışı artış görüldü: +{connectionDelta}.");
            }

            if (data.SynSentConnectionCount >= 10)
            {
                threatScore += 25;
                evidence.Add($"SYN_SENT durumunda {data.SynSentConnectionCount} bağlantı gözlendi.");
            }

            if (data.TimeWaitConnectionCount >= 40)
            {
                threatScore += 20;
                evidence.Add($"Kısa ömürlü bağlantıları gösteren TIME_WAIT sayısı yüksek: {data.TimeWaitConnectionCount}.");
            }

            if (data.UniqueRemotePortCount >= 20)
            {
                threatScore += 25;
                evidence.Add($"Kısa aralıkta {data.UniqueRemotePortCount} farklı uzak porta bağlantı davranışı tespit edildi.");
            }
            else if (data.UniqueRemotePortCount >= 10)
            {
                threatScore += 12;
                evidence.Add($"Farklı uzak port sayısı normal seviyenin üzerinde: {data.UniqueRemotePortCount}.");
            }

            if (data.UniqueRemoteAddressCount >= 10)
            {
                threatScore += 20;
                evidence.Add($"Birden fazla uzak adrese bağlantı davranışı var: {data.UniqueRemoteAddressCount} hedef.");
            }

            if (data.NetworkConnectionCount >= 150)
            {
                threatScore += 10;
                evidence.Add($"Toplam ağ bağlantı sayısı yüksek: {data.NetworkConnectionCount}.");
            }

            if (data.ListeningPortCount >= 20)
            {
                threatScore += 10;
                evidence.Add($"Dinleyen port sayısı genişledi: {data.ListeningPortCount} port.");
            }

            if (previousData is not null &&
                previousData.ThreatScore >= 35 &&
                threatScore >= 30)
            {
                threatScore += 10;
                evidence.Add("Benzer ağ tabanlı risk önceki kayıtta da devam ediyordu.");
            }

            data.ThreatScore = Math.Clamp(threatScore, 0, 100);
            data.ThreatLevel = GetThreatLevel(data.ThreatScore);

            if (data.ThreatLevel == NoThreatLevel)
            {
                data.ThreatType = null;
                data.ThreatEvidence = "Aktif ağ tabanlı saldırı davranışı tespit edilmedi.";
                data.RecommendedActions = "Normal izlemeye devam edin.";
                data.ThreatDetectedAt = null;
                return;
            }

            data.ThreatType = NetworkThreatType;
            data.ThreatEvidence = string.Join(" | ", evidence);
            data.RecommendedActions = string.Join(" | ", new[]
            {
                "ss -tulpen ile aktif servisleri ve bağlantıları inceleyin.",
                "Gereksiz açık portları kapatın veya firewall ile sınırlandırın.",
                "Şüpheli uzak adresleri doğrulayıp gerekiyorsa ağ seviyesinde engelleyin.",
                "Firewall durumunu kontrol edin: sudo ufw status"
            });
            data.ThreatDetectedAt = data.Timestamp;
        }

        private static string GetThreatLevel(int threatScore)
        {
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
