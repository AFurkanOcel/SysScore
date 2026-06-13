const API_BASE_URL = window.SYSSCORE_API_BASE_URL || "http://localhost:5070";
const REFRESH_INTERVAL_MS = 60000;
const HISTORY_LIMIT = 20;
const MAX_MONITORED_RECORDS = 100;
const DEFAULT_LANGUAGE = "tr";

const RISK_SEVERITIES = {
  excellent: {
    label: { tr: "Mükemmel", en: "Excellent" },
    caption: { tr: "Mükemmel güvenlik durumu", en: "Excellent posture" },
    className: "risk-excellent",
    color: "#4ade80",
    backgroundColor: "rgba(74, 222, 128, 0.14)",
  },
  stable: {
    label: { tr: "Stabil", en: "Stable" },
    caption: { tr: "Stabil sistem durumu", en: "Stable system state" },
    className: "risk-stable",
    color: "#86d993",
    backgroundColor: "rgba(134, 217, 147, 0.14)",
  },
  moderate: {
    label: { tr: "Orta Risk", en: "Moderate Risk" },
    caption: { tr: "Orta seviye risk tespit edildi", en: "Moderate risk detected" },
    className: "risk-moderate",
    color: "#facc15",
    backgroundColor: "rgba(250, 204, 21, 0.14)",
  },
  high: {
    label: { tr: "Yüksek Risk", en: "High Risk" },
    caption: { tr: "Yüksek risk inceleme gerektiriyor", en: "High risk requires review" },
    className: "risk-high",
    color: "#fb923c",
    backgroundColor: "rgba(251, 146, 60, 0.14)",
  },
  critical: {
    label: { tr: "Kritik", en: "Critical" },
    caption: { tr: "Kritik risk durumu", en: "Critical risk condition" },
    className: "risk-critical",
    color: "#ef4444",
    backgroundColor: "rgba(239, 68, 68, 0.14)",
  },
};

const RISK_CLASS_NAMES = Object.values(RISK_SEVERITIES).map((severity) => severity.className);

const THREAT_CLASS_NAMES = ["threat-none", "threat-low", "threat-medium", "threat-high", "threat-critical"];

const TRANSLATIONS = {
  tr: {
    eyebrow: "Pardus Güvenlik İzleme",
    appTitle: "SysScore Güvenlik Paneli",
    securityScore: "Güvenlik Skoru",
    waitingForData: "Sistem verisi bekleniyor",
    aiExplanationTitle: "Güvenlik Açıklaması",
    waitingExplanation: "Backend açıklaması bekleniyor.",
    noExplanation: "Son kayıt için açıklama henüz mevcut değil.",
    online: "Çevrimiçi",
    offline: "Çevrimdışı",
    updated: "Güncellendi",
    records: "Kayıt",
    connectionError: "Bağlantı hatası",
    threatStatusTitle: "Aktif Tehdit Durumu",
    noThreatHeadline: "Tehdit Yok",
    suspiciousHeadline: "Şüpheli Ağ Aktivitesi",
    highThreatHeadline: "Yüksek Riskli Ağ Davranışı",
    criticalThreatHeadline: "Kritik Tehdit Davranışı",
    threatType: "Tehdit Tipi",
    threatScore: "Tehdit Skoru",
    threatDetectedAt: "Son Tespit",
    threatEvidence: "Kanıtlar",
    recommendedActions: "Önerilen Müdahale",
    noThreatType: "Aktif tehdit yok",
    noThreatEvidence: "Aktif ağ tabanlı saldırı davranışı tespit edilmedi.",
    noThreatAction: "Normal izlemeye devam edin.",
    threatNone: "Yok",
    threatLow: "Düşük",
    threatMedium: "Orta",
    threatHigh: "Yüksek",
    threatCritical: "Kritik",
    resourceUsage: "Kaynak Kullanımı",
    resourceUsageSubtitle: "CPU / RAM / Disk",
    securityTrend: "Güvenlik Trendi",
    scoreHistory: "Skor geçmişi",
    recentRecords: "Son Kayıtlar",
    notUpdated: "Henüz güncellenmedi",
    tableTime: "Zaman",
    tableProcesses: "Process",
    tableThreat: "Tehdit",
    tablePorts: "Port",
    tableScore: "Skor",
    noRecords: "Sistem kaydı bulunamadı",
    waitingBackend: "Backend verisi bekleniyor",
    systemDetails: "Sistem Ayrıntıları",
    systemDetailsHint: "CPU, RAM, disk, process ve ağ metrikleri",
    cpuUsage: "CPU Kullanımı",
    ramUsage: "RAM Kullanımı",
    diskUsage: "Disk Kullanımı",
    processes: "Process",
    swapUsage: "Swap Kullanımı",
    diskFree: "Boş Disk",
    listeningPorts: "Dinleyen Portlar",
    connections: "Bağlantılar",
    highCpuProcesses: "Yüksek CPU Process",
    highMemoryProcesses: "Yüksek Bellek Process",
    systemUptime: "Sistem Çalışma Süresi",
    bootTime: "Açılış Zamanı",
    storageHygiene: "Depolama Hijyeni",
    storageSubtitle: "Geçici, cache ve çöp dosyası izleme",
    unnecessaryFiles: "Gereksiz Dosyalar",
    totalSize: "Toplam Boyut",
    noStorageLocations: "Tarama konumu mevcut değil.",
    scannedLocationsWaiting: "Taranan konumlar burada görünecek.",
    noUnnecessaryFiles: "Henüz gereksiz dosya örneği tespit edilmedi.",
    unknown: "Bilinmiyor",
  },
  en: {
    eyebrow: "Pardus Security Monitoring",
    appTitle: "SysScore Dashboard",
    securityScore: "Security Score",
    waitingForData: "Waiting for system data",
    aiExplanationTitle: "AI Explanation",
    waitingExplanation: "Waiting for backend explanation.",
    noExplanation: "No explanation is available for the latest record yet.",
    online: "Online",
    offline: "Offline",
    updated: "Updated",
    records: "Records",
    connectionError: "Connection error",
    threatStatusTitle: "Active Threat Status",
    noThreatHeadline: "No Threat",
    suspiciousHeadline: "Suspicious Network Activity",
    highThreatHeadline: "High-Risk Network Behavior",
    criticalThreatHeadline: "Critical Threat Behavior",
    threatType: "Threat Type",
    threatScore: "Threat Score",
    threatDetectedAt: "Last Detection",
    threatEvidence: "Evidence",
    recommendedActions: "Recommended Actions",
    noThreatType: "No active threat",
    noThreatEvidence: "No active network-based attack behavior detected.",
    noThreatAction: "Continue normal monitoring.",
    threatNone: "None",
    threatLow: "Low",
    threatMedium: "Medium",
    threatHigh: "High",
    threatCritical: "Critical",
    resourceUsage: "Resource Usage",
    resourceUsageSubtitle: "CPU / RAM / Disk",
    securityTrend: "Security Trend",
    scoreHistory: "Score history",
    recentRecords: "Recent Records",
    notUpdated: "Not updated yet",
    tableTime: "Time",
    tableProcesses: "Processes",
    tableThreat: "Threat",
    tablePorts: "Ports",
    tableScore: "Score",
    noRecords: "No system records found",
    waitingBackend: "Waiting for backend data",
    systemDetails: "System Details",
    systemDetailsHint: "CPU, RAM, disk, process and network metrics",
    cpuUsage: "CPU Usage",
    ramUsage: "RAM Usage",
    diskUsage: "Disk Usage",
    processes: "Processes",
    swapUsage: "Swap Usage",
    diskFree: "Disk Free",
    listeningPorts: "Listening Ports",
    connections: "Connections",
    highCpuProcesses: "High CPU Processes",
    highMemoryProcesses: "High Memory Processes",
    systemUptime: "System Uptime",
    bootTime: "Boot Time",
    storageHygiene: "Storage Hygiene",
    storageSubtitle: "Temporary, cache and trash file monitoring",
    unnecessaryFiles: "Unnecessary Files",
    totalSize: "Total Size",
    noStorageLocations: "No scan locations are available.",
    scannedLocationsWaiting: "Scanned locations will appear here.",
    noUnnecessaryFiles: "No unnecessary file samples detected yet.",
    unknown: "Unknown",
  },
};

const DYNAMIC_TEXT_TRANSLATIONS = [
  ["Mükemmel durum: sistem göstergeleri sağlıklı ve acil bir risk paterni tespit edilmedi.", "Excellent posture: system indicators are healthy and no immediate risk pattern was detected."],
  ["Sistem stabil görünüyor: izlenen göstergeler kabul edilebilir aralıkta.", "System appears stable: monitored indicators remain within acceptable ranges."],
  ["Orta seviye risk tespit edildi: kalıcı hale gelmeden ilgili göstergeler incelenmelidir.", "Moderate risk detected: the highlighted indicators should be reviewed before they become persistent."],
  ["Yüksek risk göstergeleri mevcut: etkilenen alanlar öncelikli olarak incelenmelidir.", "High risk indicators are present: affected areas should be reviewed first."],
  ["Kritik sistem riski tespit edildi: acil inceleme önerilir.", "Critical system risk detected: immediate review is recommended."],
  ["Güvenlik skoru önceki kayda göre düştü; sistemde artan risk göstergeleri incelenmelidir.", "The security score decreased compared with the previous record; increasing risk indicators should be reviewed."],
  ["Port Scan / Worm-like Network Activity tespit edildi.", "Port Scan / Worm-like Network Activity detected."],
  ["Aktif ağ tabanlı saldırı davranışı tespit edilmedi.", "No active network-based attack behavior detected."],
  ["Normal izlemeye devam edin.", "Continue normal monitoring."],
  ["ss -tulpen ile aktif servisleri ve bağlantıları inceleyin.", "Review active services and connections with ss -tulpen."],
  ["Gereksiz açık portları kapatın veya firewall ile sınırlandırın.", "Close unnecessary open ports or restrict them with a firewall."],
  ["Şüpheli uzak adresleri doğrulayıp gerekiyorsa ağ seviyesinde engelleyin.", "Validate suspicious remote addresses and block them at the network level if needed."],
  ["Firewall durumunu kontrol edin: sudo ufw status", "Check firewall status: sudo ufw status"],
  ["RAM ve swap kullanımının birlikte yükselmesi geçici bir yükten çok kalıcı bellek baskısına işaret edebilir.", "Combined RAM and swap pressure may indicate sustained memory stress rather than a temporary load spike."],
  ["Dinleyen port ve aktif bağlantı sayıları birlikte yüksek olduğu için ağ yüzeyi gözden geçirilmelidir.", "Listening ports and active connections are both high, so the network exposure surface should be reviewed."],
  ["Yüksek CPU ve yüksek bellek tüketen process'ler aynı anda görüldüğü için process davranışı incelenmelidir.", "High CPU and high memory processes are active at the same time, so process behavior should be reviewed."],
  ["Geçici, cache veya çöp dosyalarının boyutu ardışık kayıtlarda yüksek kaldığı için storage hygiene riski süreklilik gösteriyor.", "Temporary, cache, or trash file size remains high across records, indicating persistent storage hygiene risk."],
  ["Process sayısı ardışık kayıtlarda yüksek kaldığı için gereksiz arka plan süreçleri kontrol edilmelidir.", "Process count remains high across records, so unnecessary background processes should be reviewed."],
  ["CPU kullanımı yüksek; yoğun iş yükü veya olağandışı kaynak tüketen bir process olabilir.", "CPU usage is high; this may indicate heavy workload or an unusually resource-intensive process."],
  ["Bellek kullanımı yüksek ve devam ederse sistem kararlılığını azaltabilir.", "Memory usage is high and may reduce system stability if it continues."],
  ["Disk kullanımı yüksek; düşük boş alan loglama ve sistem güvenilirliğini etkileyebilir.", "Disk usage is high; low free space may affect logging and system reliability."],
  ["Swap kullanımı yüksek; bu durum RAM baskısının normal seviyeyi aştığını gösterebilir.", "Swap usage is high; this may indicate memory pressure beyond normal RAM usage."],
  ["Process sayısı beklenen seviyenin üzerinde ve incelenmelidir.", "Process count is above the expected baseline and should be reviewed."],
  ["Bir veya daha fazla process olağandışı CPU ya da bellek kullanıyor.", "One or more processes are using unusual CPU or memory resources."],
  ["Dinleyen port sayısı beklenenden yüksek; dışa açık servisler kontrol edilmelidir.", "The number of listening ports is higher than expected; exposed services should be reviewed."],
  ["Ağ bağlantı sayısı yüksek; yoğun servis aktivitesi veya şüpheli bağlantı davranışı olabilir.", "Network connection count is high; this may indicate heavy service activity or suspicious connection behavior."],
  ["Geçici, cache veya çöp dosyaları birikiyor; storage hygiene açısından gözden geçirilmelidir.", "Temporary, cache, or trash files are accumulating and should be reviewed for storage hygiene."],
  ["Dinleyen port sayısı önceki kayda göre arttı.", "Listening port count increased compared with the previous record."],
  ["Gereksiz dosya boyutu önceki kayda göre belirgin şekilde arttı.", "Unnecessary file size increased noticeably compared with the previous record."],
  ["Benzer ağ tabanlı risk önceki kayıtta da devam ediyordu.", "A similar network-based risk was also present in the previous record."],
];

const DYNAMIC_REGEX_TRANSLATIONS = [
  [/Tehdit seviyesi: Kritik, tehdit skoru: (\d+)\./g, "Threat level: Critical, threat score: $1."],
  [/Tehdit seviyesi: Yüksek, tehdit skoru: (\d+)\./g, "Threat level: High, threat score: $1."],
  [/Tehdit seviyesi: Orta, tehdit skoru: (\d+)\./g, "Threat level: Medium, threat score: $1."],
  [/Tehdit seviyesi: Düşük, tehdit skoru: (\d+)\./g, "Threat level: Low, threat score: $1."],
  [/Kanıtlar: /g, "Evidence: "],
  [/Önerilen müdahale: /g, "Recommended response: "],
  [/Ağ bağlantı sayısı kısa sürede (-?\d+) arttı\./g, "Network connection count increased by $1 in a short period."],
  [/Ağ bağlantı sayısında olağandışı artış görüldü: \+(-?\d+)\./g, "Unusual network connection growth was observed: +$1."],
  [/SYN_SENT durumunda (\d+) bağlantı gözlendi\./g, "$1 SYN_SENT connections were observed."],
  [/Kısa ömürlü bağlantıları gösteren TIME_WAIT sayısı yüksek: (\d+)\./g, "TIME_WAIT count is high, indicating many short-lived connections: $1."],
  [/Kısa aralıkta (\d+) farklı uzak porta bağlantı davranışı tespit edildi\./g, "Connections to $1 different remote ports were detected in a short interval."],
  [/Farklı uzak port sayısı normal seviyenin üzerinde: (\d+)\./g, "Unique remote port count is above the normal level: $1."],
  [/Birden fazla uzak adrese bağlantı davranışı var: (\d+) hedef\./g, "Connections to multiple remote addresses were observed: $1 targets."],
  [/Toplam ağ bağlantı sayısı yüksek: (\d+)\./g, "Total network connection count is high: $1."],
  [/Dinleyen port sayısı genişledi: (\d+) port\./g, "Listening port exposure increased: $1 ports."],
];

const elements = {
  connectionStatus: document.getElementById("connectionStatus"),
  scorePanel: document.getElementById("scorePanel"),
  securityScore: document.getElementById("securityScore"),
  scoreCaption: document.getElementById("scoreCaption"),
  scoreRing: document.getElementById("scoreRing"),
  scoreRingValue: document.getElementById("scoreRingValue"),
  cpuUsage: document.getElementById("cpuUsage"),
  ramUsage: document.getElementById("ramUsage"),
  diskUsage: document.getElementById("diskUsage"),
  processCount: document.getElementById("processCount"),
  swapUsage: document.getElementById("swapUsage"),
  diskFreeGb: document.getElementById("diskFreeGb"),
  listeningPortCount: document.getElementById("listeningPortCount"),
  networkConnectionCount: document.getElementById("networkConnectionCount"),
  highCpuProcessCount: document.getElementById("highCpuProcessCount"),
  highMemoryProcessCount: document.getElementById("highMemoryProcessCount"),
  systemUptime: document.getElementById("systemUptime"),
  bootTime: document.getElementById("bootTime"),
  unnecessaryFileCount: document.getElementById("unnecessaryFileCount"),
  unnecessaryFileSizeMb: document.getElementById("unnecessaryFileSizeMb"),
  unnecessaryFileLocations: document.getElementById("unnecessaryFileLocations"),
  largestUnnecessaryFiles: document.getElementById("largestUnnecessaryFiles"),
  recordsTable: document.getElementById("recordsTable"),
  lastUpdated: document.getElementById("lastUpdated"),
  aiSeverity: document.getElementById("aiSeverity"),
  aiExplanation: document.getElementById("aiExplanation"),
  languageButtons: document.querySelectorAll("[data-language]"),
  threatPanel: document.getElementById("threatPanel"),
  threatHeadline: document.getElementById("threatHeadline"),
  threatLevel: document.getElementById("threatLevel"),
  threatType: document.getElementById("threatType"),
  threatScore: document.getElementById("threatScore"),
  threatDetectedAt: document.getElementById("threatDetectedAt"),
  threatEvidenceList: document.getElementById("threatEvidenceList"),
  recommendedActionsList: document.getElementById("recommendedActionsList"),
};

let resourceChart;
let scoreChart;
let refreshTimerId;
let isRefreshing = false;
let monitoredRecords = [];
let currentLanguage = DEFAULT_LANGUAGE;

function formatPercent(value) {
  return Number.isFinite(value) ? `${value.toFixed(1)}%` : "--";
}

function formatNumber(value) {
  return Number.isFinite(value) ? value.toLocaleString("en-US") : "--";
}

function formatGb(value) {
  return Number.isFinite(value) ? `${value.toFixed(1)} GB` : "--";
}

function formatMb(value) {
  return Number.isFinite(value) ? `${value.toFixed(1)} MB` : "--";
}

function t(key) {
  return TRANSLATIONS[currentLanguage]?.[key] || TRANSLATIONS.en[key] || key;
}

function localized(value) {
  if (value && typeof value === "object") {
    return value[currentLanguage] || value.en || Object.values(value)[0];
  }

  return value;
}

function splitList(value, fallback) {
  const items = String(value || "")
    .split("|")
    .map((item) => item.trim())
    .filter(Boolean);

  return items.length > 0 ? items : [fallback];
}

function renderList(element, items) {
  element.innerHTML = items.map((item) => `<li>${escapeHtml(translateDynamicText(item))}</li>`).join("");
}

function translateDynamicText(value) {
  let text = String(value || "");

  if (currentLanguage !== "en") {
    return text;
  }

  for (const [source, target] of DYNAMIC_TEXT_TRANSLATIONS) {
    text = text.replaceAll(source, target);
  }

  for (const [pattern, target] of DYNAMIC_REGEX_TRANSLATIONS) {
    text = text.replace(pattern, target);
  }

  return text;
}

function translateThreatType(value) {
  if (!value) {
    return value;
  }

  if (currentLanguage === "en") {
    return value;
  }

  if (value === "Port Scan / Worm-like Network Activity") {
    return "Port Tarama / Solucan Benzeri Ağ Aktivitesi";
  }

  return value;
}

function formatDuration(seconds) {
  if (!Number.isFinite(seconds) || seconds <= 0) {
    return "--";
  }

  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);

  if (days > 0) {
    return `${days}d ${hours}h`;
  }

  return `${hours}h`;
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function formatTime(timestamp) {
  if (!timestamp) {
    return "--";
  }

  const parsedTimestamp = parseBackendTimestamp(timestamp);

  return new Intl.DateTimeFormat("en-US", {
    timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  }).format(parsedTimestamp);
}

function parseBackendTimestamp(timestamp) {
  if (timestamp instanceof Date) {
    return timestamp;
  }

  const value = String(timestamp);
  const hasTimeZone = /z$|[+-]\d{2}:\d{2}$/i.test(value);

  return new Date(hasTimeZone ? value : `${value}Z`);
}

function setConnectionStatus(isOnline, message) {
  elements.connectionStatus.textContent = message || (isOnline ? t("online") : t("offline"));
  elements.connectionStatus.classList.toggle("online", isOnline);
  elements.connectionStatus.classList.toggle("offline", !isOnline);
}

function applyLanguage(language) {
  currentLanguage = TRANSLATIONS[language] ? language : DEFAULT_LANGUAGE;
  document.documentElement.lang = currentLanguage;

  document.querySelectorAll("[data-i18n]").forEach((element) => {
    element.textContent = t(element.dataset.i18n);
  });

  elements.languageButtons.forEach((button) => {
    button.classList.toggle("active", button.dataset.language === currentLanguage);
  });

  if (scoreChart) {
    scoreChart.data.datasets[0].label = currentLanguage === "tr" ? "Güvenlik Skoru" : "Security Score";
    scoreChart.update("none");
  }

  if (resourceChart) {
    resourceChart.data.datasets[0].label = "CPU";
    resourceChart.data.datasets[1].label = currentLanguage === "tr" ? "RAM" : "RAM";
    resourceChart.data.datasets[2].label = currentLanguage === "tr" ? "Disk" : "Disk";
    resourceChart.data.datasets[3].label = "Swap";
    resourceChart.update("none");
  }

  const latest = monitoredRecords[0];
  updateScore(latest, monitoredRecords);
  updateExplanation(latest, monitoredRecords);
  updateThreatPanel(latest);
  updateRecordsTable(monitoredRecords);
}

function getRiskSeverity(score, latest, history) {
  if (!Number.isFinite(score)) {
    return null;
  }

  const riskKey = score >= 90
    ? "excellent"
    : score >= 75
      ? "stable"
      : score >= 60
        ? "moderate"
        : score >= 40
          ? "high"
          : "critical";

  return {
    ...RISK_SEVERITIES[riskKey],
    compoundRiskActive: hasCompoundRisk(latest),
    persistentRiskActive: hasPersistentRisk(latest, history),
  };
}

function hasCompoundRisk(latest) {
  if (!latest) {
    return false;
  }

  const ramUsage = Number(latest.ramUsage);
  const swapUsage = Number(latest.swapUsage);
  const listeningPortCount = Number(latest.listeningPortCount);
  const networkConnectionCount = Number(latest.networkConnectionCount);
  const highCpuProcessCount = Number(latest.highCpuProcessCount);
  const highMemoryProcessCount = Number(latest.highMemoryProcessCount);

  return (
    (ramUsage >= 80 && swapUsage >= 25) ||
    (listeningPortCount >= 12 && networkConnectionCount >= 120) ||
    (highCpuProcessCount > 0 && highMemoryProcessCount > 0)
  );
}

function hasPersistentRisk(latest, history) {
  if (!latest || !Array.isArray(history) || history.length < 2) {
    return false;
  }

  const previous = history.find((record) => getRecordKey(record) !== getRecordKey(latest));

  if (!previous) {
    return false;
  }

  return (
    (Number(latest.ramUsage) >= 80 && Number(previous.ramUsage) >= 80) ||
    (Number(latest.swapUsage) >= 25 && Number(previous.swapUsage) >= 25) ||
    (Number(latest.listeningPortCount) >= 12 && Number(previous.listeningPortCount) >= 12) ||
    (Number(latest.unnecessaryFileSizeMb) >= 1024 && Number(previous.unnecessaryFileSizeMb) >= 1024)
  );
}

function applyRiskClass(element, severity) {
  if (!element) {
    return;
  }

  element.classList.remove(...RISK_CLASS_NAMES);

  if (severity) {
    element.classList.add(severity.className);
  }
}

function updateScore(latest, history = monitoredRecords) {
  const score = Number(latest?.securityScore);

  if (!Number.isFinite(score)) {
    [elements.scorePanel, elements.securityScore, elements.scoreRing, elements.scoreRingValue, elements.scoreCaption].forEach(
      (element) => applyRiskClass(element, null),
    );
    elements.securityScore.textContent = "--";
    elements.scoreRingValue.textContent = "--";
    elements.scoreCaption.textContent = t("waitingForData");
    elements.scoreRing.style.background = "conic-gradient(#263241 0deg, #263241 0deg)";
    return;
  }

  const severity = getRiskSeverity(score, latest, history);
  const scoreDegrees = Math.max(0, Math.min(score, 100)) * 3.6;

  [elements.scorePanel, elements.securityScore, elements.scoreRing, elements.scoreRingValue, elements.scoreCaption].forEach(
    (element) => applyRiskClass(element, severity),
  );

  elements.securityScore.textContent = score;
  elements.scoreRingValue.textContent = score;
  elements.scoreCaption.textContent = localized(severity.caption);
  elements.scoreRing.style.background = `conic-gradient(${severity.color} ${scoreDegrees}deg, #263241 0deg)`;
}

function updateMetrics(latest) {
  elements.cpuUsage.textContent = formatPercent(Number(latest?.cpuUsage));
  elements.ramUsage.textContent = formatPercent(Number(latest?.ramUsage));
  elements.diskUsage.textContent = formatPercent(Number(latest?.diskUsage));
  elements.processCount.textContent = formatNumber(Number(latest?.processCount));
  elements.swapUsage.textContent = formatPercent(Number(latest?.swapUsage));
  elements.diskFreeGb.textContent = formatGb(Number(latest?.diskFreeGb));
  elements.listeningPortCount.textContent = formatNumber(Number(latest?.listeningPortCount));
  elements.networkConnectionCount.textContent = formatNumber(Number(latest?.networkConnectionCount));
  elements.highCpuProcessCount.textContent = formatNumber(Number(latest?.highCpuProcessCount));
  elements.highMemoryProcessCount.textContent = formatNumber(Number(latest?.highMemoryProcessCount));
  elements.systemUptime.textContent = formatDuration(Number(latest?.systemUptimeSeconds));
  elements.bootTime.textContent = latest?.bootTime ? formatTime(latest.bootTime) : "--";
}

function updateExplanation(latest, history = monitoredRecords) {
  const score = Number(latest?.securityScore);
  const severity = getRiskSeverity(score, latest, history);

  applyRiskClass(elements.aiSeverity, severity);
  elements.aiSeverity.textContent = severity ? localized(severity.label) : t("waitingForData");
  elements.aiExplanation.textContent =
    latest?.explanation?.trim() ? translateDynamicText(latest.explanation.trim()) : t("noExplanation");
}

function getThreatLevelKey(level) {
  return String(level || "None").toLowerCase();
}

function getThreatClass(level) {
  const key = getThreatLevelKey(level);

  if (key === "critical") {
    return "threat-critical";
  }

  if (key === "high") {
    return "threat-high";
  }

  if (key === "medium") {
    return "threat-medium";
  }

  if (key === "low") {
    return "threat-low";
  }

  return "threat-none";
}

function getThreatLabel(level) {
  const key = getThreatLevelKey(level);

  if (key === "critical") {
    return t("threatCritical");
  }

  if (key === "high") {
    return t("threatHigh");
  }

  if (key === "medium") {
    return t("threatMedium");
  }

  if (key === "low") {
    return t("threatLow");
  }

  return t("threatNone");
}

function getThreatHeadline(level) {
  const key = getThreatLevelKey(level);

  if (key === "critical") {
    return t("criticalThreatHeadline");
  }

  if (key === "high") {
    return t("highThreatHeadline");
  }

  if (key === "medium" || key === "low") {
    return t("suspiciousHeadline");
  }

  return t("noThreatHeadline");
}

function updateThreatPanel(latest) {
  const threatLevel = latest?.threatLevel || "None";
  const threatClass = getThreatClass(threatLevel);
  const hasThreat = threatClass !== "threat-none";

  elements.threatPanel.classList.remove(...THREAT_CLASS_NAMES);
  elements.threatPanel.classList.add(threatClass);
  elements.threatLevel.classList.remove(...THREAT_CLASS_NAMES);
  elements.threatLevel.classList.add(threatClass);

  elements.threatHeadline.textContent = getThreatHeadline(threatLevel);
  elements.threatLevel.textContent = getThreatLabel(threatLevel);
  elements.threatType.textContent = hasThreat ? translateThreatType(latest?.threatType) || t("unknown") : t("noThreatType");
  elements.threatScore.textContent = hasThreat ? formatNumber(Number(latest?.threatScore)) : "0";
  elements.threatDetectedAt.textContent = hasThreat && latest?.threatDetectedAt
    ? formatTime(latest.threatDetectedAt)
    : "--";

  renderList(
    elements.threatEvidenceList,
    splitList(latest?.threatEvidence, t("noThreatEvidence")),
  );
  renderList(
    elements.recommendedActionsList,
    splitList(latest?.recommendedActions, t("noThreatAction")),
  );
}

function updateStorageHygiene(latest) {
  elements.unnecessaryFileCount.textContent = formatNumber(Number(latest?.unnecessaryFileCount));
  elements.unnecessaryFileSizeMb.textContent = formatMb(Number(latest?.unnecessaryFileSizeMb));
  elements.unnecessaryFileLocations.textContent =
    latest?.unnecessaryFileLocations || t("noStorageLocations");

  const entries = String(latest?.largestUnnecessaryFiles || "")
    .split("|")
    .map((entry) => entry.trim())
    .filter(Boolean);

  elements.largestUnnecessaryFiles.innerHTML = entries.length === 0
    ? `<li>${t("noUnnecessaryFiles")}</li>`
    : entries.map((entry) => `<li>${escapeHtml(entry)}</li>`).join("");
}

function updateRecordsTable(history) {
  const recentRecords = history.slice(0, MAX_MONITORED_RECORDS);

  if (recentRecords.length === 0) {
    elements.recordsTable.innerHTML =
      `<tr><td colspan="8" class="empty-state">${t("noRecords")}</td></tr>`;
    return;
  }

  elements.recordsTable.innerHTML = recentRecords
    .map(
      (record) => {
        const severity = getRiskSeverity(Number(record.securityScore), record, recentRecords);

        return `
        <tr>
          <td>${formatTime(record.timestamp)}</td>
          <td>${formatPercent(Number(record.cpuUsage))}</td>
          <td>${formatPercent(Number(record.ramUsage))}</td>
          <td>${formatPercent(Number(record.diskUsage))}</td>
          <td>${formatNumber(Number(record.processCount))}</td>
          <td class="threat-cell ${getThreatClass(record.threatLevel)}">${getThreatLabel(record.threatLevel)}</td>
          <td>${formatNumber(Number(record.listeningPortCount))}</td>
          <td class="score-cell ${severity?.className || ""}">
            ${formatNumber(Number(record.securityScore))}
            <span>${severity ? localized(severity.label) : t("unknown")}</span>
          </td>
        </tr>
      `;
      },
    )
    .join("");
}

function createCharts() {
  const chartTextColor = "#95a4b5";
  const gridColor = "rgba(149, 164, 181, 0.14)";

  Chart.defaults.color = chartTextColor;
  Chart.defaults.font.family =
    'Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';

  resourceChart = new Chart(document.getElementById("resourceChart"), {
    type: "line",
    data: {
      labels: [],
      datasets: [
        { label: "CPU", data: [], borderColor: "#3dd6d0", backgroundColor: "rgba(61, 214, 208, 0.12)", tension: 0.35 },
        { label: "RAM", data: [], borderColor: "#76a9ff", backgroundColor: "rgba(118, 169, 255, 0.12)", tension: 0.35 },
        { label: "Disk", data: [], borderColor: "#facc15", backgroundColor: "rgba(250, 204, 21, 0.12)", tension: 0.35 },
        { label: "Swap", data: [], borderColor: "#ef4444", backgroundColor: "rgba(239, 68, 68, 0.12)", tension: 0.35 },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: { min: 0, max: 100, grid: { color: gridColor } },
        x: { grid: { color: gridColor } },
      },
      plugins: { legend: { labels: { boxWidth: 10, boxHeight: 10 } } },
    },
  });

  scoreChart = new Chart(document.getElementById("scoreChart"), {
    type: "line",
    data: {
      labels: [],
      datasets: [
        {
          label: currentLanguage === "tr" ? "Güvenlik Skoru" : "Security Score",
          data: [],
          borderColor: "#78db85",
          backgroundColor: "rgba(120, 219, 133, 0.14)",
          fill: true,
          tension: 0.35,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: { min: 0, max: 100, grid: { color: gridColor } },
        x: { grid: { color: gridColor } },
      },
    },
  });
}

function updateCharts(history) {
  const chartRecords = history.slice(0, HISTORY_LIMIT).reverse();
  const labels = chartRecords.map((record) => formatTime(record.timestamp));

  resourceChart.data.labels = labels;
  resourceChart.data.datasets[0].data = chartRecords.map((record) => Number(record.cpuUsage) || 0);
  resourceChart.data.datasets[1].data = chartRecords.map((record) => Number(record.ramUsage) || 0);
  resourceChart.data.datasets[2].data = chartRecords.map((record) => Number(record.diskUsage) || 0);
  resourceChart.data.datasets[3].data = chartRecords.map((record) => Number(record.swapUsage) || 0);
  resourceChart.update("none");

  scoreChart.data.labels = labels;
  scoreChart.data.datasets[0].data = chartRecords.map((record) => Number(record.securityScore) || 0);
  const latestChartRecord = history[0];
  const chartSeverity = getRiskSeverity(Number(latestChartRecord?.securityScore), latestChartRecord, history);

  if (chartSeverity) {
    scoreChart.data.datasets[0].borderColor = chartSeverity.color;
    scoreChart.data.datasets[0].backgroundColor = chartSeverity.backgroundColor;
  }

  scoreChart.update("none");
}

async function fetchJson(path) {
  const url = new URL(`${API_BASE_URL}${path}`);
  url.searchParams.set("_", Date.now().toString());

  const response = await fetch(url, {
    cache: "no-store",
    headers: {
      Accept: "application/json",
    },
  });

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`);
  }

  return response.json();
}

function normalizeHistory(history) {
  if (!Array.isArray(history)) {
    return [];
  }

  return [...history].sort(
    (first, second) =>
      parseBackendTimestamp(second.timestamp).getTime() -
      parseBackendTimestamp(first.timestamp).getTime(),
  );
}

function getRecordKey(record) {
  if (record?.id !== undefined && record?.id !== null) {
    return `id:${record.id}`;
  }

  return [
    "fallback",
    record?.timestamp ?? "",
    record?.cpuUsage ?? "",
    record?.ramUsage ?? "",
    record?.diskUsage ?? "",
    record?.processCount ?? "",
  ].join(":");
}

function mergeMonitoredRecords(latest, history) {
  const recordsByKey = new Map();

  for (const record of monitoredRecords) {
    recordsByKey.set(getRecordKey(record), record);
  }

  for (const record of normalizeHistory(history)) {
    recordsByKey.set(getRecordKey(record), record);
  }

  if (latest) {
    recordsByKey.set(getRecordKey(latest), latest);
  }

  monitoredRecords = normalizeHistory([...recordsByKey.values()]).slice(0, MAX_MONITORED_RECORDS);

  return monitoredRecords;
}

async function refreshDashboard() {
  if (isRefreshing) {
    return;
  }

  isRefreshing = true;

  try {
    const [latest, history] = await Promise.all([
      fetchJson("/api/system-data/latest"),
      fetchJson("/api/system-data/history"),
    ]);
    const liveHistory = mergeMonitoredRecords(latest, history);

    updateScore(latest, liveHistory);
    updateMetrics(latest);
    updateExplanation(latest, liveHistory);
    updateThreatPanel(latest);
    updateStorageHygiene(latest);
    updateRecordsTable(liveHistory);
    updateCharts(liveHistory);

    elements.lastUpdated.textContent = `${t("updated")} ${formatTime(new Date())} | ${t("records")} ${liveHistory.length}/${MAX_MONITORED_RECORDS}`;
    setConnectionStatus(true);
  } catch (error) {
    setConnectionStatus(false);
    elements.lastUpdated.textContent = `${t("connectionError")}: ${error.message}`;
  } finally {
    isRefreshing = false;
    scheduleRefresh();
  }
}

function scheduleRefresh() {
  clearTimeout(refreshTimerId);
  refreshTimerId = setTimeout(refreshDashboard, REFRESH_INTERVAL_MS);
}

elements.languageButtons.forEach((button) => {
  button.addEventListener("click", () => applyLanguage(button.dataset.language));
});

createCharts();
applyLanguage(DEFAULT_LANGUAGE);
refreshDashboard();
