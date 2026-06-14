# Support URL Protocol and WebSocket Transports

Superseded by [0007-use-cloud-only-report-task-transport.md](0007-use-cloud-only-report-task-transport.md).

The local helper will support both URL protocol and WebSocket report task transports at the same time. URL protocol transport remains available for backend-persisted report tasks by ID, while WebSocket transport is restored as a direct frontend-to-helper channel where complete task JSON is sent over the socket and results are returned on the same connection.

WebSocket transport should have its own enable flag and port. It may auto-start with the application, defaulting to enabled on port `9293`, and WebSocket startup failure should not disable URL protocol transport.

**Consequences**

The two transports should share report task execution behavior but keep transport-specific input and output separate. URL protocol tasks fetch details from the backend and post results to backend URLs; WebSocket tasks receive complete task details from the frontend and send results through the socket.

WebSocket transport should keep the previous direct message contract: frontend messages use `cmd`, `template`, `source`, and `extInfo`; WebSocket responses use the existing `ticketId`, `success`, `code`, `state`, `type`, and `data` shape. Design saves should return to the originating transport only: WebSocket-originated design tasks send `save-report` through the socket, while URL protocol-originated design tasks save through `saveUrl`.

The main window should remain a single vertical settings surface. WebSocket configuration is added inline with an enable checkbox, port input, and start/stop button rather than restoring tabs.
