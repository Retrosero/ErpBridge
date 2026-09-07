# Snapshot chunking deliverable

## Changed files

- Added chunked/staged bootstrap snapshot entities and PostgreSQL migration.
- Added idempotent `/api/v1/bootstrap/upload/start`, `/chunks`, and `/complete` endpoints.
- Updated the agent HTTP client to upload full snapshots in bounded chunks while preserving the legacy path for incremental and partial merge uploads.
- Updated Android reads to use active snapshot chunks for metadata, paged sections, products, customers, price data, cash/bank data, stock movements, invoice movements, and addresses.
- Updated `siparis_cepte` product/customer synchronization to consume all pages and write each page to Room immediately.
- Added central API chunk upload coverage.

## Behavior

- Only one active snapshot exists per tenant.
- A staged snapshot is invisible until completion succeeds.
- Repeated start/chunk/complete requests are idempotent.
- Existing Android response fields remain unchanged.
- Legacy `bootstrap_packages` remains a read fallback for older agents and existing data.

## Verification

- `git diff --check` passed.
- Full .NET build/test could not run because the repository requires .NET SDK `10.0.302`, while the machine currently has only SDK `8.0.424` installed.
- Android Gradle verification remains pending after the .NET SDK issue is resolved and the mobile build environment is available.

## Operational migration

Run the Central API migration with the normal release command:

```text
ErpBridge.CentralApi --migrate
```

The migration creates the chunked tables and copies the newest legacy package per tenant into section rows before the new agent upload path is enabled.
