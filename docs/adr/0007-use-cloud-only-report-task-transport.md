# Use Cloud-Only Report Task Transport

GridReportForm will be refactored into a cloud-only local report helper. URL protocol launch and local WebSocket server modes are removed from the active product surface so report task delivery has one path: the local helper connects outbound to the cloud server and receives cloud task messages there.

**Consequences**

The local helper no longer registers or handles the `gridreport://` URL protocol. Frontends should not connect to `ws://127.0.0.1` or send complete report task JSON directly to the helper.

The active cloud task commands are limited to `print` and controlled `preview`. Printer discovery is handled by printer-report messages, and template maintenance is handled through the Admin system.

This decision supersedes the earlier URL protocol and dual-transport decisions for the current implementation.
