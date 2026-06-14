# Use the gridreport URL Protocol

Superseded by [0007-use-cloud-only-report-task-transport.md](0007-use-cloud-only-report-task-transport.md).

The local helper will be launched from the business system through the custom `gridreport://` URL protocol. The protocol should carry only a task identifier and validation metadata, while task details and results remain in backend APIs, because custom protocols are reliable for launching local desktop applications but are poor channels for large report templates or report data.

For the initial rollout, the protocol launch will not include dedicated security validation. This is an explicit early-stage trade-off to validate the launch and task execution flow first; it should not be treated as a durable security boundary.

**Consequences**

The installer should register the `gridreport` protocol with Windows. The application may detect missing registration and guide the user, but protocol registration should be owned by installation rather than silently rewritten on every launch.

The backend API base address should remain configurable in the local helper UI and persisted in `config.ini`. A simple connection test should be available so operators can validate the configured business system address before launching report tasks.
