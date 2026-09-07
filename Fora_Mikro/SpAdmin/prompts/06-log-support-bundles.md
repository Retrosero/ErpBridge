# 06 - Log Support Bundles Prompt

Implement agent log summary and support bundle flow.

Endpoints:

- `POST /agent/v1/log-summaries`
- `GET /admin/v1/tenants/{tenantId}/logs/summary`
- `POST /admin/v1/tenants/{tenantId}/support-bundles/request`
- `POST /agent/v1/support-bundles/{requestId}`

Rules:

- Only summaries are sent continuously.
- Detailed bundles require an active request.
- Bundle upload must pass server-side secret scan.
- Bundle metadata is stored in SQL; file content is stored outside SQL.
- Download and request actions are audited.

Tests:

- Summary ingest updates counters.
- Unauthorized bundle upload rejected.
- Expired bundle request rejected.
- Secret-containing bundle is quarantined.
