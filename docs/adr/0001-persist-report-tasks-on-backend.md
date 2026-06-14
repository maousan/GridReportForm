# Persist Report Tasks on the Backend

Report tasks will be persisted by the business system backend instead of existing only in the local helper or in short-lived memory. This gives operators a durable audit and troubleshooting trail for protocol-launched preview, print, design, and printer-list actions, while keeping the local helper focused on executing tasks by ID and reporting results back. The audit trail is intentionally simple and should not become a complex task state machine.

**Considered Options**

- Keep task details only in the URL protocol launch. Rejected because URLs are a poor data channel and do not provide durable logs.
- Keep task state only in the local helper. Rejected because backend operators need centralized task history.

**Consequences**

The backend needs report task creation, lookup, and result recording APIs. The local helper should treat the backend as the source of truth for task details and task history. Implementations should record enough information for audit and troubleshooting without modelling every intermediate runtime state.
