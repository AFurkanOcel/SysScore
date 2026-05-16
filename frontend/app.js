const API_BASE_URL = window.SYSSCORE_API_BASE_URL || "http://localhost:5070";
const REFRESH_INTERVAL_MS = 5000;
const HISTORY_LIMIT = 20;
const MAX_MONITORED_RECORDS = 100;

const RISK_SEVERITIES = {
  excellent: {
    label: "Excellent",
    caption: "Excellent posture",
    className: "risk-excellent",
    color: "#4ade80",
    backgroundColor: "rgba(74, 222, 128, 0.14)",
  },
  stable: {
    label: "Stable",
    caption: "Stable system state",
    className: "risk-stable",
    color: "#86d993",
    backgroundColor: "rgba(134, 217, 147, 0.14)",
  },
  moderate: {
    label: "Moderate Risk",
    caption: "Moderate risk detected",
    className: "risk-moderate",
    color: "#facc15",
    backgroundColor: "rgba(250, 204, 21, 0.14)",
  },
  high: {
    label: "High Risk",
    caption: "High risk requires review",
    className: "risk-high",
    color: "#fb923c",
    backgroundColor: "rgba(251, 146, 60, 0.14)",
  },
  critical: {
    label: "Critical",
    caption: "Critical risk condition",
    className: "risk-critical",
    color: "#ef4444",
    backgroundColor: "rgba(239, 68, 68, 0.14)",
  },
};

const RISK_CLASS_NAMES = Object.values(RISK_SEVERITIES).map((severity) => severity.className);

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
};

let resourceChart;
let scoreChart;
let refreshTimerId;
let isRefreshing = false;
let monitoredRecords = [];

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
  elements.connectionStatus.textContent = message;
  elements.connectionStatus.classList.toggle("online", isOnline);
  elements.connectionStatus.classList.toggle("offline", !isOnline);
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
    elements.scoreCaption.textContent = "Waiting for system data";
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
  elements.scoreCaption.textContent = severity.caption;
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
  elements.aiSeverity.textContent = severity?.label || "Waiting";
  elements.aiExplanation.textContent =
    latest?.explanation?.trim() || "No explanation is available for the latest record yet.";
}

function updateStorageHygiene(latest) {
  elements.unnecessaryFileCount.textContent = formatNumber(Number(latest?.unnecessaryFileCount));
  elements.unnecessaryFileSizeMb.textContent = formatMb(Number(latest?.unnecessaryFileSizeMb));
  elements.unnecessaryFileLocations.textContent =
    latest?.unnecessaryFileLocations || "No scan locations are available.";

  const entries = String(latest?.largestUnnecessaryFiles || "")
    .split("|")
    .map((entry) => entry.trim())
    .filter(Boolean);

  elements.largestUnnecessaryFiles.innerHTML = entries.length === 0
    ? "<li>No unnecessary file samples detected yet.</li>"
    : entries.map((entry) => `<li>${escapeHtml(entry)}</li>`).join("");
}

function updateRecordsTable(history) {
  const recentRecords = history.slice(0, MAX_MONITORED_RECORDS);

  if (recentRecords.length === 0) {
    elements.recordsTable.innerHTML =
      '<tr><td colspan="8" class="empty-state">No system records found</td></tr>';
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
          <td>${formatNumber(Number(record.listeningPortCount))}</td>
          <td>${formatMb(Number(record.unnecessaryFileSizeMb))}</td>
          <td class="score-cell ${severity?.className || ""}">
            ${formatNumber(Number(record.securityScore))}
            <span>${severity?.label || "Unknown"}</span>
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
        { label: "Disk", data: [], borderColor: "#f4c95d", backgroundColor: "rgba(244, 201, 93, 0.12)", tension: 0.35 },
        { label: "Swap", data: [], borderColor: "#ff6b6b", backgroundColor: "rgba(255, 107, 107, 0.12)", tension: 0.35 },
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
          label: "Security Score",
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
    updateStorageHygiene(latest);
    updateRecordsTable(liveHistory);
    updateCharts(liveHistory);

    elements.lastUpdated.textContent = `Updated ${formatTime(new Date())} | Records ${liveHistory.length}/${MAX_MONITORED_RECORDS}`;
    setConnectionStatus(true, "Online");
  } catch (error) {
    setConnectionStatus(false, "Offline");
    elements.lastUpdated.textContent = `Connection error: ${error.message}`;
  } finally {
    isRefreshing = false;
    scheduleRefresh();
  }
}

function scheduleRefresh() {
  clearTimeout(refreshTimerId);
  refreshTimerId = setTimeout(refreshDashboard, REFRESH_INTERVAL_MS);
}

createCharts();
refreshDashboard();
