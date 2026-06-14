from _socket_demo_common import SocketScenario, run_socket_scenario


if __name__ == "__main__":
    run_socket_scenario(
        SocketScenario(
            name="High worm-like network activity",
            expected_threat_type="Port Scan / Worm-like Network Activity",
            expected_severity="High",
            start_port=28200,
            port_count=100,
            connections_per_port=4,
            create_connections=True,
        )
    )
