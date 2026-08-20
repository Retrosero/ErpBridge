#!/bin/bash
curl -v -X POST https://lisans.appsgo.cloud/api/v1/android/bootstrap \
  -H "Authorization: Bearer 883fef5725a9126820b286666fc0ed3fa5fc4bb7201e682e" \
  -H "X-Tenant-Id: ed4b71de" \
  -H "Content-Type: application/json" \
  -d '{"tenant_id":"ed4b71de","api_key":"AK-883fef5725a9126820b286666fc0ed3fa5fc4bb7201e682e","device_id":"test-dev-id","agent_version":"1.0.0"}'
