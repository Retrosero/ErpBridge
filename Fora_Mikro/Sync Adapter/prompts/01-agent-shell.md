# 01 - Agent Shell Prompt

Create the initial .NET 8 Sync Adapter solution.

Required projects:

- `SyncAdapter.Agent`: Worker Service that can run as console and Windows Service.
- `SyncAdapter.Core`: domain models, interfaces, options and shared services.
- `SyncAdapter.Infrastructure`: SQLite, HTTP client, SQL Server, DPAPI, logging.
- `SyncAdapter.Ui`: small tray/WPF/WinUI settings and status app.
- `SyncAdapter.Tests`: unit tests.

Implement:

- Configuration loading with environment override support.
- Local SQLite database creation and migration table.
- Health status model: `Starting`, `Inactive`, `Active`, `Paused`, `Expired`, `Error`.
- Redacted logging pipeline.
- Support bundle skeleton with app version, redacted config summary, recent logs and health checks.

Acceptance:

- `dotnet test` passes.
- Agent starts in console mode.
- No secret is logged in sample config tests.
