import socket
import threading
import time
from dataclasses import dataclass
from typing import Iterable


HOST = "127.0.0.1"
HOLD_SECONDS = 75
CONNECT_TIMEOUT_SECONDS = 1


@dataclass(frozen=True)
class SocketScenario:
    name: str
    expected_threat_type: str
    expected_severity: str
    start_port: int
    port_count: int
    connections_per_port: int
    create_connections: bool
    hold_seconds: int = HOLD_SECONDS


def log(message: str) -> None:
    print(message, flush=True)


def _accept_loop(server_socket: socket.socket, accepted_connections: list[socket.socket]) -> None:
    while True:
        try:
            connection, _ = server_socket.accept()
        except OSError:
            return

        accepted_connections.append(connection)


def _open_listeners(scenario: SocketScenario, accepted_connections: list[socket.socket]) -> list[socket.socket]:
    listeners: list[socket.socket] = []

    for port in range(scenario.start_port, scenario.start_port + scenario.port_count):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        try:
            server_socket.bind((HOST, port))
            server_socket.listen()
        except OSError:
            server_socket.close()
            continue

        listeners.append(server_socket)
        threading.Thread(
            target=_accept_loop,
            args=(server_socket, accepted_connections),
            daemon=True,
        ).start()

    return listeners


def _open_connections(scenario: SocketScenario, listeners: Iterable[socket.socket]) -> list[socket.socket]:
    clients: list[socket.socket] = []

    if not scenario.create_connections:
        return clients

    target_ports = [listener.getsockname()[1] for listener in listeners]

    for port in target_ports:
        for _ in range(scenario.connections_per_port):
            client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            client_socket.settimeout(CONNECT_TIMEOUT_SECONDS)

            try:
                client_socket.connect((HOST, port))
            except OSError:
                client_socket.close()
                continue

            clients.append(client_socket)

    return clients


def _close_sockets(sockets: Iterable[socket.socket]) -> None:
    for item in sockets:
        try:
            item.close()
        except OSError:
            continue


def run_socket_scenario(scenario: SocketScenario) -> None:
    accepted_connections: list[socket.socket] = []
    listeners: list[socket.socket] = []
    clients: list[socket.socket] = []

    log(f"Starting scenario: {scenario.name}")
    log("Safety: localhost only, no file changes, no firewall changes, no external targets.")
    log(f"Expected threat type: {scenario.expected_threat_type}")
    log(f"Expected severity: {scenario.expected_severity}")
    log(f"Activity will stay active for {scenario.hold_seconds} seconds.")
    log("Keep the SysScore agent running in normal 60-second mode and wait for the next minute boundary.")

    try:
        listeners = _open_listeners(scenario, accepted_connections)
        if not listeners:
            log("No temporary listener could be opened. Check local port usage and try again.")
            return

        clients = _open_connections(scenario, listeners)
        log(f"Opened {len(listeners)} temporary localhost listeners.")
        log(f"Opened {len(clients)} temporary localhost client connections.")

        deadline = time.time() + scenario.hold_seconds
        while True:
            remaining_seconds = int(deadline - time.time())
            if remaining_seconds <= 0:
                break

            log(f"Scenario active. Remaining seconds: {remaining_seconds}")
            time.sleep(min(15, remaining_seconds))
    except KeyboardInterrupt:
        log("Scenario interrupted by user. Cleaning up sockets.")
    finally:
        _close_sockets(clients)
        _close_sockets(accepted_connections)
        _close_sockets(listeners)
        log("Scenario stopped safely.")
