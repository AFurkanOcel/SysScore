from _socket_demo_common import SocketScenario, run_socket_scenario


if __name__ == "__main__":
    run_socket_scenario(
        SocketScenario(
            name="Low worm-like network activity",
            expected_threat_type="Port Scan / Worm-like Network Activity",
            expected_severity="Low",
            start_port=28000,
            port_count=18,
            connections_per_port=1,
            create_connections=True,
        )
    )
