# 09 - Data Retention Expiry Prompt

Implement expiry, retention and cleanup workers.

Workers:

- License expiry worker.
- Token cleanup worker.
- Job lease cleanup worker.
- Support bundle retention worker.
- Soft-delete retention reporter.

Rules:

- Expired tenant changes heartbeat behavior.
- Mobile sessions are revoked on expiry.
- Support bundles are deleted after retention.
- Audit events are not silently deleted.
- ERP records are never touched by SpAdmin.

Tests:

- Expired tenant marked correctly.
- Mobile sessions revoked.
- Job lease returned to pending.
- Support bundle metadata retained after file deletion according to policy.
