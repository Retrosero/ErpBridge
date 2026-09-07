# 05 - Central API Contracts Prompt

Implement central API contracts for Android and Sync Adapter agent.

Mobile endpoints:

- `POST /mobile/v1/documents`
- `GET /mobile/v1/bootstrap`

Agent endpoints:

- `POST /agent/v1/activate`
- `POST /agent/v1/heartbeat`
- `GET /agent/v1/jobs`
- `POST /agent/v1/jobs/{jobId}/ack`

Rules:

- Tenant isolation is mandatory.
- Android payloads become queued jobs for the agent.
- Agent jobs are leased to one active device at a time.
- Ack must record Mikro evrak seri/sira and RECno/Guid.
- Expired tenants cannot receive new mobile documents.

Tests:

- Tenant cannot read another tenant's jobs.
- Expired license blocks mobile document acceptance.
- Ack completes job.
- Failed ack stores retryable or permanent error.
