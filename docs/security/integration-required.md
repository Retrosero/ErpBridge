# Integration-required release prerequisites

The automated suite intentionally skips the following checks until a dedicated,
non-production Mikro environment is supplied. A skipped check is not a passing
release gate.

## Mikro V15 and V16

Provide two isolated SQL Server databases with least-privilege credentials:

| Fixture | Required proof |
|---|---|
| Mikro V15 | connection detection, bootstrap read, sales-order header/line transaction, retry idempotency |
| Mikro V16 | connection detection, bootstrap read, sales-order header/line transaction, retry idempotency |

The fixture must use synthetic tenant, product, customer and financial data.
Never point integration environment variables at a customer or production Mikro
database. Configure only the documented `ERPBridge_*` integration variables,
run `ErpBridge.Erp.Mikro.Tests`, and retain the test result as the release
evidence.

## Release consequence

Changes touching the Mikro adapter, schema mapping, bootstrap reader or writer
cannot be promoted past their integration gate until both fixture rows pass.
