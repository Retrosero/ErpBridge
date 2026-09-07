# 08 - Security Audit Redaction Prompt

Implement security hardening, audit logging and redaction.

Required:

- Audit middleware/service for admin mutations.
- Redaction library for logs and support bundle metadata.
- Secret scanner for uploaded support bundles.
- Correlation id propagation.
- Admin session security checks.

Rules:

- No mutation without audit event.
- Audit summaries must not include secrets.
- Support bundle download is audited.
- Failed login and role escalation events are audited.

Tests:

- API key redacted.
- Bearer token redacted.
- SQL password redacted.
- Mutation without audit fails test helper.
- Bundle download audit created.
