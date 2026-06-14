# Add Local Device Discovery Service

The local helper will expose a read-only HTTP service bound to `127.0.0.1` so the web frontend can discover the Cloud Device identity and printer list of the machine it is running on. The frontend calls this local endpoint to learn the current machine's `deviceId`, `deviceName`, and local printer names, then uses that identity to submit report tasks through the cloud business API. Task delivery itself is unchanged: the cloud server still routes tasks to the helper over the cloud device WebSocket.

This is a narrow, explicit exception to the "frontends do not connect to local components" posture of [0007](0007-use-cloud-only-report-task-transport.md). "Do not connect directly" in 0007 means frontends do not open a WebSocket to the helper and do not send complete report task JSON to it. A read-only local HTTP query that returns device identity is a different concern: it has no side effects, carries no task payload, and does not bypass the cloud delivery or audit path.

The discovery service exists because the frontend has no other way to know which Cloud Device corresponds to the user's current machine without introducing a user-to-device binding model on the cloud side. Asking the cloud to resolve "the device for the current user" would require owning and maintaining that binding (which user, which tenant, shared kiosk machines), which is heavier than the problem deserves. The local query sidesteps the binding entirely: "the device for this machine" is answered by the machine itself.

The service is query-only by design. It will not accept report task submissions, printer commands, or any state-changing operation. All report task delivery continues to flow through the cloud device WebSocket so `system_cloud_task` remains the single audit source and the cloud server remains the single source of truth for pending tasks, as established in [0006](0006-add-cloud-report-task-transport.md).

**Considered Options**

- Bind the discovery service to a remote interface and let frontends reach it across the network. Rejected because it reintroduces an externally reachable listening port on the helper, which is exactly what cloud-only was meant to remove.
- Discover the device through the cloud by user-to-device binding. Rejected because it requires owning a binding model (user/tenant/device) that this project does not otherwise need, and it breaks down on shared machines.
- Probe a port pool from the frontend to locate a helper on a dynamic port. Rejected because the helper's listen port is fixed by default and only configurable as a deployment escape hatch, so a fixed default port is sufficient and a probe pool adds complexity for no benefit.

**Consequences**

The discovery service binds to `127.0.0.1` only and listens on a default port (`9294`) that the frontend can call directly. The listen port is configurable in `config.ini` as an escape hatch for deployments that cannot use the default; changing it is a coordinated deployment action, not a per-launch random value, so the frontend can rely on the configured port at runtime.

The service validates the request `Origin` against a configured allowlist (the business system's origin) and rejects all other callers. This keeps device identity from being harvested by arbitrary web pages opened in the user's browser. Binding to `127.0.0.1` ensures the service is not reachable from other machines. The allowlist defaults to `127.0.0.1` (localhost only); production deployments must add the business system origin. Matching is host-based, so a configured `127.0.0.1` matches any browser Origin on that host regardless of scheme or port, and the same applies to configured business system origins. Requests without an `Origin` header (non-browser tools such as curl) are still allowed, since they cannot come from a cross-origin web page.

The response carries `deviceId`, `deviceName`, the local printer name list, and optionally the current cloud WebSocket online state. It carries local printer names, not cloud `printerId` values, because the helper does not know the cloud-side primary keys; the frontend submits `printerName` alongside `deviceId` and the cloud resolves the printer, matching the `printerName` field already present on `CloudReportTask`.

The business system frontend is expected to be deployed over plain HTTP for the relevant environment, so browser mixed-content blocking does not apply to calls from the frontend to `http://127.0.0.1`. If the frontend is ever moved to HTTPS, this assumption must be revisited (browsers exempt `localhost` from mixed-content blocking but not reliably `127.0.0.1`).

The frontend must still handle the case where the helper is not running: the discovery call will fail, and the frontend should prompt the user to start the helper rather than silently failing to find a device.

This decision partially amends [0007](0007-use-cloud-only-report-task-transport.md): the prohibition on direct frontend-to-helper connections applies to task delivery channels (WebSocket and task JSON), not to this read-only local discovery query.
