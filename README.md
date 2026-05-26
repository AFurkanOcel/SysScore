<h1 align="center">SysScore</h1>

<p align="center">
AI-supported security scoring and monitoring platform for Pardus/Linux systems.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Python-Agent-blue"/>
  <img src="https://img.shields.io/badge/.NET-8.0-purple"/>
  <img src="https://img.shields.io/badge/Backend-ASP.NET%20Core%208-purple"/>
  <img src="https://img.shields.io/badge/ORM-Entity%20Framework%20Core-blue"/>
  <img src="https://img.shields.io/badge/API-REST-green"/>
  <img src="https://img.shields.io/badge/Database-SQL%20Server-red"/>
  <img src="https://img.shields.io/badge/Container-Docker-blue"/>
  <img src="https://img.shields.io/badge/Frontend-Chart.js-green"/>
  <img src="https://img.shields.io/badge/Platform-Pardus%20%2F%20Linux-lightgrey"/>
  <img src="https://img.shields.io/badge/AI-Fallback%20%2B%20Ollama-orange"/>
  <img src="https://img.shields.io/badge/License-MIT-brightgreen"/>
  <img src="https://img.shields.io/badge/Status-Completed-brightgreen"/>
</p>

---

## Project Overview

SysScore is a graduation project designed to collect system metrics from a Pardus/Linux machine, calculate a professional security score, store historical monitoring data, visualize system health in a live dashboard, and explain score changes with an AI-supported explanation module.

The platform follows an end-to-end monitoring flow:

```text
Python Agent -> ASP.NET Core Web API -> SQL Server -> Frontend Dashboard
                                      |
                                      -> AI / Rule-based Explanation
```

The system is built with a safe fallback-first design. Even if the optional local LLM integration is unavailable, the backend continues to calculate scores and generate deterministic rule-based explanations.

---

## Interface Preview

### Security Score & AI Explanation

The main dashboard panel displays the live security score, severity state, and AI-supported system explanation.

<img width="1040" height="375" alt="Security Score   AI Explanation" src="assets/screenshots/security-score-ai-explanation.png" />

---

### System Metrics Overview

Live monitoring metrics including CPU usage, RAM usage, disk usage, process count, swap usage, free disk space, listening ports, active network connections, high-resource processes, system uptime, and boot time information.

<img width="1039" height="329" alt="System Metrics Overview" src="assets/screenshots/system-metrics-overview.png" />

---

### Monitoring Charts & Recent Records

Real-time monitoring charts visualize resource usage and security score trends, while the Recent Records table stores the latest monitoring history.

<img width="1023" height="702" alt="Monitoring Charts   Recent Records" src="assets/screenshots/monitoring-charts-recent-records.png" />

---

### Storage Hygiene Analysis

Storage hygiene monitoring detects unnecessary files, cache/temp accumulation, and displays the largest files affecting storage cleanliness.

<img width="1028" height="459" alt="Storage Hygiene Analysis" src="assets/screenshots/storage-hygiene-analysis.png" />

---

## Key Features

### System Monitoring Agent

The Python agent collects live Linux system metrics using `psutil`:

* CPU usage
* RAM usage
* Disk usage
* Swap usage
* Disk free space
* Process count
* High CPU process count
* High memory process count
* Network connection count
* Listening port count
* System uptime
* Boot time
* Temporary/cache/trash file statistics
* Largest unnecessary file samples

The agent sends data to the backend through REST API calls at a configurable polling interval.

### Professional Security Scoring

The security score is no longer based only on CPU/RAM/Disk usage. SysScore uses a weighted, rule-based scoring model:

| Risk Area | Signals |
| --- | --- |
| Resource Pressure | CPU, RAM, disk, swap |
| Process Anomaly | Total process count, high CPU/memory process count |
| Network Exposure | Listening ports, active connections |
| Storage Hygiene | Temporary/cache/trash count and total size |
| Trend Risk | Sudden increase compared with previous records |
| Compound Risk | Multiple risky signals appearing together |
| Persistent Risk | Risky conditions continuing across consecutive records |

The score stays in the `0-100` range:

```text
SecurityScore = 100 - weightedRiskPenalty + stabilityAdjustment
```

The final value is clamped between `0` and `100`.

Current scoring behavior includes:

* **Resource pressure penalty:** CPU, RAM, disk and swap usage are evaluated with threshold-based weights.
* **Process anomaly penalty:** total process count, high CPU process count and high memory process count affect the score.
* **Network exposure penalty:** listening ports and active network connections are included.
* **Storage hygiene penalty:** temporary, cache and trash file count/size can reduce the score.
* **Trend penalty:** sudden RAM, process, port or unnecessary file growth compared with the previous record is penalized.
* **Compound risk penalty:** combined risks such as RAM + swap pressure, listening ports + active connections, or high CPU + high memory processes reduce the score more strongly.
* **Persistent risk penalty:** repeated high RAM, swap, listening port or storage hygiene risk across consecutive records adds a smaller extra penalty.
* **Stability bonus:** when critical metrics remain normal and no meaningful regression is detected, the model reduces unnecessary penalty so healthy systems are not over-punished.

This makes the score more explainable and closer to a real monitoring model while still remaining deterministic and easy to defend in an academic project.

### Risk Severity Levels

The numeric security score is mapped to a dashboard severity level:

| Score Range | Severity | Visualization Color | Meaning |
| --- | --- | --- | --- |
| `90-100` | Excellent | Bright Green | Strong and clean system posture |
| `75-89` | Stable | Soft Green | Normal and stable operating state |
| `60-74` | Moderate Risk | Yellow | Needs attention before becoming persistent |
| `40-59` | High Risk | Orange | Clear risk indicators requiring review |
| `0-39` | Critical | Red | Critical condition requiring immediate attention |

The severity is used consistently across the score panel, score ring, score text, security trend chart and recent record score cells.

### AI Explanation Module

Each system record receives an explanation.

Supported behavior:

* Deterministic fallback explanation
* Optional Ollama local LLM enhancement
* Safe fallback if Ollama is unavailable
* Score decrease interpretation
* Resource, process, network and storage hygiene explanations
* Severity-aware opening text
* Compound risk explanation
* Persistent risk explanation
* Safe explanation length limiting for database persistence

Example fallback explanation:

```text
High risk indicators are present: prioritize investigation of the affected areas.
Combined RAM and swap pressure suggests sustained memory stress rather than a short resource spike.
Network exposure is elevated because listening ports and active connections are both above the expected baseline.
```

### Live Dashboard

The frontend dashboard provides:

* Live security score panel
* AI explanation panel
* Severity-aware score ring, score text, score chart and table score cells
* Professional risk color mapping: Excellent, Stable, Moderate Risk, High Risk and Critical
* CPU, RAM, disk and process cards
* Swap, disk free, uptime and boot time cards
* Listening ports and network connection metrics
* High CPU / high memory process indicators
* Chart.js resource usage graph
* Chart.js security score trend graph
* Scrollable recent records table
* Storage Hygiene panel for unnecessary files
* Dark, responsive, security-themed UI

---

## Architecture

SysScore follows a modular and layered architecture that separates monitoring, processing, storage, and visualization responsibilities across independent components.
Although the system is not designed as a fully distributed microservice platform, its components are loosely coupled and communicate through REST-based interactions.

```mermaid
flowchart LR
    A[Python Agent] -->|POST /api/system-data| B[ASP.NET Core Web API]
    B --> C[ScoreService]
    B --> D[AIService]
    C --> E[(SQL Server)]
    D --> E
    E -->|GET latest/history| F[Frontend Dashboard]
    F -->|Chart.js| G[Live Charts]
```

### Data Flow

```mermaid
sequenceDiagram
    participant Agent as Python Agent
    participant API as ASP.NET Core API
    participant Score as ScoreService
    participant AI as AIService
    participant DB as SQL Server
    participant UI as Dashboard

    Agent->>API: POST system metrics
    API->>DB: Read previous record
    API->>Score: Calculate weighted security score
    API->>AI: Generate explanation
    API->>DB: Store enriched record
    UI->>API: GET latest / history
    API->>UI: Return live monitoring data
```

---

## Project Structure

```text
SysScore/
│── SysScore.sln
│── docker-compose.yml
│── README.md
│── LICENSE
├── agent/
│   ├── agent.py
│   └── requirements.txt
├── assets/
│   └── screenshots/
├── backend/
│   ├── Controllers/
│   │   └── SystemController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Models/
│   │   └── SystemData.cs
│   ├── Services/
│   │   ├── ScoreService.cs
│   │   └── AIService.cs
│   ├── Migrations/
│   ├── Program.cs
│   ├── appsettings.json
│   └── SysScore.csproj
└── frontend/
    ├── index.html
    ├── styles.css
    ├── app.js
    ├── server.js
    ├── package.json
    └── package-lock.json
```

---

## Technologies Used

| Category | Technology |
| --- | --- |
| Agent | Python |
| System Metrics | psutil |
| HTTP Client | requests |
| Backend | ASP.NET Core Web API |
| Runtime | .NET 8 |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| Container | Docker |
| Frontend | HTML, CSS, JavaScript |
| Charts | Chart.js |
| AI Explanation | Rule-based fallback, optional Ollama |
| License | MIT |

---

## API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/system-data` | Receives system metrics, calculates score, stores record |
| `GET` | `/api/system-data/latest` | Returns the latest monitoring record |
| `GET` | `/api/system-data/history` | Returns historical monitoring records |

Example payload:

```json
{
  "cpuUsage": 12.5,
  "ramUsage": 43.1,
  "diskUsage": 16.3,
  "swapUsage": 0.9,
  "diskFreeGb": 72.9,
  "processCount": 290,
  "highCpuProcessCount": 0,
  "highMemoryProcessCount": 0,
  "networkConnectionCount": 74,
  "listeningPortCount": 7,
  "systemUptimeSeconds": 14049,
  "bootTime": "2026-05-15T18:14:18Z",
  "unnecessaryFileCount": 5353,
  "unnecessaryFileSizeMb": 237.01,
  "unnecessaryFileLocations": "/tmp, ~/.cache, ~/.local/share/Trash/files",
  "largestUnnecessaryFiles": "10.6 MB /tmp/example.log"
}
```

---

## Installation and Running

### 1. Clone Repository

```bash
git clone https://github.com/AFurkanOcel/SysScore.git
cd SysScore
```

### 2. Start SQL Server

If Docker Compose is available:

```bash
docker compose up -d
```

If your system uses the legacy command:

```bash
docker-compose up -d
```

The SQL Server container uses:

```text
Server: localhost,1433
Database: SysScoreDb
User: sa
Password: SysScore_2026!
```

### 3. Apply Database Migrations

```bash
dotnet ef database update --project backend/SysScore.csproj --startup-project backend/SysScore.csproj
```

### 4. Run Backend API

```bash
dotnet run --project backend/SysScore.csproj --urls http://localhost:5070
```

Swagger:

```text
http://localhost:5070/swagger
```

### 5. Run Python Agent

Install Python dependencies:

```bash
pip install -r agent/requirements.txt
```

Run the agent:

```bash
python agent/agent.py
```

Optional configuration:

```bash
export SYSSCORE_API_URL=http://localhost:5070/api/system-data
export SYSSCORE_POLL_INTERVAL_SECONDS=5
python agent/agent.py
```

### 6. Run Frontend Dashboard

```bash
npm install --prefix frontend
npm start --prefix frontend
```

Dashboard:

```text
http://localhost:5173
```

---

## AI Configuration

Default configuration uses deterministic fallback explanations:

```json
"AI": {
  "UseOllama": false,
  "OllamaUrl": "http://localhost:11434/api/generate",
  "OllamaModel": "llama3.2",
  "TimeoutSeconds": 3
}
```

To enable optional Ollama support, set:

```json
"UseOllama": true
```

If Ollama is unavailable, SysScore continues to work with fallback explanations.

---

## Security and Safety Notes

* The unnecessary file monitoring feature does not delete files.
* The agent only scans limited locations: `/tmp`, `~/.cache`, and `~/.local/share/Trash/files`.
* Permission errors during scanning are ignored safely.
* AI explanation failure does not stop backend processing.
* The system is intended for local monitoring and academic demonstration.

---

## Future Improvements

* More advanced anomaly detection
* Process whitelist/blacklist support
* Open port risk classification
* Failed login monitoring
* Firewall status monitoring
* Package update and patch status monitoring
* Role-based dashboard authentication
* Exportable reports
* Production-ready secret management

---

## Learning Outcomes

This project demonstrates:

* Multi-component system architecture
* Python system monitoring
* ASP.NET Core Web API development
* Entity Framework Core migrations
* SQL Server persistence with Docker
* Real-time dashboard design
* Chart.js visualization
* Rule-based security scoring
* AI-supported explanation design
* Safe fallback engineering

---

## Author

**A. Furkan ÖCEL**

---

## License

This project is licensed under the terms included in the repository's `LICENSE` file.
