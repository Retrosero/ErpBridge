# 04 - Local Store Queue Prompt

Implement durable local queue, mapping and checkpoint storage.

SQLite tables:

- `sync_jobs`
- `job_attempts`
- `mappings`
- `checkpoints`
- `audit_logs`
- `schema_cache`

Rules:

- Remote jobs are saved locally before processing.
- Retry transient errors with exponential backoff.
- Data errors move to dead-letter.
- Mappings are saved only after Mikro transaction success.
- Checkpoints advance only after successful batch push or pull.

Tests:

- Job survives process restart.
- Retry increments attempt count.
- Dead-letter keeps actionable error.
- Duplicate external id resolves to existing mapping.
