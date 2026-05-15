import os
import time
from datetime import datetime, timezone
from typing import Any

import psutil
import requests


DEFAULT_API_URL = "http://localhost:5070/api/system-data"
DEFAULT_POLL_INTERVAL_SECONDS = 5
REQUEST_TIMEOUT_SECONDS = 5
HIGH_PROCESS_CPU_THRESHOLD = 50.0
HIGH_PROCESS_MEMORY_THRESHOLD = 10.0
MAX_SCANNED_FILES_PER_LOCATION = 5000
MAX_REPORTED_UNNECESSARY_FILES = 40


def log(message: str) -> None:
    print(message, flush=True)


def get_api_url() -> str:
    return os.getenv("SYSSCORE_API_URL", DEFAULT_API_URL).strip() or DEFAULT_API_URL


def get_poll_interval() -> int:
    raw_value = os.getenv("SYSSCORE_POLL_INTERVAL_SECONDS")

    if raw_value is None:
        return DEFAULT_POLL_INTERVAL_SECONDS

    try:
        interval = int(raw_value)
    except ValueError:
        log(
            f"Invalid SYSSCORE_POLL_INTERVAL_SECONDS value '{raw_value}'. "
            f"Using default value: {DEFAULT_POLL_INTERVAL_SECONDS}s."
        )
        return DEFAULT_POLL_INTERVAL_SECONDS

    if interval <= 0:
        log(
            "SYSSCORE_POLL_INTERVAL_SECONDS must be greater than zero. "
            f"Using default value: {DEFAULT_POLL_INTERVAL_SECONDS}s."
        )
        return DEFAULT_POLL_INTERVAL_SECONDS

    return interval


def collect_system_data() -> dict[str, float | int | str]:
    disk = psutil.disk_usage("/")
    swap = psutil.swap_memory()
    boot_time = psutil.boot_time()
    network_metrics = collect_network_metrics()
    process_metrics = collect_process_metrics()
    storage_hygiene = collect_storage_hygiene()

    return {
        "cpuUsage": psutil.cpu_percent(interval=1),
        "ramUsage": psutil.virtual_memory().percent,
        "diskUsage": disk.percent,
        "swapUsage": swap.percent,
        "diskFreeGb": round(disk.free / (1024**3), 2),
        "processCount": len(psutil.pids()),
        "highCpuProcessCount": process_metrics["highCpuProcessCount"],
        "highMemoryProcessCount": process_metrics["highMemoryProcessCount"],
        "networkConnectionCount": network_metrics["networkConnectionCount"],
        "listeningPortCount": network_metrics["listeningPortCount"],
        "systemUptimeSeconds": int(time.time() - boot_time),
        "bootTime": datetime.fromtimestamp(boot_time, timezone.utc).isoformat(),
        "unnecessaryFileCount": storage_hygiene["unnecessaryFileCount"],
        "unnecessaryFileSizeMb": storage_hygiene["unnecessaryFileSizeMb"],
        "unnecessaryFileLocations": storage_hygiene["unnecessaryFileLocations"],
        "largestUnnecessaryFiles": storage_hygiene["largestUnnecessaryFiles"],
    }


def collect_process_metrics() -> dict[str, int]:
    high_cpu_process_count = 0
    high_memory_process_count = 0

    for process in psutil.process_iter(["cpu_percent", "memory_percent"]):
        try:
            cpu_percent = process.info.get("cpu_percent") or 0
            memory_percent = process.info.get("memory_percent") or 0

            if cpu_percent >= HIGH_PROCESS_CPU_THRESHOLD:
                high_cpu_process_count += 1

            if memory_percent >= HIGH_PROCESS_MEMORY_THRESHOLD:
                high_memory_process_count += 1
        except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
            continue

    return {
        "highCpuProcessCount": high_cpu_process_count,
        "highMemoryProcessCount": high_memory_process_count,
    }


def collect_network_metrics() -> dict[str, int]:
    try:
        connections = psutil.net_connections(kind="inet")
    except (psutil.AccessDenied, OSError):
        return {
            "networkConnectionCount": 0,
            "listeningPortCount": 0,
        }

    listening_ports = {
        connection.laddr.port
        for connection in connections
        if connection.status == psutil.CONN_LISTEN and connection.laddr
    }

    return {
        "networkConnectionCount": len(connections),
        "listeningPortCount": len(listening_ports),
    }


def collect_storage_hygiene() -> dict[str, int | float | str]:
    scan_locations = get_storage_hygiene_locations()
    total_count = 0
    total_size = 0
    largest_files: list[tuple[int, str]] = []

    for location in scan_locations:
        scanned_count = 0

        for root, _, files in os.walk(location, followlinks=False):
            for file_name in files:
                if scanned_count >= MAX_SCANNED_FILES_PER_LOCATION:
                    break

                file_path = os.path.join(root, file_name)

                try:
                    if os.path.islink(file_path):
                        continue

                    file_size = os.path.getsize(file_path)
                except OSError:
                    continue

                scanned_count += 1
                total_count += 1
                total_size += file_size
                largest_files.append((file_size, file_path))

            if scanned_count >= MAX_SCANNED_FILES_PER_LOCATION:
                break

    largest_files.sort(reverse=True)
    top_entries = [
        f"{round(size / (1024**2), 2)} MB {path}"
        for size, path in largest_files[:MAX_REPORTED_UNNECESSARY_FILES]
    ]

    return {
        "unnecessaryFileCount": total_count,
        "unnecessaryFileSizeMb": round(total_size / (1024**2), 2),
        "unnecessaryFileLocations": ", ".join(scan_locations),
        "largestUnnecessaryFiles": " | ".join(top_entries),
    }


def get_storage_hygiene_locations() -> list[str]:
    candidate_locations = [
        "/tmp",
        os.path.expanduser("~/.cache"),
        os.path.expanduser("~/.local/share/Trash/files"),
    ]

    return [
        location
        for location in candidate_locations
        if os.path.isdir(location)
    ]


def send_system_data(api_url: str, payload: dict[str, float | int | str]) -> None:
    response = requests.post(
        api_url,
        json=payload,
        timeout=REQUEST_TIMEOUT_SECONDS,
    )
    response.raise_for_status()

    response_data: dict[str, Any] = response.json()
    record_id = response_data.get("id", "unknown")
    security_score = response_data.get("securityScore", "unknown")

    log(
        f"Sent system data successfully. "
        f"RecordId={record_id}, SecurityScore={security_score}"
    )


def run_agent() -> None:
    api_url = get_api_url()
    poll_interval = get_poll_interval()

    log(f"SysScore agent started. ApiUrl={api_url}, PollInterval={poll_interval}s")

    while True:
        started_at = time.monotonic()

        try:
            payload = collect_system_data()
            send_system_data(api_url, payload)
        except requests.RequestException as error:
            log(f"Backend request failed: {error}")
        except psutil.Error as error:
            log(f"System data collection failed: {error}")
        except ValueError as error:
            log(f"Backend response could not be parsed: {error}")
        except Exception as error:
            log(f"Unexpected agent error: {error}")

        elapsed_seconds = time.monotonic() - started_at
        time.sleep(max(0, poll_interval - elapsed_seconds))


if __name__ == "__main__":
    try:
        run_agent()
    except KeyboardInterrupt:
        log("SysScore agent stopped by user.")
