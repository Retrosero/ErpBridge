# 00 - Master Prompt

You are building `Sync Adapter`, a .NET 8 synchronization product for Mikro ERP and an Android field sales application.

Build decisions:

- Android communicates only with the central API using JSON.
- Customer machines run a .NET 8 Windows Service agent.
- The agent never requires inbound customer firewall ports.
- The agent pulls pending jobs from the central API over HTTPS.
- Mikro SQL writes happen only inside the customer-installed agent.
- License expiry stops sync and purges local cache/tokens/queues, but never deletes legal ERP records.
- Mikro V15 and V16 must both be supported behind an adapter abstraction.

Non-negotiable rules:

- No duplicate Mikro documents.
- No partial header-without-lines writes.
- No ack/checkpoint before durable local success and Mikro transaction success.
- No SQL string concatenation with user data.
- No secret values in logs, UI errors, support bundles, or crash reports.
- No arbitrary SQL endpoint.

Use the project docs and `AGENTS.md` as binding instructions.
