# GridReportForm

GridReportForm is a local report helper that receives report tasks from a business system and opens the appropriate reporting workflow on the user's Windows machine.

## Language

**Report Task**:
A request from the cloud server for the local helper to perform one report-related action, such as previewing or printing.
_Avoid_: command, socket message

**Report Template**:
The report layout used by Grid++Report to render, preview, print, or edit a report.
_Avoid_: format, design file

**Report Data Source**:
The data supplied by the business system for a report task.
_Avoid_: source, payload

**Report Task Transport**:
The channel used by the cloud server to deliver report tasks to the local helper.
_Avoid_: command style, startup mode

**Cloud Transport**:
A report task transport where the local helper actively connects to a cloud server and receives report tasks over that outbound connection.
_Avoid_: local port forwarding, cloud callback

**Cloud Device**:
A registered local helper installation that can receive cloud report tasks and execute them on its Windows machine.
_Avoid_: printer, websocket client

**Cloud Printer**:
A cloud-managed printer entry that maps to one local printer on one cloud device.
_Avoid_: local printer name, global printer name

**Preview Task Success**:
A preview task is successful when the local helper opens the preview window for the requested report.
_Avoid_: printed, completed document lifecycle

**Cloud Server Address**:
The configurable WebSocket address used by the local helper to connect to the cloud server.
_Avoid_: local websocket port, business system address

**Application Update**:
A newer packaged version of the local report helper that can replace the currently installed helper.
_Avoid_: report task, cloud task
