# Use GitHub Release Installer For Application Updates

GridReportForm will check GitHub Releases for application updates. A release asset represents a complete installer for the local helper, not an incremental patch or loose zip of application files.

When a newer release is available, the helper downloads the installer asset and starts that installer. The installer is responsible for replacing the installed application files.

**Consequences**

The application update flow stays separate from cloud report task delivery. GitHub Releases are the update source, while Cloud Transport remains responsible only for cloud report tasks.

The helper does not hot-swap its own executable or DLL files. This avoids file-locking, partial replacement, and inconsistent runtime state while the WinForms process is running.

Release assets must follow a predictable convention so the helper can find the intended installer asset.

GitHub Release tags use `vX.Y.Z` semantic versions, such as `v1.0.1`. The helper compares versions by `major.minor.patch`; a fourth assembly revision component is ignored for update ordering.

The installer asset name uses `ReportHelperSetup-{version}.exe`, for example `ReportHelperSetup-1.0.1.exe`. If the latest release does not contain the expected installer asset, the helper treats the release as unavailable for automatic update.

Automatic update checks are controlled by the existing `AutoUpdate` configuration. When enabled, the helper checks GitHub Releases once shortly after startup. A manual "check update" entry remains available even when automatic checks are disabled. Automatic checks only notify when an update is available; manual checks may also report that the current version is already latest.

The helper must ask the user before downloading and installing an available update. It does not perform silent automatic installation. If the user accepts, the helper downloads the installer, stops cloud connectivity, starts the installer, and exits the current process.

Initial installer validation is intentionally lightweight: the download must succeed, the downloaded file must be non-empty, the asset name must match the expected installer naming convention, and the asset URL must come from the selected GitHub Release asset. Cryptographic hash validation and code-signing enforcement are deferred.

The GitHub update source is fixed to the project repository `maousan/GridReportForm`. The helper resolves `https://github.com/maousan/GridReportForm/releases/latest` to the latest release tag and then derives the installer asset URL from the release tag and asset naming convention. It avoids the GitHub REST API for routine update checks so public clients are not blocked by anonymous REST API rate limits. The repository is not exposed as an end-user setting.

The manual update check entry is placed in the tray context menu. The compact main window remains focused on activation, cloud connection, settings, and printer management.
