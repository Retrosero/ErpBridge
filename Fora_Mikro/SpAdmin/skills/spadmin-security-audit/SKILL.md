---
name: spadmin-security-audit
description: Implement SpAdmin RBAC, 2FA, audit event, redaction, support bundle security and retention rules.
---

# SpAdmin Security Audit

Use this skill whenever implementing authentication, authorization, audit, redaction or support bundle flows.

## Rules

- RBAC is mandatory for every admin endpoint.
- SuperAdmin and Support require 2FA.
- Every admin mutation creates an audit event.
- Audit summaries must not contain secrets.
- Support bundles require explicit request, expiry and authorization.
- Server performs secret scan on support bundle upload.
- Support bundle download is audited.

## Required Tests

- Missing 2FA denied.
- Role policy denies unauthorized mutation.
- Mutation creates audit event.
- API key, bearer token, SQL password and connection string are redacted.
- Unauthorized support bundle upload/download denied.
