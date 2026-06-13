import socket
import threading
import time
from contextlib import closing


HOST = "127.0.0.1"
START_PORT = 23000
PORT_COUNT = 45
CONNECTIONS_PER_PORT = 2
HOLD_SECONDS = 25
CONNECT_TIMEOUT_SECONDS = 1


def log(message: str) -> None:
    print(message, flush=True)


def handle_client(connection: socket.socket) -> None:
    with closing(connection):
        time.sleep(HOLD_SECONDS)


def run_listener(server_socket: socket.socket) -> None:
    with closing(server_socket):
        while True:
            try:
                connection, _ = server_socket.accept()
            except OSError:
                return

            threading.Thread(target=handle_client, args=(connection,), daemon=True).start()


def open_demo_listeners() -> list[socket.socket]:
    listeners: list[socket.socket] = []

    for port in range(START_PORT, START_PORT + PORT_COUNT):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        try:
            server_socket.bind((HOST, port))
            server_socket.listen()
        except OSError:
            server_socket.close()
            continue

        listeners.append(server_socket)
        threading.Thread(target=run_listener, args=(server_socket,), daemon=True).start()

    return listeners


def open_demo_connections(listeners: list[socket.socket]) -> list[socket.socket]:
    clients: list[socket.socket] = []
    target_ports = [listener.getsockname()[1] for listener in listeners]

    for port in target_ports:
        for _ in range(CONNECTIONS_PER_PORT):
            client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            client_socket.settimeout(CONNECT_TIMEOUT_SECONDS)

            try:
                client_socket.connect((HOST, port))
            except OSError:
                client_socket.close()
                continue

            clients.append(client_socket)

    return clients


def close_sockets(sockets: list[socket.socket]) -> None:
    for item in sockets:
        try:
            item.close()
        except OSError:
            continue


def main() -> None:
    log("Starting safe worm-like network activity simulation.")
    log("This script only opens temporary localhost listeners and client connections.")

    listeners = open_demo_listeners()
    if not listeners:
        log("No demo listener could be opened. Try again after checking local port usage.")
        return

    clients = open_demo_connections(listeners)
    log(f"Opened {len(listeners)} temporary listening ports and {len(clients)} localhost connections.")
    log(f"Keep SysScore agent running. The activity will remain visible for about {HOLD_SECONDS} seconds.")

    try:
        time.sleep(HOLD_SECONDS)
    finally:
        close_sockets(clients)
        close_sockets(listeners)
        log("Demo network activity stopped safely.")


if __name__ == "__main__":
    main()
