# Forward Protocol Launches with Named Pipes

Superseded by [0007-use-cloud-only-report-task-transport.md](0007-use-cloud-only-report-task-transport.md).

When `gridreport://` is opened while the local helper is already running, the new process will forward the protocol URL to the existing process through a local named pipe and then exit. This preserves single-instance desktop behavior while allowing repeated protocol launches to create report tasks without losing the launch parameters.

**Considered Options**

- Allow multiple helper instances. Rejected because report windows, tray behavior, and Grid++Report COM components are easier to keep predictable in a single process.
- Use Windows messages. Rejected because named pipes provide a clearer and less UI-dependent local IPC channel for passing the protocol URL.
