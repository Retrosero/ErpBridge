import re
import os

# Fix SyncRepository
f = "app/src/main/java/com/example/data/SyncRepository.kt"
c = open(f).read()
c = c.replace('barcode = it.barkod ?: it.id,', 'barcode = it.barkod ?: it.id ?: "",')
c = c.replace('code = it.actualUrunKod ?: it.id,', 'code = it.actualUrunKod,')
open(f, "w").write(c)

# Fix ErpIntegrationScreen pushRes.requestId
f = "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt"
c = open(f).read()
c = c.replace('apiService.getPushStatus(pushRes.requestId)', 'apiService.getPushStatus(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="pushStatus", since=pushRes.requestId))')

# Also in ErpIntegrationScreen, there is apiService.getCariler(request) but request doesn't exist in 1075.
# Let's see: ErpIntegrationScreen.kt:1075:103 No value passed for parameter 'request'.
# The error was on apiService.getSyncStatus(...) probably. We need to check line 1075.
open(f, "w").write(c)

