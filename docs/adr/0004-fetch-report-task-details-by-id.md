# Fetch Report Task Details by ID

Superseded by [0007-use-cloud-only-report-task-transport.md](0007-use-cloud-only-report-task-transport.md).

The protocol URL will carry only a report task ID, and the local helper will fetch the full report task details from the backend before execution. The task detail response should include the task action, template URL, data URL, result URL, optional template save URL, and existing `extInfo` values so the local helper can reuse the current preview, print, and design workflows without putting large report data into the protocol URL.

**Consequences**

The initial task detail contract is `id`, `action`, `templateUrl`, `dataUrl`, `resultUrl`, `saveUrl`, and `extInfo`. The backend owns translating business concepts such as purchase orders into report templates, data URLs, and report parameters.
