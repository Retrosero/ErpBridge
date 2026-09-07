# 07 - Document Transformers Prompt

Implement JSON payload to Mikro document transformation.

Components:

- `DocumentEnvelope`
- `DocumentPayloadValidator`
- `TransformerRegistry`
- One transformer per document type.
- `MikroDocumentModel`

Supported initial types:

- `sales_order`
- `sales_invoice`
- `sales_dispatch`
- `collection`
- `new_customer`
- `visit`
- `customer_location`
- `day_opening`
- `day_closing`

Rules:

- Validate before opening SQL transaction.
- Required lookup values must be resolved before writing.
- Use explicit mappings for every Mikro field.
- Trim or reject values according to Mikro column length rules.
- Store original payload checksum for idempotency.

Tests:

- Valid payload transforms.
- Missing customer fails.
- Unknown stock fails.
- Unsupported payment type fails.
- Same payload generates same checksum.
