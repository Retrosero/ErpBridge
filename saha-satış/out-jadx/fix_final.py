import re

# Fix BridgeSyncHelper
f = "app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"
c = open(f).read()
c = c.replace('val items = responseDef.body()!!.items', 'val items = responseDef.body()!!.items ?: emptyList()')
c = c.replace('val items = responseDef2.body()!!.items', 'val items = responseDef2.body()!!.items ?: emptyList()')
c = c.replace('apiService.getFaturaHareket(com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="faturaHareket", since=cari.erpKod))', 'apiService.getFaturaHareket(com.example.data.api.PullJobsRequest(tenant_id="", api_key="", device_id="", agent_version="", entity="faturaHareket", since=code))')
c = c.replace('log("Hata [$code]', '/* log removed */')
open(f, "w").write(c)

# Fix ErpIntegrationScreen
f = "app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt"
c = open(f).read()
c = c.replace('apiService.triggerSync(erp = "mikro", entity = null)', 'apiService.triggerSync(com.example.data.api.PullJobsRequest(tenant_id=tenantId, api_key=apiKey, device_id=deviceId, agent_version="v2.0", entity="trigger"))')
c = c.replace('if (items.isNotEmpty())', 'if (items?.isNotEmpty() == true)')
c = c.replace('for (item in items)', 'for (item in items ?: emptyList())')
open(f, "w").write(c)

# Fix SalesScreen
f = "app/src/main/java/com/example/ui/screens/SalesScreen.kt"
c = open(f).read()
c = c.replace('since=customer.id', 'since=customer?.id')
open(f, "w").write(c)

