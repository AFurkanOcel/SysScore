import os
import time
from typing import Any

import psutil
import requests


DEFAULT_API_URL = "http://localhost:5070/api/system-data"
DEFAULT_POLL_INTERVAL_SECONDS = 5
REQUEST_TIMEOUT_SECONDS = 5


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


def collect_system_data() -> dict[str, float | int]:
    return {
        "cpuUsage": psutil.cpu_percent(interval=1),
        "ramUsage": psutil.virtual_memory().percent,
        "diskUsage": psutil.disk_usage("/").percent,
        "processCount": len(psutil.pids()),
    }


def send_system_data(api_url: str, payload: dict[str, float | int]) -> None:
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

        time.sleep(poll_interval)


if __name__ == "__main__":
    try:
        run_agent()
    except KeyboardInterrupt:
        log("SysScore agent stopped by user.")
