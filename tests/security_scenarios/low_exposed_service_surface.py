from _socket_demo_common import SocketScenario, run_socket_scenario


if __name__ == "__main__":
    run_socket_scenario(
        SocketScenario(
            name="Low exposed service surface increase",
            expected_threat_type="Exposed Service Surface Increase",
            expected_severity="Low",
            start_port=28400,
            port_count=20,
            connections_per_port=0,
            create_connections=False,
        )
    )
