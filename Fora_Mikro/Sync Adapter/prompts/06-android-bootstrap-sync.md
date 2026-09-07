# 06 - Android Bootstrap Sync Prompt

Implement Mikro-to-cloud bootstrap data sync for Android.

Entities:

- Customers
- Customer addresses
- Stocks
- Barcodes
- Prices
- Stock balances
- Warehouses
- Payment plans
- Cash accounts
- Bank accounts
- Sales representatives
- Mobile user settings

Rules:

- Agent reads Mikro using allowlisted queries.
- Agent uploads incremental batches to central API.
- Checkpoints advance only after central API confirms batch.
- Android reads only from central API.
- No SQL credentials leave customer machine.

Tests:

- Full bootstrap.
- Incremental changed records.
- Deleted/inactive record propagation.
- Failed upload does not advance checkpoint.
