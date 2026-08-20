#!/bin/bash
curl -v -X POST https://lisans.appsgo.cloud/api/v1/android/bootstrap \
  -H "Authorization: Bearer AK-fake1234567890" \
  -H "X-Tenant-Id: ed4b71de" \
  -H "Content-Type: application/json" \
  -d '{"tenant_id":"ed4b71de","api_key":"AK-fake1234567890","device_id":"test-dev-id","agent_version":"1.0.0"}'
