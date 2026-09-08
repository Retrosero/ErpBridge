# Mobile outbound sync queue

## Changed files

- Added `mobile_sync_queue` PostgreSQL table and EF migration.
- Change-set ingest now atomically fans out invoice/collection ERP events.
- Added `GET /api/v1/android/sync/queue` with tenant isolation and cursor paging.
- Added Central API integration tests and documented the contract.
- Replaced the unavailable Mikro configuration binder call with equivalent explicit binding so the solution builds in locked restore environments.

## Verification

- `dotnet build ErpBridge.sln --no-restore` — passed, 0 warnings, 0 errors.
- `dotnet test ErpBridge.sln --no-build --no-restore` — passed, 0 failures; 20 live integration tests skipped because external ERP settings were not configured.
- `dotnet test tests/ErpBridge.CentralApi.Tests/ErpBridge.CentralApi.Tests.csproj --no-build --no-restore` — passed, 190 tests.
- `git diff --check` — passed.

## Runtime note

Apply the new EF migration to the central PostgreSQL database before enabling the Android queue consumer. The existing `jobs` table remains the central-to-Agent write queue.
