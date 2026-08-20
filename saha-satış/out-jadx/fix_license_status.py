import re

f = "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt"
c = open(f).read()
c = c.replace('apiService.getLicenseStatus()', 'apiService.getLicenseStatus(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="licenseStatus"))')

open(f, "w").write(c)

