from _socket_demo_common import SocketScenario, run_socket_scenario


if __name__ == "__main__":
    run_socket_scenario(
        SocketScenario(
            name="Medium worm-like network activity",
            expected_threat_type="Port Scan / Worm-like Network Activity",
            expected_severity="Medium",
            start_port=28100,
            port_count=45,
            connections_per_port=2,
            create_connections=True,
        )
    )
