---
name: mikro-windows-sync-architect
description: Design and implement customer-installed Windows synchronization agents for Mikro ERP integrations.
---

# Mikro Windows Sync Architect

Use this skill when building the Sync Adapter Windows agent.

## Architecture

- Use a .NET 8 Windows Service for continuous sync.
- Use tray/WPF/WinUI for local configuration, status and support bundle export.
- Prefer pull-mode: the customer machine calls the central API and pulls pending work.
- Use SQLite for settings metadata, queue, mapping, checkpoint and logs.
- Store secrets with Windows DPAPI or Credential Manager.
- Run the local API on `127.0.0.1` by default.
- Use allowlisted entities and tables only.
- Treat the central API as the source of license, config, queue, ack and pushed ERP data.

## Modules

- `LicenseService`
- `SettingsService`
- `MikroDbConnector`
- `RemoteApiClient`
- `LocalApiHost`
- `SyncEngine`
- `QueueManager`
- `TransformerRegistry`
- `MappingService`
- `CheckpointService`
- `SchemaExplorer`
- `AuditLogger`

## Acceptance

- Invalid API keys prevent sync from starting.
- Mikro connection can be tested from UI.
- Pending jobs are pulled from central API.
- Failed jobs can be retried.
- Checkpoints only advance after successful batches.
- User can see current status, last sync time, failed records and support logs.
